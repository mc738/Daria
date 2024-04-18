namespace Daria.V2.Operations.Build.PageRenderer

module Series =

    open System
    open System.IO
    open FDOM.Core.Common
    open FDOM.Core.Parsing
    open FDOM.Rendering
    open Fluff.Core
    open Freql.Sqlite
    open Daria.V2.DataStore
    open Daria.V2.DataStore.Models
        
    let createSeriesIndexPageData
        (depth: int)
        (title: DOM.HeaderBlock)
        (description: DOM.ParagraphBlock)
        (url: string)
        (series: RenderableSeriesIndex)
        (articles: RenderableArticle list)
        =
        let localUrlPrefix = createLocalUrlPrefix depth

        [ "title_html", Mustache.Value.Scalar <| Html.renderTitle title
          "title_text", Mustache.Value.Scalar <| title.GetRawText()
          "description_html", Mustache.Value.Scalar <| Html.renderDescription description
          "description_text", Mustache.Value.Scalar <| description.GetRawText()
          "title_slug", Mustache.Value.Scalar series.TitleSlug
          "local_url_prefix", Mustache.Value.Scalar localUrlPrefix
          "tags", createTagsData series.Tags

          match series.Image with
          | Some rsi ->
              "image", Mustache.Value.Scalar $"{localUrlPrefix}/img/{rsi.GetName()}"

              "preview_image",
              rsi.PreviewUrl
              |> Option.defaultValue $"{localUrlPrefix}/img/{rsi.GetName()}"
              |> Mustache.Value.Scalar

              "thanks", Mustache.Value.Scalar rsi.Thanks
          | None -> ()

          "parts",
          articles
          |> List.map (fun a ->
              [ "title", Mustache.Value.Scalar a.Title
                "url", Mustache.Value.Scalar $"./{a.TitleSlug}.html"
                "description", Mustache.Value.Scalar a.Description ]
              |> Map.ofList
              |> Mustache.Value.Object)
          |> Mustache.Value.Array

          match articles.IsEmpty with
          | true -> ()
          | false ->
              "articles",
              [ "items",
                articles
                |> List.map (fun a ->
                    [ "title", Mustache.Value.Scalar a.Title
                      "url", Mustache.Value.Scalar $"./{a.TitleSlug}.html"
                      "description", Mustache.Value.Scalar a.Description ]
                    |> Map.ofList
                    |> Mustache.Value.Object)
                |> Mustache.Value.Array ]
              |> Map.ofList
              |> Mustache.Value.Object

          match series.Series.IsEmpty with
          | true -> ()
          | false ->
              "series",
              [ "items",
                series.Series
                |> List.map (fun s ->
                    [ "title", Mustache.Value.Scalar s.Title
                      "title_slug", Mustache.Value.Scalar s.TitleSlug
                      "description", Mustache.Value.Scalar s.Description
                      "url", Mustache.Value.Scalar $"./{s.TitleSlug}/index.html" ]
                    |> Map.ofList
                    |> Mustache.Value.Object)
                |> Mustache.Value.Array ]
              |> Map.ofList
              |> Mustache.Value.Object ]

    let renderSeriesIndexPage
        (ctx: SqliteContext)
        (indexTemplate: Mustache.Token list)
        (depth: int)
        (url: string)
        (saveDirectory: string)
        (series: RenderableSeriesIndex)
        (articles: RenderableArticle list)
        (indexContent: string)
        =

        let blocks =
            Parser.ParseLinesAndMetadata(indexContent.Split Environment.NewLine |> List.ofArray)
            |> fun (p, _) -> p.CreateBlockContent()

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

        let pageData =
            ({ Values =
                createSeriesIndexPageData depth title description url series articles
                |> Map.ofList
               Partials = Map.empty }
            : Mustache.Data)

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
        (series: RenderableSeriesIndex)
        =

        let dirPath = Path.Combine(saveDirectory, series.TitleSlug)
        let newUrl = $"{url}/{series.TitleSlug}"

        Directory.CreateDirectory(dirPath) |> ignore

        // Render the index page.
        let articles = Articles.getRenderableArticles ctx series.Id

        // TODO Better handling if series content is not found (but in reality it should be).
        Series.getSeriesIndexVersionContent ctx series.VersionId
        |> Option.iter (renderSeriesIndexPage ctx indexTemplate depth newUrl dirPath series articles)

        // Render article pages.
        articles
        |> List.iter (fun ra ->
            Articles.getArticleVersionContent ctx ra.VersionId
            |> Option.iter (Articles.renderArticle ctx pageTemplate depth newUrl dirPath ra))

        Series.getRenderableSeriesForParent ctx series.Id
        |> List.iter (renderSeries ctx pageTemplate indexTemplate (depth + 1) newUrl dirPath)

