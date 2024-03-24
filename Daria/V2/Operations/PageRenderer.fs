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

    let createPartsData title url =
        [ "part_title", Mustache.Value.Scalar title
          "part_url", Mustache.Value.Scalar url ]
        |> Map.ofList
        |> Mustache.Value.Object

    let createPageData _ =
        [ yield! a.Content.CreateValues()
          "sections",
          Mustache.Value.Array
              [ [ "collection_title", Mustache.Value.Scalar "Parts"
                  "parts", Mustache.Value.Array(a.Parts |> List.map (fun p -> p.CreateValues())) ]
                |> Map.ofList
                |> Mustache.Value.Object ]
          "share_links", createShareLinksData "" "" ""
          "tags", createTagsData []
          "url", Mustache.Value.Scalar a.Url
          "version", Mustache.Value.Scalar <| string a.Version
          "article_date", Mustache.Value.Scalar <| a.PublishDate.ToString("dd MMMM yyyy")
          "image", Mustache.Value.Scalar a.Image
          "preview_image", Mustache.Value.Scalar a.ImagePreview
          "now", Mustache.Value.Scalar(DateTime.Now.ToString("dd MMMM yyyy 'at' HH:mm:ss"))
          match a.PreviousPart with
          | Some pp -> "previous_part", pp.CreateValues()
          | None -> ()

          match a.NextPart with
          | Some np -> "next_part", np.CreateValues()
          | None -> ()

          "links", createLinkData []

          "gh_issue_link", Mustache.Value.Scalar <| createIssueLink a.Content.TitleText
          "thanks", Mustache.Value.Scalar a.Thanks
          "raw_link", Mustache.Value.Scalar a.RawLink
          match a.OverrideCssUrl with
          | Some url -> "override_css", [ "css_url", Mustache.Value.Scalar url ] |> Map.ofList |> Mustache.Object
          | None -> () ]

    let renderPage () = ()


    let run _ =
        use ctx = SqliteContext.Open ""






        ()
