namespace Daria.V2.DataStore

open Daria.V2.DataStore.Common
open Daria.V2.DataStore.Persistence





[<RequireQualifiedAccess>]
module Series =

    open System
    open System.IO
    open Freql.Core.Common.Types
    open Freql.Sqlite
    open FsToolbox.Extensions.Streams
    open FsToolbox.Extensions.Strings
    open Daria.V2.DataStore.Persistence
    open Daria.V2.DataStore.Common
    open Daria.V2.DataStore.Models

    module private Internal =

        /// <summary>
        /// An internal record representing a series version.
        /// This contains the minimum data required for internal operations.
        /// </summary>
        type SeriesVersionListingItem =
            { Id: string
              Version: int
              DraftVersion: int option
              Hash: string
              Active: bool
              Draft: bool }

        let fetchTopLevelSeries (ctx: SqliteContext) =
            Operations.selectSeriesRecords ctx [ "WHERE parent_series_id IS NULL" ] []

        let fetchSeriesByParent (ctx: SqliteContext) (parentSeriesId: string) =
            Operations.selectSeriesRecords ctx [ "WHERE parent_series_id = @0" ] [ parentSeriesId ]

        /// <summary>
        /// This used a bespoke query bypassing `Operations` because the version blob is not needed.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="seriesId"></param>
        /// <param name="activeStatus"></param>
        /// <param name="draftStatus"></param>
        let fetchSeriesVersions
            (ctx: SqliteContext)
            (seriesId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =
            let sql =
                [ "SELECT id, series_id, version, draft_version, title, title_slug, description, hash, created_on, active FROM series_versions"
                  "WHERE series_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> () ]
                |> toSql

            ctx.SelectAnon<SeriesVersionOverview>(sql, [ seriesId ])

        let fetchSeriesVersionListings
            (ctx: SqliteContext)
            (seriesId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =
            let sql =
                [ "SELECT id, version, hash, active, draft FROM series_versions"
                  "WHERE series_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> () ]
                |> toSql

            ctx.SelectAnon<SeriesVersionOverview>(sql, [ seriesId ])

        let fetchLatestVersionListing
            (ctx: SqliteContext)
            (seriesId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =

            let sql =
                [ "SELECT id, version, active, draft FROM series_versions"
                  "WHERE series_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  "ORDER BY version DESC, draft_version DESC"
                  "LIMIT 1" ]
                |> toSql

            ctx.SelectSingleAnon<SeriesVersionListingItem>(sql, [ seriesId ])

        let deleteSeriesVersion (ctx: SqliteContext) (seriesVersionId: string) =
            ctx.ExecuteVerbatimNonQueryAnon("DELETE FROM series_versions WHERE id = @0", [ seriesVersionId ])
            |> ignore

    open Internal

    let rec fetchSeriesVersionOverviews (ctx: SqliteContext) (seriesId: string) = Internal.fetchSeriesVersions

    let exists (ctx: SqliteContext) (seriesId: string) =
        Operations.selectSeriesRecord ctx [ "WHERE id = @0;" ] [ seriesId ]
        |> Option.isSome

    let list (ctx: SqliteContext) =
        let rec build (series: Records.Series list) =
            series
            |> List.map (fun s ->
                ({ Id = s.Id
                   Name = s.Name
                   Order = s.SeriesOrder
                   CreatedOn = s.CreatedOn
                   Active = s.Active
                   Children = Internal.fetchSeriesByParent ctx s.Id |> build
                   Versions = Internal.fetchSeriesVersions ctx s.Id ActiveStatus.All DraftStatus.All }
                : SeriesListingItem))

        Internal.fetchTopLevelSeries ctx |> build

    let fetchLatestVersion (ctx: SqliteContext) (seriesId: string) (includeDrafts: bool) =
        Operations.selectSeriesVersionRecord
            ctx
            [ "WHERE series_id = @0"
              match includeDrafts with
              | true -> ()
              | false -> "AND draft = 0"
              "ORDER BY version DESC"
              "LIMIT 1" ]
            [ seriesId ]

    let deleteLatestDraft (ctx: SqliteContext) (seriesId: string) =
        match
            Internal.fetchLatestVersionListing ctx seriesId ActiveStatus.Active DraftStatus.Draft,
            Internal.fetchLatestVersionListing ctx seriesId ActiveStatus.Active DraftStatus.NotDraft
        with
        | Some dv, Some ndv ->
            // Check if the latest draft version is the same or high than the latest non draft version.
            // This is to ensure old draft versions are not removed.
            match dv.Version >= ndv.Version with
            | true -> Internal.deleteSeriesVersion ctx dv.Id
            | false -> ()
        | Some dv, None -> Internal.deleteSeriesVersion ctx dv.Id
        | None, _ -> ()



    /// <summary>
    /// Add a new draft series version to the store.
    /// This will check if the previous draft version matches the new one.
    /// If so it no new draft version will be added unless the force parameter is true.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="newVersion"></param>
    /// <param name="force">Skip the diff check and add the new draft version. This can be useful if the check is handled externally.</param>
    let addDraftVersion (ctx: SqliteContext) (newVersion: NewSeriesVersion) (force: bool) =
        ctx.ExecuteInTransactionV2(fun t ->
            let (ms, hash) =
                match newVersion.IndexBlob with
                | Blob.Prepared(memoryStream, hash) -> memoryStream, hash
                | Blob.Stream stream ->
                    let ms = stream |> toMemoryStream
                    ms, ms.GetSHA256Hash()
                | Blob.Text t ->
                    use ms = new MemoryStream(t.ToUtf8Bytes())
                    ms, ms.GetSHA256Hash()
                | Blob.Bytes b ->
                    use ms = new MemoryStream(b)
                    ms, ms.GetSHA256Hash()

            (*
            let version, draftVersion, prevHash =
                match
                    Internal.fetchLatestVersionListing t newVersion.SeriesId ActiveStatus.Active DraftStatus.Draft,
                    Internal.fetchLatestVersionListing t newVersion.SeriesId ActiveStatus.Active DraftStatus.NotDraft
                with
                | Some dv, Some ndv ->
                    // TODO this can be simplified?
                    // Fetch the latest version (draft or non draft) 
                    
                    // Check if the latest draft version is the same or high than the latest non draft version.
                    // This is to ensure old draft versions are not removed.
                    match dv.Version >= ndv.Version with
                    | true -> dv.Version, dv.DraftVersion |> Option.map ((+) 1), Some dv.Hash
                    | false -> ndv.Version + 1, Some 1, None
                | Some dv, None -> dv.Version, dv.DraftVersion |> Option.map ((+) 1), Some dv.Hash
                | None, Some ndv -> ndv.Version + 1, Some 1, None
                | None, None -> 1, Some 1, None
            *)

            // Check if the previous version is a draft (or exists).
            // If it was use it's version number and increment the draft version.
            // If it wasn't increment the version number and reset the draft number.
            // If it doesn't exist start at the beginning.
            let version, draftVersion, prevHash =
                match fetchLatestVersionListing t newVersion.SeriesId ActiveStatus.Active DraftStatus.All with
                | Some pv ->
                    match pv.DraftVersion with
                    | Some dv -> pv.Version, dv + 1, Some pv.Hash
                    | None -> pv.Version + 1, 1, Some pv.Hash
                | None -> 1, 1, None

            match force || compareHashes prevHash hash with
            | true ->
                let ivi =
                    newVersion.ImageVersion
                    |> Option.bind (function
                        | RelatedEntity.Specified id -> Some id
                        | RelatedEntity.Lookup version ->
                            match version with
                            | Specific(id, version) -> Images.Internal.getSpecificVersion t id version
                            | Latest id -> Images.Internal.getLatestVersion t id
                            |> Option.map (fun i -> i.Id)
                        | RelatedEntity.Bespoke fn -> fn ctx)

                let id = newVersion.Id.ToString()

                ({ Id = id
                   SeriesId = newVersion.SeriesId
                   Version = version
                   DraftVersion = Some draftVersion
                   Title = newVersion.Title
                   TitleSlug =
                     newVersion.TitleSlug
                     |> Option.defaultWith (fun _ -> newVersion.Title |> slugify)
                   Description = newVersion.Description
                   IndexBlob = BlobField.FromStream ms
                   Hash = hash
                   ImageVersionId = ivi
                   CreatedOn = newVersion.CreatedOn |> Option.defaultValue DateTime.UtcNow
                   Active = true }
                : Parameters.NewSeriesVersion)
                |> Operations.insertSeriesVersion t

                AddVersionResult.Success id
            | false -> AddVersionResult.NotChange
            |> Ok)
        |> function
            | Ok r -> r
            | Error e -> AddVersionResult.Failure(e, None)

    /// <summary>
    /// A new series version to the store.
    /// This will check the new version against the previous version and if there are no changes no new version will be added,
    /// unless the force parameter is true.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="newVersion"></param>
    /// <param name="force"></param>
    let addVersion (ctx: SqliteContext) (newVersion: NewSeriesVersion) (force: bool) =
        ctx.ExecuteInTransactionV2(fun t ->
            let (ms, hash) =
                match newVersion.IndexBlob with
                | Blob.Prepared(memoryStream, hash) -> memoryStream, hash
                | Blob.Stream stream ->
                    let ms = stream |> toMemoryStream
                    ms, ms.GetSHA256Hash()
                | Blob.Text t ->
                    use ms = new MemoryStream(t.ToUtf8Bytes())
                    ms, ms.GetSHA256Hash()
                | Blob.Bytes b ->
                    use ms = new MemoryStream(b)
                    ms, ms.GetSHA256Hash()

            let version, prevHash =
                match
                    Internal.fetchLatestVersionListing t newVersion.SeriesId ActiveStatus.Active DraftStatus.NotDraft
                with
                | Some pv ->
                    match pv.DraftVersion with
                    | Some _ ->
                        // Return the previous version's version because it was a draft so no need to increment it.
                        // Return None as the hash because the previous version was a draft, so this version should be added even if they match.
                        // This currently doesn't check the last non draft version. It could do but this can be added later.
                        pv.Version, None
                    | None -> pv.Version + 1, Some pv.Hash
                | None -> 1, None

            match force || compareHashes prevHash hash with
            | true ->
                let ivi =
                    newVersion.ImageVersion
                    |> Option.bind (function
                        | RelatedEntity.Specified id -> Some id
                        | RelatedEntity.Lookup version ->
                            match version with
                            | Specific(id, version) -> Images.Internal.getSpecificVersion t id version
                            | Latest id -> Images.Internal.getLatestVersion t id
                            |> Option.map (fun i -> i.Id)
                        | RelatedEntity.Bespoke fn -> fn ctx)

                let id = newVersion.Id.ToString()

                ({ Id = id
                   SeriesId = newVersion.SeriesId
                   Version = version
                   DraftVersion = None
                   Title = newVersion.Title
                   TitleSlug =
                     newVersion.TitleSlug
                     |> Option.defaultWith (fun _ -> newVersion.Title |> slugify)
                   Description = newVersion.Description
                   IndexBlob = BlobField.FromStream ms
                   Hash = hash
                   ImageVersionId = ivi
                   CreatedOn = newVersion.CreatedOn |> Option.defaultValue DateTime.UtcNow
                   Active = true }
                : Parameters.NewSeriesVersion)
                |> Operations.insertSeriesVersion t

                AddVersionResult.Success id
            | false -> AddVersionResult.NotChange
            |> Ok)
        |> function
            | Ok r -> r
            | Error e -> AddVersionResult.Failure(e, None)
