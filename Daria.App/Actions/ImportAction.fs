namespace Daria.App.Actions

open Daria.App.Common.Options
open Daria.V2.DataStore.Common
open Daria.V2.Operations
open Daria.V2.Operations.Import

module ImportAction =

    open Daria.V2.Operations

    let run (options: ImportOptions) =

        match run options.SettingsPath with
        | Ok results ->
            match options.Verbose with
            | true ->
                let rec output (success: int) (skipped: int) (indent: int) (r: ImportDirectoryResult) =
                    let indentStr = System.String(' ', indent)

                    match r with
                    | Success importDirectorySuccessResult ->
                        printfn $"{indentStr}Success: {importDirectorySuccessResult.Path}"

                        importDirectorySuccessResult.Results
                        |> List.iter (fun r ->
                            match r.Result with
                            | AddResult.Success id -> printfn $"{indentStr}{r.Path} - Success"
                            | AddResult.NoChange id -> printfn $"{indentStr}{r.Path} - No change"
                            | AddResult.AlreadyExists id -> printfn $"{indentStr}{r.Path} - Already exists"
                            | AddResult.MissingRelatedEntity(entityType, id) ->
                                printfn $"{indentStr}{r.Path} - Missing related entity ({entityType} - {id})"
                            | AddResult.Failure(message, ``exception``) ->
                                printfn $"{indentStr}{r.Path} - Failure: {message}")

                        let (innerSuccess, innerSkipped) =
                            importDirectorySuccessResult.ChildrenResults
                            |> List.map (output (success + 1) skipped (indent + 1))
                            |> List.unzip

                        (success + 1, skipped)
                    | Skipped(path, reason) ->
                        printfn $"{indentStr}Skipped ({reason}): {path}"
                        (success, skipped + 1)

                let (success, skipped) = output 0 0 0 results
                
                printfn $"Success: {success} Skipped: {skipped}"
            | false ->
                failwith "Non-verbose mode not implemented yet."
                (*
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
                *)



                ()

            ()
        | Error e -> printfn $"Error importing pages. {e}"
