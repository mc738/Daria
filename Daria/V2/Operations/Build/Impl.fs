namespace Daria.V2.Operations.Build

open Daria.V2.Operations.Common

[<AutoOpen>]
module Impl =

    open System.IO
    open Fluff.Core
    open Freql.Sqlite
    open Daria.V2.DataStore
    open Daria.V2.Operations.Build.PageRenderer

    [<RequireQualifiedAccess>]
    type BuildOperationResult =
        | Success
        | Failure of BuildOperationFailure

    and BuildOperationFailure =
        | SettingsError of Message: string
        | ProfileNotFound of ProfileName: string
        

    let run (settingsPath: string) (profile: string) =
        match OperationSettings.Load settingsPath with
        | Ok settings ->
            match settings.Build.Profiles |> List.tryFind (fun bp -> bp.Name = profile) with
            | Some profile -> BuildOperationResult.Success
            | None -> BuildOperationFailure.SettingsError e |> BuildOperationResult.Failure
        | Error e ->
            BuildOperationFailure.SettingsError e |> BuildOperationResult.Failure

        
        

        OperationSettings.Load settingsPath
        |> Result.bind (fun settings ->


            ())
        |> Result.map (fun (settings, profile) ->


            ())

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

        ExportResources.exportImages ctx rootPath

    ()
