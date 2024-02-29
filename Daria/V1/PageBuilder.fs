namespace Daria.V1

open Daria.V1.Persistence
open Fluff.Core
open Freql.Sqlite

module PageBuilder =

    let createPageItem (ctx: SqliteContext) (series: Records.Series) =
        let tags =
            DataStore.getSeriesTags ctx series.Name

        [ "item_name", Mustache.Value.Scalar series.Name
          "item_description", Mustache.Value.Scalar series.Description
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
              [ "tag", Mustache.Value.Scalar t.Tag ]
              |> Map.ofList
              |> Mustache.Value.Object)
          |> Mustache.Array ]
        |> Map.ofList
        |> Mustache.Value.Object

    let createPageItems (ctx: SqliteContext) (isFirst: bool) (isLast: bool) (index: int) (series: Records.Series list) =

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