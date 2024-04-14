namespace Daria.V2.Operations.Build

[<AutoOpen>]
module Impl =

    open System.IO
    open Fluff.Core
    open Freql.Sqlite
    open Daria.V2.DataStore
    open Daria.V2.Operations.Build.PageRenderer
    
    let run storePath =
        use ctx = SqliteContext.Open storePath

        let rootPath = "C:\\ProjectData\\Articles\\_rendered_v2"
        let url = "https://blog.psionic.cloud/"

        let pageTemplate =
            File.ReadAllText "C:\\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache"
            |> Mustache.parse

        let seriesIndexTemplate =
            File.ReadAllText "C:\\Users\\44748\\Projects\\Daria\\Resources\\templates\\series_index.mustache"
            |> Mustache.parse

        let indexTemplate =
            File.ReadAllText "C:\\Users\\44748\\Projects\\Daria\\Resources\\templates\\index.mustache"
            |> Mustache.parse

        Series.getTopLevelRenderableSeries ctx
        |> List.iter (Series.renderSeries ctx pageTemplate seriesIndexTemplate 1 url rootPath)

        Index.renderIndex ctx indexTemplate rootPath

    
    ()

