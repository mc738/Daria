namespace Daria.V2.Operations.Build

open System
open Daria.V2.DataStore.Persistence
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
        | PreBuildFailure of Message: string * Exception: Exception option
        | BuildFailure of Message: string * Exception: Exception option
        | PostBuildFailure of Message: string * Exception: Exception option
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
                match version with
                | ItemVersion.Latest -> Templates.getLatestVersionExportListItem ctx id
                | ItemVersion.Specific version -> Templates.getSpecificVersionExportListItem ctx id version
                |> Option.bind (fun tv -> Resources.fetchVersionDataAsUtf8 ctx tv.ResourceVersionId)
                |> function
                    | Some t -> Ok t
                    | None ->
                        BuildOperationFailure.LoadTemplateSourceFailure(
                            $"Template `{id}` not found or could not be loaded",
                            None
                        )
                        |> Error
            | BuildTemplateSource.File path ->
                match File.Exists path with
                | true -> File.ReadAllText path |> Ok
                | false ->
                    BuildOperationFailure.LoadTemplateSourceFailure($"File `{path}` not found", None)
                    |> Error

            |> Result.map Mustache.parse
        with ex ->
            BuildOperationFailure.LoadTemplateSourceFailure(ex.Message, Some ex) |> Error

    type BuildContext =
        { StoreContext: SqliteContext
          Settings: OperationSettings
          Profile: BuildProfileSettings
          Templates: Templates }

    and Templates =
        { Articles: Mustache.Token list
          Series: Mustache.Token list
          Index: Mustache.Token list }

    let runBuildStep (ctx: SqliteContext) (buildStep: BuildStep) = ()

    let createBuildContext (ctx: SqliteContext) (settings: OperationSettings) (profile: BuildProfileSettings) =
        match
            loadPageTemplate ctx profile.ArticlesTemplateSource,
            loadPageTemplate ctx profile.SeriesTemplateSource,
            loadPageTemplate ctx profile.IndexTemplateSource
        with
        | Ok articleTemplate, Ok seriesTemplate, Ok indexTemplate ->

            { StoreContext = ctx
              Settings = settings
              Profile = profile
              Templates =
                { Articles = articleTemplate
                  Series = seriesTemplate
                  Index = indexTemplate } }
            |> Ok
        | Error e, _, _
        | _, Error e, _
        | _, _, Error e -> Error e

    let runPreBuildSteps (buildContext: BuildContext) =
        try

            Ok()
        with ex ->
            BuildOperationFailure.PreBuildFailure(ex.Message, Some ex) |> Error

    let build (ctx: BuildContext) =
        try
            // Run pre build steps

            Series.getTopLevelRenderableSeries ctx.StoreContext
            |> List.iter (
                Series.renderSeries
                    ctx.StoreContext
                    ctx.Templates.Articles
                    ctx.Templates.Series
                    1
                    ctx.Profile.Url
                    ctx.Profile.RootPath
            )

            Index.renderIndex ctx.StoreContext ctx.Templates.Articles ctx.Profile.RootPath

            ExportResources.exportImages ctx.StoreContext ctx.Profile.RootPath

            // Result post build steps

            Ok()
        with ex ->
            BuildFailure(ex.Message, Some ex) |> Error


    let runPostBuildSteps (buildContext: BuildContext) =
        try

            Ok()
        with ex ->
            BuildOperationFailure.PostBuildFailure(ex.Message, Some ex) |> Error

    let run (settingsPath: string) (profile: string) =
        try
            ``load settings and get profile`` settingsPath profile
            |> Result.bind (fun (settings, profile) ->
                match File.Exists settings.Common.StorePath with
                | true ->
                    use ctx = SqliteContext.Open settings.Common.StorePath

                    match createBuildContext ctx settings profile with
                    | Ok buildCtx ->
                        // Run pre build steps
                        runPreBuildSteps buildCtx
                        |> Result.bind (fun _ -> build buildCtx)
                        |> Result.bind (fun _ -> runPostBuildSteps buildCtx)
                    | Error e -> Error e
                | false -> BuildOperationFailure.StoreFailure settings.Common.StorePath |> Error)
        with ex ->
            Error(BuildOperationFailure.UnhandledException(ex.Message, ex))
