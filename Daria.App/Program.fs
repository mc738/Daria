open System
open System.IO
open System.Text.Json.Serialization
open Daria.V1
open FDOM.Core.Common
open FDOM.Core.Dsl
open FDOM.Core.Parsing
open FDOM.Rendering
open Fluff.Core
open Freql.Sqlite

(*
type Article =
    { Title: DOM.HeaderBlock
      Description: DOM.ParagraphBlock
      Document: DOM.Document }

[<CLIMutable>]
type ArticleConfiguration =
    { Image: string
      ImagePreview: string
      Parts: Part list
      NextPart: Part option
      PreviousPart: Part option
      ShareLinks: ShareLink list
      Links: Links list
      Tags: string list }

and Part = { Name: string; Url: string }

and ShareLink = { Icon: string; Url: string }

and Links = { Title: string; Url: string }

let parse path =
    let lines =
        File.ReadAllLines path |> List.ofArray

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
      Description = description
      Document = doc }

let loadConfig (series: string) (part: string) =
    
    ()

let renderHtml (template: string) (article: Article) =

    let parsedTemplate =
        Mustache.parse (File.ReadAllText template)

    let rawText (content: DOM.InlineContent list) =
        content
        |> List.map (fun c ->
            match c with
            | DOM.InlineContent.Text t -> t.Content
            | DOM.InlineContent.Span s -> s.Content)
        |> String.concat ""

    let titleText =
        rawText article.Title.Content

    let descriptionText =
        rawText article.Description.Content

    let values =
        ({ Values =
            [ "title",
              Mustache.Value.Scalar
              <| Html.renderTitle article.Title
              "title_text", Mustache.Value.Scalar titleText
              "description",
              Mustache.Value.Scalar
              <| Html.renderDescription article.Description
              "description_text", Mustache.Value.Scalar descriptionText
              //"titleSlug", Mustache.Value.Scalar action.TitleSlug
              "has_parts", Mustache.Value.Scalar "true"
              "sections",
              Mustache.Value.Array [ [ "collection_title", Mustache.Value.Scalar "Parts"
                                       "parts",
                                       Mustache.Value.Array [ [ "part_title", Mustache.Value.Scalar "Introduction"
                                                                "part_url", Mustache.Value.Scalar "#" ]
                                                              |> Map.ofList
                                                              |> Mustache.Value.Object ] ]
                                     |> Map.ofList
                                     |> Mustache.Value.Object ]
              //"parts",
              //Mustache.Value.Array [ [ "part_title", Mustache.Value.Scalar "Introduction"; "part_url", Mustache.Value.Scalar "#" ] |> Map.ofList |> Mustache.Value.Object ]
              "tags",
              Mustache.Value.Array [ [ "tag_name", Mustache.Value.Scalar "fsharp" ]
                                     |> Map.ofList
                                     |> Mustache.Value.Object
                                     [ "tag_name", Mustache.Value.Scalar "csv" ]
                                     |> Map.ofList
                                     |> Mustache.Value.Object ]
              "url", Mustache.Value.Scalar "https://blog.psionic.cloud/writing_a_csv_parser_in_fsharp/part1.html"
              "article_date",
              Mustache.Value.Scalar
              <| DateTime.UtcNow.ToString("dd MMMM yyyy")
              "image", Mustache.Value.Scalar "https://blog.psionic.cloud/img/ken-cheung-KonWFWUaAuk-unsplash.jpg"
              "preview_image",
              Mustache.Value.Scalar "http://psionic.cloud-public.eu-central-1.linodeobjects.com/twitter.jpg"
              "now", Mustache.Value.Scalar(DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss"))
              "gh_link",
              Mustache.Value.Scalar
                  "https://github.com/mc738/Articles/blob/master/FSharp_CSV_parser/writing_a_csv_parser_in_fsharp.md"
              "gh_issue_title",
              Mustache.Value.Scalar
              <| (titleText.Replace(" ", "+").Replace("#", "%23")
                  |> fun s -> $"`{s}` issue") ]
            // @ (action.Metadata
            //    |> Map.toList
            //    |> List.map (fun (k, v) -> k, Mustache.Value.Scalar v))
            |> Map.ofList
           Partials = Map.empty }: Mustache.Data)

    Html.renderFromParsedTemplate parsedTemplate values [] [] article.Document
*)
let saveFile (path: string) (file: string) = File.WriteAllText(path, file)


let test (ctx: SqliteContext) (title: string) (template) (savePath: string) =
    
    let parsedTemplate =
        Mustache.parse (File.ReadAllText template)
    
    let render (article: Article) =
        let data = ({
            Values = article.CreateValues() |> Map.ofList //|> Mustache.Value.Object
            Partials = Map.empty
        }: Mustache.Data)
        
        Html.renderFromParsedTemplate parsedTemplate data [] [] article.Content.Document
        
    DataStore.getLatestArticleVersion ctx title
    |> Option.map (fun a -> render a |> saveFile savePath)
    |> Option.defaultWith (fun _ -> failwith "Error loading version")
  
let ctx = SqliteContext.Open("C:\\ProjectData\\Articles\\daria.db")
 
let template = File.ReadAllText "C:\\Users\\44748\\Projects\\Daria\\Resources\\templates\\index.mustache" |> Mustache.parse
 
PageBuilder.buildIndex template ctx |> fun r -> File.WriteAllText("C:\\ProjectData\\Articles\\_rendered\\index_test.html", r)
    
//test ctx "Writing a CSV parser in F# - Introduction" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\writing_a_csv_parser_in_fsharp\\part1.html"
//test ctx "Writing a CSV parser in F# - Basic parsing" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\writing_a_csv_parser_in_fsharp\\part2.html"
//test ctx "Writing a CSV parser in F# - Record building" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\writing_a_csv_parser_in_fsharp\\part3.html"
//test ctx "Save points in F# - Introduction" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\save_points_in_fsharp\\part1.html"
//test ctx "Adding images to FDOM" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\general\\adding_images_to_fdom.html"
//test ctx "Create a basic file watcher in F#" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\general\\create_a_basic_file_watcher_in_fsharp.html"
//test ctx "Build a blog - Introduction" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\build_a_blog\\part1.html"

//test ctx "Writing a shell in F# - Introduction" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\writing_a_shell_in_fsharp\\part1.html"
//test ctx "Writing a shell in F# - Pipes and CoreUtils" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\writing_a_shell_in_fsharp\\part2.html"
//test ctx "Writing a shell in F# - Input controller" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\writing_a_shell_in_fsharp\\part3.html"
//test ctx "Writing a shell in F# - Parsing and syntax highlighting" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\\writing_a_shell_in_fsharp\\part4.html"

test ctx "Writing a dev server in rust - Introduction" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\writing_a_dev_server_in_rust\\part1.html"
test ctx "Writing a dev server in rust - Logging" "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache" "C:\\ProjectData\\Articles\\_rendered\writing_a_dev_server_in_rust\\part2.html"
//C:\ProjectData\Articles\_rendered\writing_a_dev_server_in_rust

// Writing a shell in F# - Parsing and syntax highlighting

(*
parse "C:\\ProjectData\\Articles\\FSharp_CSV_parser\\part2.md"
|> renderHtml "C:\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache"
|> saveFile "C:\\ProjectData\\Articles\\_rendered\\writing_a_csv_parser_in_fsharp\\part2.html"
*)

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"