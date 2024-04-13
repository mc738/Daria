namespace Daria.V2.Operations.Build.PageRenderer

open System
open System.Text.Encodings.Web
open Daria.V2.DataStore.Models
open FDOM.Core.Common
open FDOM.Core.Parsing
open Fluff.Core
open Freql.Sqlite

module Articles =
    
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
        let localUrlPrefix = createLocalUrlPrefix depth

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
              "image", Mustache.Value.Scalar $"{localUrlPrefix}/img/{rai.GetName()}"

              "preview_image",
              rai.PreviewUrl
              |> Option.defaultValue $"{localUrlPrefix}/img/{rai.GetName()}"
              |> Mustache.Value.Scalar

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
            Parser.ParseLinesAndMetadata(articleContent.Split Environment.NewLine |> List.ofArray)
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

