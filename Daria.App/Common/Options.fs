namespace Daria.App.Common

open System
open FsToolbox.AppEnvironment.Args

module Options =

    open FsToolbox.AppEnvironment.Args.Mapping

    type AppOptions =
        | [<CommandValue("import")>] Import of ImportOptions
        | [<CommandValue("build")>] Build of BuildOptions

    and ImportOptions =
        { [<ArgValue("-s", "--settings")>]
          SettingsPath: string
          [<ArgValue("-v", "--verbose")>]
          Verbose: bool }
        
    and BuildOptions =
        {
            [<ArgValue("-s", "--settings")>]
            SettingPath: string
            [<ArgValue("-p", "--profile")>]
            ProfileName: string
        }


    let getOptions _ =
        Environment.GetCommandLineArgs()
        |> List.ofArray
        |> ArgParser.tryGetOptions<AppOptions>