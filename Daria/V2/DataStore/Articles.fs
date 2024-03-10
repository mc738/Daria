namespace Daria.V2.DataStore

// No warn for "internal use" warnings.
#nowarn "100001"

open Daria.V2.DataStore.Common

[<RequireQualifiedAccess>]
module Articles =

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
        /// An internal record representing a article version.
        /// This contains the minimum data required for internal operations.
        /// </summary>
        type ArticleVersionListingItem =
            { Id: string
              Version: int
              DraftVersion: int option
              Hash: string
              Active: bool
              Draft: bool }

            static member SelectSql() =
                "SELECT id, version, draft_version, hash, active FROM article_versions"

        /// <summary>
        /// This used a bespoke query bypassing `Operations` because the version blob is not needed.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="articleId"></param>
        /// <param name="activeStatus"></param>
        /// <param name="draftStatus"></param>
        let fetchArticleVersions
            (ctx: SqliteContext)
            (articleId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =
            let sql =
                [ "SELECT id, article_id, version, draft_version, title, title_slug, description, hash, created_on, published_on, active FROM article_versions"
                  "WHERE article_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> () ]
                |> toSql

            ctx.SelectAnon<ArticleVersionOverview>(sql, [ articleId ])

        let fetchArticleVersionListings
            (ctx: SqliteContext)
            (articleId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =
            let sql =
                [ ArticleVersionListingItem.SelectSql()
                  "WHERE article_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> () ]
                |> toSql

            ctx.SelectAnon<ArticleVersionListingItem>(sql, [ articleId ])

        let fetchLatestVersionListing
            (ctx: SqliteContext)
            (articleId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =

            let sql =
                [ ArticleVersionListingItem.SelectSql()
                  "WHERE article_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  "ORDER BY version DESC, draft_version DESC"
                  "LIMIT 1" ]
                |> toSql

            ctx.SelectSingleAnon<ArticleVersionListingItem>(sql, [ articleId ])

        let fetchVersionListingById (ctx: SqliteContext) (versionId: string) =
            let sql = [ ArticleVersionListingItem.SelectSql(); "WHERE id = @0" ] |> toSql

            ctx.SelectSingleAnon<ArticleVersionListingItem>(sql, [ versionId ])

        let deleteArticleVersion (ctx: SqliteContext) (versionId: string) =
            ctx.ExecuteVerbatimNonQueryAnon("DELETE FROM article_versions WHERE id = @0", [ versionId ])
            |> ignore

        let addVersionTag (ctx: SqliteContext) (versionId: string) (tag: string) =
            ({ ArticleVersionId = versionId
               Tag = tag }
            : Parameters.NewArticleVersionTag)
            |> Operations.insertArticleVersionTag ctx

        let addVersionMetadata (ctx: SqliteContext) (versionId: string) (key: string) (value: string) =
            ({ ArticleVersionId = versionId
               ItemKey = key
               ItemValue = value }
            : Parameters.NewArticleVersionMetadataItem)
            |> Operations.insertArticleVersionMetadataItem ctx

    open Internal

    let rec fetchArticleVersionOverviews (ctx: SqliteContext) (articleId: string) =
        // TODO finish
        Internal.fetchArticleVersions

    let exists (ctx: SqliteContext) (articleId: string) =
        Operations.selectArticleRecord ctx [ "WHERE id = @0;" ] [ articleId ]
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

    let list (ctx: SqliteContext) =
        let rec build (article: Records.Article list) = article


        Operations.selectArticleRecords ctx [] []
        |> List.map (fun a ->
            ({ Id = a.Id
               Name = a.Name
               SeriesId = a.SeriesId
               Order = a.ArticleOrder
               CreatedOn = a.CreatedOn
               Active = a.Active
               Versions = Internal.fetchArticleVersions ctx a.Id ActiveStatus.All DraftStatus.All }
            : ArticleListingItem))

    let fetchLatestVersion (ctx: SqliteContext) (articleId: string) (includeDrafts: bool) =
        Operations.selectArticleVersionRecord
            ctx
            [ "WHERE article_id = @0"
              match includeDrafts with
              | true -> ()
              | false -> "AND draft = 0"
              "ORDER BY version DESC"
              "LIMIT 1" ]
            [ articleId ]

    let deleteLatestDraft (ctx: SqliteContext) (articleId: string) =
        match
            Internal.fetchLatestVersionListing ctx articleId ActiveStatus.Active DraftStatus.Draft,
            Internal.fetchLatestVersionListing ctx articleId ActiveStatus.Active DraftStatus.NotDraft
        with
        | Some dv, Some ndv ->
            // Check if the latest draft version is the same or high than the latest non draft version.
            // This is to ensure old draft versions are not removed.
            match dv.Version >= ndv.Version with
            | true -> Internal.deleteArticleVersion ctx dv.Id
            | false -> ()
        | Some dv, None -> Internal.deleteArticleVersion ctx dv.Id
        | None, _ -> ()

    let add (ctx: SqliteContext) (newArticle: NewArticle) =
        let id = newArticle.Id.ToString()

        match Series.exists ctx newArticle.SeriesId, exists ctx id |> not with
        | true, true ->
            ({ Id = id
               Name = newArticle.Name
               SeriesId = newArticle.SeriesId
               ArticleOrder = newArticle.ArticleOrder
               CreatedOn = DateTime.UtcNow
               Active = true }
            : Parameters.NewArticle)
            |> Operations.insertArticle ctx

            AddResult.Success id
        | false, _ -> AddResult.MissingRelatedEntity("series", newArticle.SeriesId)
        | _, false -> AddResult.AlreadyExists id

    /// <summary>
    /// Add a new draft article version to the store.
    /// This will check if the previous draft version matches the new one.
    /// If so it no new draft version will be added unless the force parameter is true.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="newVersion"></param>
    /// <param name="force">Skip the diff check and add the new draft version. This can be useful if the check is handled externally.</param>
    let addDraftVersion (ctx: SqliteContext) (newVersion: NewArticleVersion) (force: bool) =
        let id = newVersion.Id.ToString()

        let (ms, hash) =
            match newVersion.ArticleBlob with
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
                    Internal.fetchLatestVersionListing t newVersion.ArticleId ActiveStatus.Active DraftStatus.Draft,
                    Internal.fetchLatestVersionListing t newVersion.ArticleId ActiveStatus.Active DraftStatus.NotDraft
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
            match fetchLatestVersionListing ctx newVersion.ArticleId ActiveStatus.Active DraftStatus.All with
            | Some pv ->
                match pv.DraftVersion with
                | Some dv -> pv.Version, dv + 1, Some pv.Hash
                | None -> pv.Version + 1, 1, Some pv.Hash
            | None -> 1, 1, None

        match versionExists ctx id |> not, force || compareHashes prevHash hash with
        | true, true ->
            let ivi =
                newVersion.ImageVersion
                |> Option.bind (function
                    | RelatedEntityVersion.Specified id -> Some id
                    | RelatedEntityVersion.Lookup version ->
                        match version with
                        | Specific(id, version) -> Images.Internal.getSpecificVersion ctx id version
                        | Latest id -> Images.Internal.getLatestVersion ctx id
                        |> Option.map (fun i -> i.Id)
                    | RelatedEntityVersion.Bespoke fn -> fn ctx)

            ({ Id = id
               ArticleId = newVersion.ArticleId
               Version = version
               DraftVersion = Some draftVersion
               Title = newVersion.Title
               TitleSlug =
                 newVersion.TitleSlug
                 |> Option.defaultWith (fun _ -> newVersion.Title |> slugify)
               Description = newVersion.Description
               ArticleBlob = BlobField.FromStream ms
               Hash = hash
               ImageVersionId = ivi
               RawLink = newVersion.RawLink
               OverrideCssName = newVersion.OverrideCss 
               CreatedOn = newVersion.CreatedOn |> Option.defaultValue DateTime.UtcNow
               PublishedOn = newVersion.PublishedOn 
               Active = true }
            : Parameters.NewArticleVersion)
            |> Operations.insertArticleVersion ctx

            addVersionTags ctx id newVersion.Tags
            newVersion.Metadata |> Map.iter (addVersionMetadata ctx id)

            AddResult.Success id
        | false, _ -> AddResult.AlreadyExists id
        | _, false -> AddResult.NoChange id

    /// <summary>
    /// A new article version to the store.
    /// This will check the new version against the previous version and if there are no changes no new version will be added,
    /// unless the force parameter is true.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="newVersion"></param>
    /// <param name="force"></param>
    let addVersion (ctx: SqliteContext) (newVersion: NewArticleVersion) (force: bool) =
        let id = newVersion.Id.ToString()

        let ms, hash =
            match newVersion.ArticleBlob with
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
                Internal.fetchLatestVersionListing ctx newVersion.ArticleId ActiveStatus.Active DraftStatus.NotDraft
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

        match versionExists ctx id |> not, force || compareHashes prevHash hash with
        | true, true ->
            let ivi =
                newVersion.ImageVersion
                |> Option.bind (function
                    | RelatedEntityVersion.Specified id -> Some id
                    | RelatedEntityVersion.Lookup version ->
                        match version with
                        | Specific(id, version) -> Images.Internal.getSpecificVersion ctx id version
                        | Latest id -> Images.Internal.getLatestVersion ctx id
                        |> Option.map (fun i -> i.Id)
                    | RelatedEntityVersion.Bespoke fn -> fn ctx)

            ({ Id = id
               ArticleId = newVersion.ArticleId
               Version = version
               DraftVersion = None
               Title = newVersion.Title
               TitleSlug =
                 newVersion.TitleSlug
                 |> Option.defaultWith (fun _ -> newVersion.Title |> slugify)
               Description = newVersion.Description
               ArticleBlob = BlobField.FromStream ms
               Hash = hash
               ImageVersionId = ivi
               RawLink = newVersion.RawLink
               OverrideCssName = newVersion.OverrideCss 
               CreatedOn = newVersion.CreatedOn |> Option.defaultValue DateTime.UtcNow
               PublishedOn = newVersion.PublishedOn 
               Active = true }
            : Parameters.NewArticleVersion)
            |> Operations.insertArticleVersion ctx

            addVersionTags ctx id newVersion.Tags
            newVersion.Metadata |> Map.iter (addVersionMetadata ctx id)

            AddResult.Success id
        | false, _ -> AddResult.AlreadyExists id
        | _, false -> AddResult.NoChange id
