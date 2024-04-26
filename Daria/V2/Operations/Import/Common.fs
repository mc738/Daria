namespace Daria.V2.Operations.Import

open System

[<AutoOpen>]
module Common =

    open System.IO
    open System.Text.RegularExpressions
    open System.Text.Json
    open FsToolbox.Core
    open FDOM.Core.Parsing
    open FDOM.Core.Parsing.BlockParser
    open Daria.V2.DataStore
    open Daria.V2.DataStore.Common

    type ImportResult = { Path: string; Result: AddResult }

    type [<RequireQualifiedAccess>] ImportDirectoryResult =
        | Success of ImportDirectorySuccessResult
        | Skipped of Path: string * Reason: string

    and ImportDirectorySuccessResult =
        { Path: string
          IndexResult: AddResult
          Results: ImportResult list
          ChildrenResults: ImportDirectoryResult list }

    and [<RequireQualifiedAccess>] ImportResourcesResult =
        | Success of ImportResourcesSuccessResult
        | Failure of Message: string * Exception: exn option
    
    and ImportResourcesSuccessResult = {
        ImageResults: ImportResult list
        ResourceBucketResults: ImportResult list
        ExternalTemplatesResults: ImportResult list
    }
    
    and ImportActionResults =
        { Directories: ImportDirectoryResult list
          Resources: ImportResourcesResult }

    [<RequireQualifiedAccess>]
    module TokenExtractor =

        type State = { Input: Input; CurrentLine: int }

        let next (state: State) =
            BlockParser.tryParseBlock state.Input state.CurrentLine
            |> Option.map (fun (bt, i) -> bt, { state with CurrentLine = i })

        let tryFindNext (fn: BlockToken -> bool) (state: State) =
            let rec handler (i: int) =
                match tryParseBlock state.Input state.CurrentLine with
                | Some(bt, newI) ->
                    match fn bt with
                    | true -> Some(bt, { state with CurrentLine = newI })
                    | false -> handler newI
                | None -> None

            handler state.CurrentLine

    let tryGetTitleAndDescription (lines: string list) =
        let state =
            ({ Input = Input.Create lines
               CurrentLine = 0 }
            : TokenExtractor.State)

        match
            state
            |> TokenExtractor.tryFindNext (fun bt ->
                match bt with
                | BlockToken.Header _ -> true
                | _ -> false)
        with
        | Some(hbt, newState) ->
            let headerContent =
                match hbt with
                | BlockToken.Header ht -> Some ht
                | _ -> None
                |> Option.bind (fun ht -> ht.Split(' ', 2) |> Array.tryItem 1)

            match
                newState
                |> TokenExtractor.tryFindNext (fun bt ->
                    match bt with
                    | BlockToken.Paragraph _ -> true
                    | _ -> false)
            with
            | Some(pbt, _) ->
                headerContent,
                match pbt with
                | BlockToken.Paragraph pt -> Some pt
                | _ -> None
            | None -> headerContent, None
        | None -> None, None
