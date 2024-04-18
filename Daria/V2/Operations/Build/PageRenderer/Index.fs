namespace Daria.V2.Operations.Build.PageRenderer

module Index =

    open System.IO
    open Fluff.Core
    open Freql.Sqlite
    open Daria.V2.DataStore
    open Daria.V2.DataStore.Models
        
    let createPageItem (ctx: SqliteContext) (series: RenderableSeriesIndex) =
        [ "item_name", Mustache.Value.Scalar series.Title
          "item_description", Mustache.Value.Scalar series.Description
          match series.Image with
          | Some i -> "item_image", Mustache.Value.Scalar $"./img/{i.GetName()}" // NOTE this assumes the image will be in the img directory.
          | None -> ()
          "item_link",
          [ "url", Mustache.Scalar $"./{series.TitleSlug}/index.html" ]
          |> Map.ofList
          |> Mustache.Object
          "item_tags",
          series.Tags
          |> List.map (fun t -> [ "tag", Mustache.Value.Scalar t ] |> Map.ofList |> Mustache.Value.Object)
          |> Mustache.Array ]
        |> Map.ofList
        |> Mustache.Value.Object

    let createPageItems
        (ctx: SqliteContext)
        (isFirst: bool)
        (isLast: bool)
        (index: int)
        (series: RenderableSeriesIndex list)
        =

        [ "page_id", Mustache.Value.Scalar $"page-{index + 1}"
          "page_name", Mustache.Value.Scalar $"Page {index + 1}"
          "page_items", series |> List.map (createPageItem ctx) |> Mustache.Value.Array
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

    let createPages (ctx: SqliteContext) (topLevelSeries: RenderableSeriesIndex list) =
        let series = topLevelSeries |> List.chunkBySize 4

        series
        |> List.mapi (fun i s -> createPageItems ctx (i = 0) (i = series.Length - 1) i s)
        |> Mustache.Value.Array

    let createLatestPost (ctx: SqliteContext) (article: RenderableArticle) =
        [ "post_name", Mustache.Value.Scalar article.Title
          "post_description", Mustache.Value.Scalar article.Description
          "post_date",
          article.PublishedOn
          |> Option.defaultValue article.CreatedOn
          |> fun dt -> dt.ToString("dd MMMM yyyy")
          |> Mustache.Value.Scalar

          match article.Image with
          | Some pi -> "post_image", Mustache.Value.Scalar $"./img/{pi.GetName()}"
          | None -> ()

          match Articles.createArticleLinkParts ctx article.VersionId with
          | Some ap ->
              "post_link",
              // Need way to create title slug link.
              String.concat "/" ap |> (fun r -> $"./{r}") |> Mustache.Value.Scalar
          | None -> () ]
        |> Map.ofList
        |> Mustache.Value.Object

    let createLatestPosts (ctx: SqliteContext) =
        Articles.getLatestCreatedRenderableArticles ctx 3
        |> List.map (createLatestPost ctx)
        |> Mustache.Value.Array

    let createLink (ctx: SqliteContext) (series: RenderableSeriesIndex) =
        [ "series_name", Mustache.Value.Scalar series.TitleSlug
          "series_url", Mustache.Value.Scalar $"./{series.TitleSlug}/index.html" ]
        |> Map.ofList
        |> Mustache.Value.Object

    let createLinks (ctx: SqliteContext) (topLevelSeries: RenderableSeriesIndex list) =
        topLevelSeries |> List.map (createLink ctx) |> Mustache.Value.Array

    let buildIndex (template: Mustache.Token list) (ctx: SqliteContext) =

        let topLevelSeries = Series.getTopLevelRenderableSeries ctx

        let data =
            ({ Values =
                [ "pages", createPages ctx topLevelSeries
                  "recent_posts", createLatestPosts ctx
                  "series", createLinks ctx topLevelSeries ]
                |> Map.ofList
               Partials = Map.empty }
            : Mustache.Data)

        Mustache.replace data true template

    let renderIndex (ctx: SqliteContext) (template: Mustache.Token list) (savePath: string) =
        buildIndex template ctx
        |> fun r -> File.WriteAllText(Path.Combine(savePath, "index.html"), r)
