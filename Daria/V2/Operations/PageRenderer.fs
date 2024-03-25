namespace Daria.V2.Operations

open System
open Daria.V2.DataStore.Models
open FDOM.Rendering
open Fluff.Core
open Freql.Sqlite
open SQLitePCL

module PageRenderer =

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

    let createArticleData _ =
        [ "title", Mustache.Value.Scalar <| Html.renderTitle a.Title
          "title_text", Mustache.Value.Scalar a.TitleText
          "description", Mustache.Value.Scalar <| Html.renderDescription a.Description
          "description_text", Mustache.Value.Scalar a.DescriptionText ]

    let createPartData (part: RenderableArticlePart) =
        [ "part_title", Mustache.Value.Scalar part.Title
          "part_url", Mustache.Value.Scalar $"./{part.TitleSlug}.html" ]
        |> Map.ofList
        |> Mustache.Value.Object

    let createPageData (depth: int) (article: RenderableArticle) =
        let urlDepth =
            match depth >= 1 with
            | true ->
                [ for i in 0 .. (depth - 1) do
                      ".." ]
                |> String.concat "/"
            | _ -> "."


        [ yield! a.Content.CreateValues()
          "sections",
          Mustache.Value.Array
              [ [ "collection_title", Mustache.Value.Scalar "Parts"
                  "parts", article.AllParts |> List.map createPartData |> Mustache.Value.Array ]
                |> Map.ofList
                |> Mustache.Value.Object ]
          "share_links", createShareLinksData "" "" ""
          "tags", createTagsData []
          "url", Mustache.Value.Scalar a.Url
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

          "links", createLinkData []

          "gh_issue_link", Mustache.Value.Scalar <| createIssueLink article.Title
          match article.Image with
          | Some rai ->
              "image", Mustache.Value.Scalar $"{urlDepth}/img/{rai.Name}"
              "preview_image", Mustache.Value.Scalar $"{urlDepth}/img/{rai.PreviewName}"
              "thanks", Mustache.Value.Scalar rai.Thanks
          | None -> ()
          
          match article.RawLink with
          | Some rawLink -> "raw_link", Mustache.Value.Scalar rawLink
          | None -> ()
          match article.OverrideCssName with
          | Some name ->
              let url =
                  match name.EndsWith(".css") with
                  | true -> $"{urlDepth}/{name}"
                  | false -> $"{urlDepth}/{name}.css"

              "override_css", [ "css_url", Mustache.Value.Scalar url ] |> Map.ofList |> Mustache.Object
          | None -> () ]

    let renderPage () = ()


    let run _ =
        use ctx = SqliteContext.Open ""






        ()
