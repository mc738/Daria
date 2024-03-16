open FsToolbox.AppEnvironment.Args.Mapping

type AppOptions = | [<CommandValue("import")>] Import of ImportOptions

and ImportOptions =
    { [<ArgValue("-p", "--path")>]
      SettingsPath: string }

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
