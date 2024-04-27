namespace Daria.V2.Operations.Build

open System
open Daria.V2.Operations.Common

[<AutoOpen>]
module Impl =

    open System.IO
    open Fluff.Core
    open Freql.Sqlite
    open Daria.V2.DataStore
    open Daria.V2.Operations.Build.PageRenderer

    type BuildOperationFailure =
        | SettingsError of Message: string
        | StoreFailure of Path: string
        | ProfileNotFound of ProfileName: string
        | LoadTemplateSourceFailure of Message: string * Exception: Exception option
        | UnhandledException of Message: string * Exception: Exception

    let ``load settings and get profile`` (settingsPath: string) (profile: string) =
        match OperationSettings.Load settingsPath with
        | Ok settings ->
            match settings.Build.Profiles |> List.tryFind (fun bp -> bp.Name = profile) with
            | Some profile -> Ok(settings, profile)
            | None -> BuildOperationFailure.ProfileNotFound profile |> Error
        | Error e -> BuildOperationFailure.SettingsError e |> Error

    let loadPageTemplate (ctx: SqliteContext) (template: BuildTemplateSource) =
        try
            match template with
            | BuildTemplateSource.Store(id, version) ->
                Templates.getLatestVersion ctx 


                failwith "todo"
            | BuildTemplateSource.File path ->
                match File.Exists path with
                | true -> File.ReadAllText path |> Ok
                | false -> BuildOperationFailure.LoadTemplateSourceFailure($"File `{path}` not found", None) |> Error
                
            |> Result.map Mustache.parse
        with ex ->
            BuildOperationFailure.LoadTemplateSourceFailure(ex.Message, Some ex) |> Error

    let run (settingsPath: string) (profile: string) =
        try
            ``load settings and get profile`` settingsPath profile
            |> Result.bind (fun (settings, profile) ->
                match File.Exists settings.Common.StorePath with
                | true ->
                    use ctx = SqliteContext.Open settings.Common.StorePath




                    Ok()
                | false -> BuildOperationFailure.StoreFailure settings.Common.StorePath |> Error)

            match OperationSettings.Load settingsPath with
            | Ok settings ->
                match settings.Build.Profiles |> List.tryFind (fun bp -> bp.Name = profile) with
                | Some profile ->
                    use ctx = SqliteContext.Open storePath

                    let rootPath = "C:\\ProjectData\\Articles\\_rendered_v2"
                    let url = "https://blog.psionic.cloud/"

                    let pageTemplate =
                        File.ReadAllText "C:\\Users\\44748\\Projects\\Daria\\Resources\\templates\\article.mustache"
                        |> Mustache.parse

                    let seriesIndexTemplate =
                        File.ReadAllText
                            "C:\\Users\\44748\\Projects\\Daria\\Resources\\templates\\series_index.mustache"
                        |> Mustache.parse

                    let indexTemplate =
                        File.ReadAllText "C:\\Users\\44748\\Projects\\Daria\\Resources\\templates\\index.mustache"
                        |> Mustache.parse

                    Series.getTopLevelRenderableSeries ctx
                    |> List.iter (Series.renderSeries ctx pageTemplate seriesIndexTemplate 1 url rootPath)

                    Index.renderIndex ctx indexTemplate rootPath

                    ExportResources.exportImages ctx rootPath

                    BuildOperationResult.Success
                | None -> BuildOperationFailure.ProfileNotFound profile |> BuildOperationResult.Failure
            | Error e -> BuildOperationFailure.SettingsError e |> BuildOperationResult.Failure
        with ex ->
            Error(BuildOperationFailure.UnhandledException(ex.Message, ex))
