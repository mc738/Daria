namespace Daria.App.Common

module Options =

    open FsToolbox.AppEnvironment.Args.Mapping

    type AppOptions = | [<CommandValue("import")>] Import of ImportOptions

    and ImportOptions =
        { [<ArgValue("-p", "--path")>]
          SettingsPath: string }
