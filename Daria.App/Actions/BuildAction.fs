namespace Daria.App.Actions

open Daria.App.Common.Options
open Daria.V2.Operations
open Daria.V2.Operations.Build

module BuildAction =
    
    let run (options: BuildOptions) =
        
        run options.DataStorePath
    

