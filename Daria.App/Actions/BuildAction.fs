namespace Daria.App.Actions

open Daria.App.Common.Options
open Daria.V2.Operations

module BuildAction =
    
    let run (options: BuildOptions) =
        PageRenderer.run options.DataStorePath
    

