﻿namespace Daria.V2.DataStore

open System.Text
open Daria.V2.Common.Domain
open Daria.V2.DataStore.Common
open Daria.V2.DataStore.Models
open Daria.V2.DataStore.Persistence

// No warn for "internal use" warnings.
#nowarn "100001"


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

    module Internal =

        /// <summary>
        /// An internal record representing a series version.
        /// This contains the minimum data required for internal operations.
        /// </summary>
        
        type SeriesVersionListingItem =
            { Id: string
              Version: int
              DraftVersion: int option
              Hash: string
              Active: bool }

            static member SelectSql() =
                "SELECT id, version, draft_version, hash, active FROM series_versions"

        let fetchTopLevelSeries (ctx: SqliteContext) (activeStatus: ActiveStatus) =
            Operations.selectSeriesRecords
                ctx
                [ "WHERE parent_series_id IS NULL"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> () ]
                []

        let fetchSeriesByParent (ctx: SqliteContext) (parentSeriesId: string) (activeStatus: ActiveStatus) =
            Operations.selectSeriesRecords
                ctx
                [ "WHERE parent_series_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> () ]
                [ parentSeriesId ]


        /// <summary>
        /// This used a bespoke query bypassing `Operations` because the version blob is not needed.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="seriesId"></param>
        /// <param name="activeStatus"></param>
        /// <param name="draftStatus"></param>
        let fetchSeriesVersionOverviews
            (ctx: SqliteContext)
            (seriesId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =
            let sql =
                [ "SELECT id, series_id, version, draft_version, title, title_slug, description, hash, imagine_version_id, created_on, active FROM series_versions"
                  "WHERE series_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> () ]
                |> toSql

            ctx.SelectAnon<SeriesVersionOverview>(sql, [ seriesId ])

        let fetchSeriesVersionOverview
            (ctx: SqliteContext)
            (seriesId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =
            let sql =
                [ "SELECT id, series_id, version, draft_version, title, title_slug, description, hash, image_version_id, created_on, active FROM series_versions"
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

            ctx.SelectSingleAnon<SeriesVersionOverview>(sql, [ seriesId ])


        let fetchSeriesVersionListings
            (ctx: SqliteContext)
            (seriesId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =
            let sql =
                [ SeriesVersionListingItem.SelectSql()
                  "WHERE series_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> () ]
                |> toSql

            ctx.SelectAnon<SeriesVersionListingItem>(sql, [ seriesId ])

        let fetchLatestVersionListing
            (ctx: SqliteContext)
            (seriesId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =

            let sql =
                [ SeriesVersionListingItem.SelectSql()
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

        let fetchVersionListingById (ctx: SqliteContext) (versionId: string) =
            let sql = [ SeriesVersionListingItem.SelectSql(); "WHERE id = @0" ] |> toSql

            ctx.SelectSingleAnon<SeriesVersionListingItem>(sql, [ versionId ])

        let deleteSeriesVersion (ctx: SqliteContext) (versionId: string) =
            ctx.ExecuteVerbatimNonQueryAnon("DELETE FROM series_versions WHERE id = @0", [ versionId ])
            |> ignore

        let addVersionTag (ctx: SqliteContext) (versionId: string) (tag: string) =
            ({ SeriesVersionId = versionId
               Tag = tag }
            : Parameters.NewSeriesVersionTag)
            |> Operations.insertSeriesVersionTag ctx

        let addVersionMetadata (ctx: SqliteContext) (versionId: string) (key: string) (value: string) =
            ({ SeriesVersionId = versionId
               ItemKey = key
               ItemValue = value }
            : Parameters.NewSeriesVersionMetadataItem)
            |> Operations.insertSeriesVersionMetadataItem ctx

        let fetchRenderableSeriesIndexes (ctx: SqliteContext) (series: Records.Series list) =

            let seriesArr =
                series
                |> List.choose (fun sr ->
                    fetchSeriesVersionOverview ctx sr.Id ActiveStatus.Active DraftStatus.NotDraft
                    |> Option.map (fun sv -> sr, sv))

            seriesArr
            |> List.map (fun (sr, svr) ->
                ({ Id = sr.Id
                   VersionId = svr.Id
                   Version = svr.Version
                   Title = svr.Title
                   TitleSlug = svr.TitleSlug
                   Description = svr.Description
                   CreatedOn = svr.CreatedOn
                   Articles =
                     Operations.selectArticleRecords
                         ctx
                         [ "WHERE series_id = @0 AND active = TRUE"; "ORDER BY article_order" ]
                         [ sr.Id ]
                     |> List.choose (fun ar ->
                         Operations.selectArticleVersionRecord
                             ctx
                             [ "WHERE article_id = @0 AND active = TRUE AND draft_version IS NULL"
                               "ORDER BY version"
                               "LIMIT 1" ]
                             [ ar.Id ]
                         |> Option.map (fun avr ->
                             ({ Title = avr.Title
                                TitleSlug = avr.TitleSlug
                                Description = avr.Description }
                             : RenderableSeriesIndexArticlePart)))
                   Series =
                     fetchSeriesByParent ctx sr.Id ActiveStatus.Active
                     |> List.choose (fun csr ->
                         Operations.selectSeriesVersionRecord
                             ctx
                             [ "WHERE series_id = @0 AND active = TRUE AND draft_version IS NULL"
                               "ORDER BY version"
                               "LIMIT 1" ]
                             [ csr.Id ]
                         |> Option.map (fun csv ->
                             ({ Title = csv.Title
                                TitleSlug = csv.TitleSlug
                                Description = csv.Description }
                             : RenderableSeriesIndexSeriesPart)))
                   Image =
                     svr.ImageVersionId
                     |> Option.bind (fun iv -> Operations.selectImageVersionRecord ctx [ "WHERE id = @0" ] [ iv ])
                     |> Option.bind (fun iv ->
                         Operations.selectImageRecord ctx [ "WHERE id = @0" ] [ iv.ImageId ]
                         |> Option.map (fun ir -> ir, iv))
                     |> Option.bind (fun (ir, iv) ->
                         Operations.selectResourceVersionRecord ctx [ "WHERE id = @0" ] [ iv.ResourceVersionId ]
                         |> Option.map (fun rv -> ir, iv, rv))
                     |> Option.map (fun (ir, iv, rv) ->
                         ({ Name = ir.Name
                            Version = iv.Version
                            Extension = FileType.GetFileExtensionFromString rv.FileType 
                            Thanks = iv.ThanksHtml |> Option.defaultValue ""
                            PreviewUrl = iv.PreviewUrl }
                         : RenderableSeriesIndexImage))
                   Tags =
                     Operations.selectSeriesVersionTagRecords ctx [ "WHERE series_version_id = @0" ] [ svr.Id ]
                     |> List.map (fun t -> t.Tag) }
                : RenderableSeriesIndex))

    open Internal

    let rec fetchSeriesVersionOverviews (ctx: SqliteContext) (seriesId: string) =
        // TODO finish
        Internal.fetchSeriesVersionOverviews

    let exists (ctx: SqliteContext) (seriesId: string) =
        Operations.selectSeriesRecord ctx [ "WHERE id = @0;" ] [ seriesId ]
        |> Option.isSome

    let versionExists (ctx: SqliteContext) (versionId: string) =
        Internal.fetchVersionListingById ctx versionId |> Option.isSome

    let addVersionTags (ctx: SqliteContext) (versionId: string) (tags: string list) =
        tags
        |> List.iter (fun t ->
            // Check tag already exists. If not add it.
            match Tags.exists ctx t with
            | true -> ()
            | false -> Tags.add ctx t

            addVersionTag ctx versionId t)

    let list (ctx: SqliteContext) (activeStatus: ActiveStatus) =
        let rec build (series: Records.Series list) =
            series
            |> List.map (fun s ->
                ({ Id = s.Id
                   Name = s.Name
                   Order = s.SeriesOrder
                   CreatedOn = s.CreatedOn
                   Active = s.Active
                   Children = Internal.fetchSeriesByParent ctx s.Id activeStatus |> build
                   Versions = Internal.fetchSeriesVersionOverviews ctx s.Id ActiveStatus.All DraftStatus.All }
                : SeriesListingItem))

        Internal.fetchTopLevelSeries ctx activeStatus |> build

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

    let add (ctx: SqliteContext) (newSeries: NewSeries) =
        let id = newSeries.Id.ToString()

        match exists ctx id |> not with
        | true ->
            let addHandler () =
                ({ Id = id
                   Name = newSeries.Name
                   ParentSeriesId = newSeries.ParentId
                   SeriesOrder = newSeries.SeriesOrder
                   CreatedOn = newSeries.CreatedOn |> Option.defaultValue DateTime.UtcNow
                   Active = true }
                : Parameters.NewSeries)
                |> Operations.insertSeries ctx

                AddResult.Success id

            // TODO if it exists check of parent is the same. If not update.

            match newSeries.ParentId with
            | Some parentId when exists ctx parentId -> addHandler ()
            | Some parentId -> AddResult.MissingRelatedEntity("parent_series", parentId)
            | None -> addHandler ()
        | false -> AddResult.AlreadyExists id

    /// <summary>
    /// Add a new draft series version to the store.
    /// This will check if the previous draft version matches the new one.
    /// If so it no new draft version will be added unless the force parameter is true.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="newVersion"></param>
    /// <param name="force">Skip the diff check and add the new draft version. This can be useful if the check is handled externally.</param>
    let addDraftVersion (ctx: SqliteContext) (force: bool) (newVersion: NewSeriesVersion) =
        let id = newVersion.Id.ToString()

        // TODO might need to check this gets properly disposed.
        use ms =
            match newVersion.IndexBlob with
            | Blob.Prepared(memoryStream, _) -> memoryStream
            | Blob.Stream stream ->
                let ms = stream |> toMemoryStream
                ms
            | Blob.Text t ->
                // Uses let or else the memory stream gets disposed before being used later.
                let ms = new MemoryStream(t.ToUtf8Bytes())
                ms
            | Blob.Bytes b ->
                // Uses let or else the memory stream gets disposed before being used later.
                let ms = new MemoryStream(b)
                ms

        let hash =
            match newVersion.IndexBlob with
            | Blob.Prepared(_, hash) -> hash
            | _ -> ms.GetSHA256Hash()

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
            match fetchLatestVersionListing ctx newVersion.SeriesId ActiveStatus.Active DraftStatus.All with
            | Some pv ->
                match pv.DraftVersion with
                | Some dv -> pv.Version, dv + 1, Some pv.Hash
                | None -> pv.Version + 1, 1, Some pv.Hash
            | None -> 1, 1, None

        match exists ctx newVersion.SeriesId, versionExists ctx id |> not, force || ``hash has changed`` prevHash hash with
        | true, true, true ->
            let ivi =
                newVersion.ImageVersion
                |> Option.bind (function
                    | RelatedEntityVersion.Specified id ->
                        // TODO should this check the image version exists?
                        Some id
                    | RelatedEntityVersion.Lookup version ->
                        match version with
                        | Specific(id, version) -> Images.Internal.fetchSpecificVersion ctx id version
                        | Latest id -> Images.Internal.fetchLatestVersion ctx id
                        |> Option.map (fun i -> i.Id)
                    | RelatedEntityVersion.Bespoke fn -> fn ctx)

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
            |> Operations.insertSeriesVersion ctx

            addVersionTags ctx id newVersion.Tags
            newVersion.Metadata |> Map.iter (addVersionMetadata ctx id)

            AddResult.Success id
        | false, _, _ -> AddResult.MissingRelatedEntity("series", newVersion.SeriesId)
        | _, false, _ -> AddResult.AlreadyExists id
        | _, _, false -> AddResult.NoChange id

    /// <summary>
    /// A new series version to the store.
    /// This will check the new version against the previous version and if there are no changes no new version will be added,
    /// unless the force parameter is true.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="newVersion"></param>
    /// <param name="force"></param>
    let addVersion (ctx: SqliteContext) (force: bool) (newVersion: NewSeriesVersion) =
        let id = newVersion.Id.ToString()

        // TODO might need to check this gets properly disposed.
        use ms =
            match newVersion.IndexBlob with
            | Blob.Prepared(memoryStream, _) -> memoryStream
            | Blob.Stream stream ->
                let ms = stream |> toMemoryStream
                ms
            | Blob.Text t ->
                // Uses let or else the memory stream gets disposed before being used later.
                let ms = new MemoryStream(t.ToUtf8Bytes())
                ms
            | Blob.Bytes b ->
                // Uses let or else the memory stream gets disposed before being used later.
                let ms = new MemoryStream(b)
                ms

        let hash =
            match newVersion.IndexBlob with
            | Blob.Prepared(_, hash) -> hash
            | _ -> ms.GetSHA256Hash()

        let version, prevHash =
            match
                Internal.fetchLatestVersionListing ctx newVersion.SeriesId ActiveStatus.Active DraftStatus.NotDraft
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

        match exists ctx newVersion.SeriesId, versionExists ctx id |> not, force || ``hash has changed`` prevHash hash with
        | true, true, true ->
            let ivi =
                newVersion.ImageVersion
                |> Option.bind (function
                    | RelatedEntityVersion.Specified id -> Some id
                    | RelatedEntityVersion.Lookup version ->
                        match version with
                        | Specific(id, version) -> Images.Internal.fetchSpecificVersion ctx id version
                        | Latest id -> Images.Internal.fetchLatestVersion ctx id
                        |> Option.map (fun i -> i.Id)
                    | RelatedEntityVersion.Bespoke fn -> fn ctx)

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
            |> Operations.insertSeriesVersion ctx

            addVersionTags ctx id newVersion.Tags
            newVersion.Metadata |> Map.iter (addVersionMetadata ctx id)

            AddResult.Success id
        | false, _, _ -> AddResult.MissingRelatedEntity("series", newVersion.SeriesId)
        | _, false, _ -> AddResult.AlreadyExists id
        | _, _, false -> AddResult.NoChange id

    let getSeriesIndexVersionContent (ctx: SqliteContext) (versionId: string) =
        Operations.selectSeriesVersionRecord ctx [ "WHERE id = @0" ] [ versionId ]
        |> Option.map (fun ar -> ar.IndexBlob.ToBytes() |> Encoding.UTF8.GetString)

    let getSeriesVersionTags (ctx: SqliteContext) (versionId: string) =
        Operations.selectSeriesVersionTagRecords ctx [ "WHERE series_version_id = @0" ] [ versionId ]
        |> List.map (fun svt -> svt.Tag)

    let getTopLevelRenderableSeries (ctx: SqliteContext) =
        Operations.selectSeriesRecords
            ctx
            [ "WHERE parent_series_id IS NULL AND active = TRUE ORDER BY series_order" ]
            []
        |> fetchRenderableSeriesIndexes ctx

    let getRenderableSeriesForParent (ctx: SqliteContext) (parentSeriesId: string) =
        Operations.selectSeriesRecords
            ctx
            [ "WHERE parent_series_id = @0 AND active = TRUE ORDER BY series_order" ]
            [ parentSeriesId ]
        |> fetchRenderableSeriesIndexes ctx
