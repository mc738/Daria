namespace Daria.V1

open System
open Daria
open FDOM.Core.Common
open FDOM.Core.Parsing
open FDOM.Rendering
open Fluff.Core


[<AutoOpen>]
module Common =

    [<RequireQualifiedAccess>]
    module Utils =

        let rawText (content: DOM.InlineContent list) =
            content
            |> List.map (fun c ->
                match c with
                | DOM.InlineContent.Text t -> t.Content
                | DOM.InlineContent.Span s -> s.Content
                | DOM.InlineContent.Link l -> l.Content)
            |> String.concat ""

        let createIssueLink (title: string) =
            title.Replace(" ", "+").Replace("#", "%23")
            |> fun s -> $"https://github.com/mc738/Articles/issues/new?title=`{s}`+issue"

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

    type ArticleContent =
        { Title: DOM.HeaderBlock
          TitleText: string
          Description: DOM.ParagraphBlock
          DescriptionText: string
          Document: DOM.Document }

        static member Create(lines: string list) =
            let blocks =
                Parser.ParseLines(lines).CreateBlockContent()
            
            let (titleBlock, descriptionBlock, content) =
                blocks.[0], blocks.[1], blocks.[2..]

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

            { Title = title
              TitleText = Utils.rawText title.Content
              Description = description
              DescriptionText = Utils.rawText description.Content
              Document = doc }

        member a.CreateValues() =
            [ "title", Mustache.Value.Scalar <| Html.renderTitle a.Title
              "title_text", Mustache.Value.Scalar a.TitleText
              "description",
              Mustache.Value.Scalar
              <| Html.renderDescription a.Description
              "description_text", Mustache.Value.Scalar a.DescriptionText ]

    type Article =
        { Content: ArticleContent
          Version: int
          Image: string
          ImagePreview: string
          PublishDate: DateTime
          Url: string
          Parts: Part list
          NextPart: Part option
          PreviousPart: Part option
          ShareLinks: ShareLink list
          Links: Link list
          Tags: string list
          Thanks: string
          RawLink: string
          OverrideCssUrl: string option}

        member a.CreateValues() =
            let tags =
                a.Tags
                |> List.map (fun t ->
                    [ "tag_name", Mustache.Value.Scalar t ]
                    |> Map.ofList
                    |> Mustache.Value.Object)

            [ yield! a.Content.CreateValues()
              "sections",
              Mustache.Value.Array [ [ "collection_title", Mustache.Value.Scalar "Parts"
                                       "parts", Mustache.Value.Array(a.Parts |> List.map (fun p -> p.CreateValues())) ]
                                     |> Map.ofList
                                     |> Mustache.Value.Object ]
              "share_links",
              Mustache.Array(
                  a.ShareLinks
                  |> List.map (fun sl -> sl.CreateValue())
              )
              "tags", Mustache.Value.Array tags
              "url", Mustache.Value.Scalar a.Url
              "version", Mustache.Value.Scalar <| string a.Version
              "article_date",
              Mustache.Value.Scalar
              <| a.PublishDate.ToString("dd MMMM yyyy")
              "image", Mustache.Value.Scalar a.Image
              "preview_image", Mustache.Value.Scalar a.ImagePreview
              "now", Mustache.Value.Scalar(DateTime.Now.ToString("dd MMMM yyyy 'at' HH:mm:ss"))
              match a.PreviousPart with
              | Some pp -> "previous_part", pp.CreateValues()
              | None -> ()

              match a.NextPart with
              | Some np -> "next_part", np.CreateValues()
              | None -> ()

              "links", Mustache.Value.Array(a.Links |> List.map (fun l -> l.CreateValue()))

              "gh_issue_link",
              Mustache.Value.Scalar
              <| Utils.createIssueLink a.Content.TitleText
              "thanks", Mustache.Value.Scalar a.Thanks
              "raw_link", Mustache.Value.Scalar a.RawLink
              match a.OverrideCssUrl with
              | Some url -> "override_css", [ "css_url", Mustache.Value.Scalar url ] |> Map.ofList |> Mustache.Object
              | None -> () ]

    and Part =
        { Title: string
          PartNumber: int
          Url: string }

        member p.CreateValues() =
            [ "part_title", Mustache.Value.Scalar p.Title
              "part_url", Mustache.Value.Scalar p.Url ]
            |> Map.ofList
            |> Mustache.Value.Object

    and ShareLink =
        { Icon: string
          Url: string
          Title: string }

        member sl.CreateValue() =
            [ "icon", Mustache.Value.Scalar sl.Icon
              "link_url", Mustache.Value.Scalar sl.Url
              "link_title", Mustache.Value.Scalar sl.Title ]
            |> Map.ofList
            |> Mustache.Value.Object

    and Link =
        { Title: string
          Description: string
          Url: string }

        member l.CreateValue() =
            [ "link_title", Mustache.Value.Scalar l.Title
              "link_description", Mustache.Value.Scalar l.Description
              "link_url", Mustache.Value.Scalar l.Url ]
            |> Map.ofList
            |> Mustache.Value.Object
