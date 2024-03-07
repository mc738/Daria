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
                [ "SELECT id, series_id, version, title, title_slug, description, hash, created_on, active, draft FROM series_versions"
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
                [ "SELECT id, version, active, draft FROM series_versions"
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
                  "ORDER BY version DESC"
                  "LIMIT 1" ]
                |> toSql

            ctx.SelectSingleAnon<SeriesVersionListingItem>(sql, [ seriesId ])

        let deleteSeriesVersion (ctx: SqliteContext) (seriesVersionId: string) =
            ctx.ExecuteVerbatimNonQueryAnon("DELETE FROM series_versions WHERE id = @0", [ seriesVersionId ])
            |> ignore

    let rec fetchSeriesVersionOverviews (ctx: SqliteContext) (seriesId: string) = Internal.fetchSeriesVersions

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

    let addOrReplaceDraftVersion (ctx: SqliteContext) (newVersion: NewSeriesVersion) =
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
        
        
        
        let version =
            match
                Internal.fetchLatestVersionListing ctx newVersion.SeriesId ActiveStatus.Active DraftStatus.Draft,
                Internal.fetchLatestVersionListing ctx newVersion.SeriesId ActiveStatus.Active DraftStatus.NotDraft
            with
            | Some dv, Some ndv ->
                // Check if the latest draft version is the same or high than the latest non draft version.
                // This is to ensure old draft versions are not removed.
                match dv.Version >= ndv.Version with
                | true -> Internal.deleteSeriesVersion ctx dv.Id; dv.Version
                | false -> ndv.Version + 1
            | Some dv, None -> Internal.deleteSeriesVersion ctx dv.Id; dv.Version
            | None, Some ndv -> ndv.Version + 1
            | None, None -> 1
        
        let ivi =
            newVersion.ImageVersion
            |> Option.bind (function
                | RelatedEntity.Specified id -> Some id
                | RelatedEntity.Lookup version ->
                    match version with
                    | Specific(id, version) -> Images.Internal.getSpecificVersion ctx id version
                    | Latest id -> Images.Internal.getLatestVersion ctx id
                    |> Option.map (fun i -> i.Id)
                | RelatedEntity.Bespoke fn -> fn ctx)


        ({ Id = newVersion.Id.ToString()
           SeriesId = newVersion.SeriesId
           Version = version
           Title = newVersion.Title
           TitleSlug =
             newVersion.TitleSlug
             |> Option.defaultWith (fun _ -> newVersion.Title |> slugify)
           Description = newVersion.Description
           IndexBlob = BlobField.FromStream ms
           Hash = hash
           ImageVersionId = ivi
           CreatedOn = newVersion.CreatedOn |> Option.defaultValue DateTime.UtcNow
           Active = true
           Draft = true }
        : Parameters.NewSeriesVersion)
        |> Operations.insertSeriesVersion ctx

    let addVersion (ctx: SqliteContext) (newVersion: Parameters.NewSeriesVersion) =
        ()