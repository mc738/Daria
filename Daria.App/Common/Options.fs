namespace Daria.App.Common

open System
open FsToolbox.AppEnvironment.Args

module Options =

    open FsToolbox.AppEnvironment.Args.Mapping

    type AppOptions =
        | [<CommandValue("import")>] Import of ImportOptions
        | [<CommandValue("build")>] Build of BuildOptions

    and ImportOptions =
        { [<ArgValue("-p", "--path")>]
          SettingsPath: string
          [<ArgValue("-v", "--verbose")>]
          Verbose: bool }
        
    and BuildOptions =
        {
            [<ArgValue("-s", "--datastore")>]
            DataStorePath: string
        }


    let getOptions _ =
        Environment.GetCommandLineArgs()
        |> List.ofArray
        |> ArgParser.tryGetOptions<AppOptions>