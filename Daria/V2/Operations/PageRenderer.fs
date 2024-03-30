namespace Daria.V2.Operations

open System
open System.IO
open System.Text
open System.Text.Encodings.Web
open Daria.V2.DataStore
open Daria.V2.DataStore.Common
open Daria.V2.DataStore.Models
open FDOM.Core.Common
open FDOM.Core.Parsing
open FDOM.Rendering
open Fluff.Core
open Freql.Sqlite
open SQLitePCL

module PageRenderer =

    module Internal =

        /// <summary>
        /// A function to create a local url prefix based on the "depth" of a entity.
        /// For example, top level entities (i.e. in the main directory) will return ".".
        /// Entities in a subdirectory (depth 1) will return "..".
        /// Entities in with a depth 2 will return "../..".
        /// </summary>
        /// <param name="depth">The current depth.</param>
        let createLocalUrlPrefix (depth: int) =
            match depth >= 1 with
            | true ->
                [ for i in 0 .. (depth - 1) do
                      ".." ]
                |> String.concat "/"
            | _ -> "."

    type ShareLinkType =
        | Facebook
        | Twitter
        | LinkedIn
        | Email

        static member Deserialize(value: string) =
            match value.ToLower() with
            | "facebook" -> Some ShareLinkType.Facebook
            | "twitter" -> Some ShareLinkType.Twitter
            | "linkedin" -> Some ShareLinkType.LinkedIn
            | "email" -> Some ShareLinkType.Email
            | _ -> None

        static member All() = [ Facebook; Twitter; LinkedIn; Email ]

        member slt.Serialize() =
            match slt with
            | Facebook -> "facebook"
            | Twitter -> "twitter"
            | LinkedIn -> "linkedin"
            | Email -> "email"

        member slt.GenerateUrl(articleUrl: string, title: string, description: string) =
            let linebreak = "%0D%0A"

            match slt with
            | Facebook -> $"https://www.facebook.com/sharer.php?u={articleUrl}"
            | Twitter -> $"https://twitter.com/intent/tweet?text={articleUrl}"
            | LinkedIn -> $"https://linkedin.com/sharing/share-offsite/?url={articleUrl}"
            | Email -> $"mailto:?subject={title}&body={description}{linebreak}{articleUrl}"

        member slt.GetIcon() =
            match slt with
            | Facebook -> "fab fa-facebook"
            | Twitter -> "fab fa-twitter"
            | LinkedIn -> "fab fa-linkedin"
            | Email -> "fas fa-envelope"

        member slt.GetTitle() =
            match slt with
            | Facebook -> "Share on Facebook"
            | Twitter -> "Share on Twitter"
            | LinkedIn -> "Share on LinkedIn"
            | Email -> "Share via email"

    let createIssueLink (title: string) =
        title.Replace(" ", "+").Replace("#", "%23")
        |> fun s -> $"https://github.com/mc738/Articles/issues/new?title=`{s}`+issue"

    let createShareLinksData (articleUrl: string) (title: string) (description: string) =
        ShareLinkType.All()
        |> List.map (fun slt ->
            [ "icon", Mustache.Value.Scalar <| slt.GetIcon()
              "link_url", Mustache.Value.Scalar <| slt.GenerateUrl(articleUrl, title, description)
              "link_title", Mustache.Value.Scalar <| slt.GetTitle() ]
            |> Map.ofList
            |> Mustache.Value.Object)
        |> Mustache.Value.Array

    let createLinkData (links: ArticleLink list) =
        links
        |> List.map (fun link ->
            [ "link_title", Mustache.Value.Scalar link.Title
              "link_description", Mustache.Value.Scalar link.Description
              "link_url", Mustache.Value.Scalar link.Url ]
            |> Map.ofList
            |> Mustache.Value.Object)
        |> Mustache.Value.Array

    let createTagsData (tags: string list) =
        tags
        |> List.map (fun tag -> [ "tag_name", Mustache.Value.Scalar tag ] |> Map.ofList |> Mustache.Value.Object)
        |> Mustache.Value.Array

    let createPartData (part: RenderableArticlePart) =
        [ "part_title", Mustache.Value.Scalar part.Title
          "part_url", Mustache.Value.Scalar $"./{part.TitleSlug}.html" ]
        |> Map.ofList
        |> Mustache.Value.Object

    let createArticlePageData
        (depth: int)
        (title: DOM.HeaderBlock)
        (description: DOM.ParagraphBlock)
        (url: string)
        (article: RenderableArticle)
        =
        let localUrlPrefix = Internal.createLocalUrlPrefix depth

        let articleUrl = $"{url}/{UrlEncoder.Default.Encode article.TitleSlug}.html"

        [ "title_html", Mustache.Value.Scalar <| Html.renderTitle title
          "title_text", Mustache.Value.Scalar <| title.GetRawText()
          "description_html", Mustache.Value.Scalar <| Html.renderDescription description
          "description_text", Mustache.Value.Scalar <| description.GetRawText()
          "local_url_prefix", Mustache.Value.Scalar localUrlPrefix
          "sections",
          Mustache.Value.Array
              [ [ "collection_title", Mustache.Value.Scalar "Parts"
                  "parts", article.AllParts |> List.map createPartData |> Mustache.Value.Array ]
                |> Map.ofList
                |> Mustache.Value.Object ]
          "share_links", createShareLinksData articleUrl article.Title article.Description
          "tags", createTagsData article.Tags
          "url", Mustache.Value.Scalar articleUrl
          "version", Mustache.Value.Scalar <| string article.Version
          "article_date",
          article.PublishedOn
          |> Option.defaultValue (article.CreatedOn)
          |> fun dt -> dt.ToString("dd MMMM yyyy")
          |> Mustache.Value.Scalar

          "now", Mustache.Value.Scalar(DateTime.Now.ToString("dd MMMM yyyy 'at' HH:mm:ss"))
          match article.PreviousPart with
          | Some pp -> "previous_part", createPartData pp
          | None -> ()

          match article.NextPart with
          | Some np -> "next_part", createPartData np
          | None -> ()

          "links", createLinkData article.Links

          "gh_issue_link", Mustache.Value.Scalar <| createIssueLink article.Title
          match article.Image with
          | Some rai ->
              "image", Mustache.Value.Scalar $"{localUrlPrefix}/img/{rai.Name}"
              "preview_image", Mustache.Value.Scalar $"{localUrlPrefix}/img/{rai.PreviewName}"
              "thanks", Mustache.Value.Scalar rai.Thanks
          | None -> ()

          match article.RawLink with
          | Some rawLink -> "raw_link", Mustache.Value.Scalar rawLink
          | None -> ()
          match article.OverrideCssName with
          | Some name ->
              let url =
                  match name.EndsWith(".css") with
                  | true -> $"{localUrlPrefix}/css/{name}"
                  | false -> $"{localUrlPrefix}/css/{name}.css"

              "override_css", [ "css_url", Mustache.Value.Scalar url ] |> Map.ofList |> Mustache.Object
          | None -> () ]

    let renderArticle
        (ctx: SqliteContext)
        (template: Mustache.Token list)
        (depth: int)
        (url: string)
        (saveDirectory: string)
        (article: RenderableArticle)
        (articleContent: string)
        =

        let blocks =
            Parser
                .ParseLines(articleContent.Split Environment.NewLine |> List.ofArray)
                .CreateBlockContent()

        let (titleBlock, descriptionBlock, content) = blocks.[0], blocks.[1], blocks.[2..]

        let title =
            match titleBlock with
            | DOM.BlockContent.Header h -> Some h
            | _ -> None
            |> Option.defaultWith (fun _ -> failwith "Missing title.")

        let description =
            match descriptionBlock with
            | DOM.BlockContent.Paragraph p -> p
            | _ ->
                { Style = DOM.Style.Default
                  Content = [ DOM.InlineContent.Text { Content = "" } ] }

        let doc: FDOM.Core.Common.DOM.Document =
            { Style = FDOM.Core.Common.DOM.Style.Default
              Name = ""
              Title = Some title
              Sections =
                [ { Style = FDOM.Core.Common.DOM.Style.Default
                    Title = None
                    Name = "Section 1"
                    Content = content } ]
              Resources = [] }

        let pageData =
            ({ Values = createArticlePageData depth title description url article |> Map.ofList
               Partials = Map.empty }
            : Mustache.Data)

        Html.renderFromParsedTemplate template pageData [] [] doc
        |> fun r -> File.WriteAllText(Path.Combine(saveDirectory, $"{article.TitleSlug}.html"), r)

    let createSeriesIndexPageData
        (depth: int)
        (title: DOM.HeaderBlock)
        (description: DOM.ParagraphBlock)
        (url: string)
        (series: SeriesListingItem)
        (seriesVersion: SeriesVersionOverview)
        (articles: RenderableArticle list)
        (tags: string list)
        =
        let localUrlPrefix = Internal.createLocalUrlPrefix depth


        [ "title_html", Mustache.Value.Scalar <| Html.renderTitle title
          "title_text", Mustache.Value.Scalar <| title.GetRawText()
          "description_html", Mustache.Value.Scalar <| Html.renderDescription description
          "description_text", Mustache.Value.Scalar <| description.GetRawText()
          "title_slug", Mustache.Value.Scalar seriesVersion.TitleSlug
          "local_url_prefix", Mustache.Value.Scalar localUrlPrefix
          "tags", createTagsData tags
          "parts",
          articles
          |> List.map (fun a ->
              [ "title", Mustache.Value.Scalar a.Title
                "url", Mustache.Value.Scalar $"./{a.TitleSlug}.html"
                "description", Mustache.Value.Scalar a.Description ]
              |> Map.ofList
              |> Mustache.Value.Object)
          |> Mustache.Value.Array
          "series",
          series.Children
          |> List.choose (fun s ->
              s.Versions
              |> List.filter (fun sv -> sv.Active && sv.DraftVersion.IsNone)
              |> List.sortByDescending (fun sv -> sv.Version)
              |> List.tryHead
              |> Option.map (fun sv ->
                  [ "title", Mustache.Value.Scalar sv.Title
                    "title_slug", Mustache.Value.Scalar sv.TitleSlug
                    "description", Mustache.Value.Scalar sv.Description
                    "url", Mustache.Value.Scalar $"./{sv.TitleSlug}/index.html" ]
                  |> Map.ofList
                  |> Mustache.Value.Object))
          |> Mustache.Value.Array ]

    let renderSeriesIndexPage
        (ctx: SqliteContext)
        (indexTemplate: Mustache.Token list)
        (depth: int)
        (url: string)
        (saveDirectory: string)
        (series: SeriesListingItem)
        (seriesVersion: SeriesVersionOverview)
        (articles: RenderableArticle list)
        (indexContent: string)
        =

        let blocks =
            Parser
                .ParseLines(indexContent.Split Environment.NewLine |> List.ofArray)
                .CreateBlockContent()

        let (titleBlock, descriptionBlock, content) = blocks.[0], blocks.[1], blocks.[2..]

        let title =
            match titleBlock with
            | DOM.BlockContent.Header h -> Some h
            | _ -> None
            |> Option.defaultWith (fun _ -> failwith "Missing title.")

        let description =
            match descriptionBlock with
            | DOM.BlockContent.Paragraph p -> p
            | _ ->
                { Style = DOM.Style.Default
                  Content = [ DOM.InlineContent.Text { Content = "" } ] }

        let tags = Series.getSeriesVersionTags ctx seriesVersion.Id

        let pageData =
            ({ Values =
                createSeriesIndexPageData depth title description url series seriesVersion articles tags
                |> Map.ofList
               Partials = Map.empty }
            : Mustache.Data)

        let (titleBlock, descriptionBlock, content) = blocks.[0], blocks.[1], blocks.[2..]

        let title =
            match titleBlock with
            | DOM.BlockContent.Header h -> Some h
            | _ -> None
            |> Option.defaultWith (fun _ -> failwith "Missing title.")

        let description =
            match descriptionBlock with
            | DOM.BlockContent.Paragraph p -> p
            | _ ->
                { Style = DOM.Style.Default
                  Content = [ DOM.InlineContent.Text { Content = "" } ] }

        let doc: FDOM.Core.Common.DOM.Document =
            { Style = FDOM.Core.Common.DOM.Style.Default
              Name = ""
              Title = Some title
              Sections =
                [ { Style = FDOM.Core.Common.DOM.Style.Default
                    Title = None
                    Name = "Section 1"
                    Content = content } ]
              Resources = [] }

        Html.renderFromParsedTemplate indexTemplate pageData [] [] doc
        |> fun r -> File.WriteAllText(Path.Combine(saveDirectory, "index.html"), r)

    let rec renderSeries
        (ctx: SqliteContext)
        (pageTemplate: Mustache.Token list)
        (indexTemplate: Mustache.Token list)
        (depth: int)
        (url: string)
        (saveDirectory: string)
        (series: SeriesListingItem)
        =

        let version =
            series.Versions
            |> List.filter (fun sv -> sv.DraftVersion.IsNone)
            |> List.maxBy (fun sv -> sv.Version)

        let dirPath = Path.Combine(saveDirectory, version.TitleSlug)

        Directory.CreateDirectory(dirPath) |> ignore

        // Render the index page.
        let articles = Articles.getRenderableArticles ctx series.Id

        // TODO Better handling if series content is not found (but in reality it should be).
        Series.getSeriesIndexVersionContent ctx version.Id
        |> Option.iter (renderSeriesIndexPage ctx indexTemplate depth url dirPath series version articles)

        // Render article pages.
        articles
        |> List.iter (fun ra ->
            Articles.getArticleVersionContent ctx ra.VersionId
            |> Option.iter (renderArticle ctx pageTemplate depth url dirPath ra))

        series.Children
        |> List.iter (renderSeries ctx pageTemplate indexTemplate (depth + 1) url dirPath)

    module Index =
        
        let createPageItem (ctx: SqliteContext) (series: SeriesListingItem) (seriesVersion: SeriesVersionOverview) =
            let tags = Series.getSeriesVersionTags ctx seriesVersion.Id

            [ "item_name", Mustache.Value.Scalar series.Name
              "item_description", Mustache.Value.Scalar seriesVersion.Description
              "item_image", Mustache.Value.Scalar series.DisplayImage
              match DataStore.getFirstArticle ctx series.Name with
              | Some a ->
                  "item_link",
                  [ "url", Mustache.Scalar a.Url ]
                  |> Map.ofList
                  |> Mustache.Object
              | None -> ()
              "item_tags",
              tags
              |> List.map (fun t ->
                  [ "tag", Mustache.Value.Scalar t ]
                  |> Map.ofList
                  |> Mustache.Value.Object)
              |> Mustache.Array ]
            |> Map.ofList
            |> Mustache.Value.Object

        let createPageItems (ctx: SqliteContext) (isFirst: bool) (isLast: bool) (index: int) (series: SeriesListingItem list) =

            [ "page_id", Mustache.Value.Scalar $"page-{index + 1}"
              "page_name", Mustache.Value.Scalar $"Page {index + 1}"
              "page_items",
              series
              |> List.map (createPageItem ctx)
              |> Mustache.Value.Array
              match isFirst |> not with
              | true ->
                  "prev_page",
                  [ "prev_page_id", Mustache.Value.Scalar $"page-{index}" ]
                  |> Map.ofList
                  |> Mustache.Value.Object
              | false -> ()

              match isLast |> not with
              | true ->
                  "next_page",
                  [ "next_page_id", Mustache.Value.Scalar $"page-{index + 2}" ]
                  |> Map.ofList
                  |> Mustache.Value.Object
              | false -> () ]
            |> Map.ofList
            |> Mustache.Value.Object

        let createPages (ctx: SqliteContext) =
            let series =
                DataStore.getOrderedSeries ctx
                |> List.chunkBySize 4

            series
            |> List.mapi (fun i s -> createPageItems ctx (i = 0) (i = series.Length - 1) i s)
            |> Mustache.Value.Array

        let createLatestPost (article: Records.Article) (version: Records.ArticleVersion) =
            [ "post_name", Mustache.Value.Scalar article.Name
              "post_description", Mustache.Value.Scalar article.Description
              "post_date",
              Mustache.Value.Scalar
              <| version.PublishDate.ToString("dd MMMM yyyy")
              "post_image", Mustache.Value.Scalar article.DisplayImage
              "post_link", Mustache.Value.Scalar article.Url ]
            |> Map.ofList
            |> Mustache.Value.Object

        let createLatestPosts (ctx: SqliteContext) =
            DataStore.getLatestPosts ctx 3
            |> List.map (fun (a, v) -> createLatestPost a v)
            |> Mustache.Value.Array

        let createLink (ctx: SqliteContext) (series: Records.Series) =
            DataStore.getFirstArticle ctx series.Name
            |> Option.map (fun fa ->
                [ "series_name", Mustache.Value.Scalar series.Name
                  "series_url", Mustache.Value.Scalar fa.Url ]
                |> Map.ofList
                |> Mustache.Value.Object)

        let createLinks (ctx: SqliteContext) =
            DataStore.getOrderedSeries ctx
            |> List.map (createLink ctx)
            |> List.choose id
            |> Mustache.Value.Array
        
        let buildIndex (template: Mustache.Token list) (ctx: SqliteContext) =

            let data =
                ({ Values =
                    [ "pages", createPages ctx
                      "recent_posts", createLatestPosts ctx
                      "series", createLinks ctx ]
                    |> Map.ofList
                   Partials = Map.empty }: Mustache.Data)

            Mustache.replace data true template
    
    let run storePath =
        use ctx = SqliteContext.Open storePath

        let rootPath = ""
        let url = ""

        let pageTemplate = []
        let indexTemplate = []

        Series.list ctx ActiveStatus.Active
        |> List.iter (renderSeries ctx pageTemplate indexTemplate 1 url rootPath)
