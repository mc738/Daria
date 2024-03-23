namespace Daria.App.Common

open System
open FsToolbox.AppEnvironment.Args

module Options =

    open FsToolbox.AppEnvironment.Args.Mapping

    type AppOptions = | [<CommandValue("import")>] Import of ImportOptions

    and ImportOptions =
        { [<ArgValue("-p", "--path")>]
          SettingsPath: string
          [<ArgValue("-v", "--verbose")>]
          Verbose: bool }


    let getOptions _ =
        Environment.GetCommandLineArgs()
        |> List.ofArray
        |> ArgParser.tryGetOptions<AppOptions>