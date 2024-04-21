namespace Daria.V2.Operations.Import

#nowarn "100001"

open Daria.V2.Operations.Common

module Articles =

    open Freql.Sqlite
    open Daria.V2.Common.Metadata
    open Daria.V2.DataStore
    open Daria.V2.DataStore.Common

    let addArticle
        (ctx: SqliteContext)
        (settings: ImportSettings)
        (metadata: Map<string, string>)
        (seriesId: string)
        (fileName: string)
        =

        let articleId =
            metadata.TryFind Keys.articleId
            |> Option.orElseWith (fun _ -> metadata.TryFind Keys.titleSlug)
            |> Option.orElseWith (fun _ -> metadata.TryFind Keys.title |> Option.map slugify)
            |> Option.defaultValue (slugify fileName)

        ({ Id = IdType.Specific articleId
           Name =
             metadata.TryFind Keys.articleName
             |> Option.orElseWith (fun _ -> metadata.TryFind Keys.title)
             |> Option.defaultValue fileName
           SeriesId = seriesId
           ArticleOrder = metadata.TryFind Keys.order |> Option.bind tryToInt |> Option.defaultValue 99999
           CreatedOn =
             metadata.TryFind Keys.createdOn
             |> Option.bind (tryToDateTime settings.DateTimeFormats) }
        : Models.NewArticle)
        |> Articles.add ctx

    let addArticleVersion
        (ctx: SqliteContext)
        (settings: ImportSettings)
        (metadata: Map<string, string>)
        (articleId: string)
        (fileName: string)
        (filePath: string)
        (rawText: string)
        (lines: string list)
        =
        let rawArticleTitle, rawArticleDescription = tryGetTitleAndDescription lines

        let articleImageVersion =
            metadata.TryFind Keys.imageVersionId
            |> Option.map RelatedEntityVersion.Specified
            |> Option.orElseWith (fun _ ->
                metadata.TryFind Keys.imageId
                |> Option.map (fun iid ->
                    match metadata.TryFind Keys.imageVersion |> Option.bind tryToInt with
                    | Some v -> EntityVersion.Specific(iid, v)
                    | None -> EntityVersion.Latest iid
                    |> RelatedEntityVersion.Lookup))

        let newArticleVersion =
            ({ Id = IdType.Generated
               ArticleId = articleId
               Title =
                 metadata.TryFind Keys.title
                 |> Option.orElse rawArticleTitle
                 |> Option.defaultValue fileName
               TitleSlug = metadata.TryFind Keys.titleSlug
               Description = rawArticleDescription |> Option.defaultValue ""
               ArticleBlob = Blob.Text rawText
               ImageVersion = articleImageVersion
               RawLink = metadata.TryFind Keys.rawLink
               OverrideCss = metadata.TryFind Keys.overrideCss
               CreatedOn = None
               PublishedOn =
                 metadata.TryFind Keys.publishedOn
                 |> Option.bind (tryToDateTime settings.DateTimeFormats)
               Tags = metadata.TryFind Keys.tags |> Option.map splitValues |> Option.defaultValue []
               Metadata = metadata }
            : Models.NewArticleVersion)

        match
            metadata.TryFind Keys.draft
            |> Option.bind tryToBool
            |> Option.defaultValue false
        with
        | true -> Articles.addDraftVersion ctx false newArticleVersion
        | false -> Articles.addVersion ctx false newArticleVersion
        |> fun r -> { Path = filePath; Result = r }
