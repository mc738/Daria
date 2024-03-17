namespace Daria.App.Actions

open Daria.App.Common.Options
open Daria.V2.Operations

module ImportAction =

    open Daria.V2.Operations

    let run (options: ImportOptions) =

        match Import.run options.SettingsPath with
        | Ok results ->
            match options.Verbose with
            | true ->
                let rec output (success: int) (skipped: int) (r: Import.ImportDirectoryResult) =
                    match r with
                    | Import.Success importDirectorySuccessResult ->
                        let (innerSuccess, innerSkipped) =
                            importDirectorySuccessResult.ChildrenResults
                            |> List.map (count (success + 1) skipped)
                            |> List.unzip
                            
                        

                        (success + 1, skipped)
                    | Import.Skipped(path, reason) -> (success, skipped + 1)


                    ()
                ()
            | false ->
                let rec count (success: int) (skipped: int) (r: Import.ImportDirectoryResult) =
                    match r with
                    | Import.Success importDirectorySuccessResult ->
                        let (innerSuccess, innerSkipped) =
                            importDirectorySuccessResult.ChildrenResults
                            |> List.map (count (success + 1) skipped)
                            |> List.unzip
                            
                        

                        (success + 1, skipped)
                    | Import.Skipped(path, reason) -> (success, skipped + 1)


                    ()



                ()

            ()
        | Error e -> printfn $"Error importing pages. {e}"
