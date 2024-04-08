namespace Daria.V2.Operations.Import

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
    
    type Settings =
        { StorePath: string
          ArticlesRoot: string
          ResourcesRoot: string
          DirectoryIgnorePatterns: Regex list
          FileIgnorePatterns: Regex list
          DateTimeFormats: string list
          IndexFileName: string }

        static member Load(path: string) =
            try
                match File.Exists path with
                | true ->
                    (File.ReadAllText path |> JsonDocument.Parse).RootElement
                    |> Settings.TryFromJson
                | false -> Error $"File `{path}` does not exist"
            with ex ->
                Error $"Unhandled exception while loading settings: {ex.Message}"

        static member TryFromJson(json: JsonElement) =
            match
                Json.tryGetStringProperty "storePath" json,
                Json.tryGetStringProperty "articlesRoot" json,
                Json.tryGetStringProperty "resourcesRoot" json
            with
            | Some sp, Some ar, Some rr ->
                { StorePath = sp
                  ArticlesRoot = ar
                  ResourcesRoot = rr
                  DirectoryIgnorePatterns =
                    Json.tryGetProperty "directoryIgnorePatterns" json
                    |> Option.bind Json.tryGetStringArray
                    |> Option.map (fun dip ->
                        dip
                        |> List.map (fun s -> Regex(s, RegexOptions.Compiled ||| RegexOptions.Singleline)))
                    |> Option.defaultValue []
                  FileIgnorePatterns =
                    Json.tryGetProperty "fileIgnorePatterns" json
                    |> Option.bind Json.tryGetStringArray
                    |> Option.map (fun fip ->
                        fip
                        |> List.map (fun s -> Regex(s, RegexOptions.Compiled ||| RegexOptions.Singleline)))
                    |> Option.defaultValue []
                  DateTimeFormats =
                    Json.tryGetProperty "dateTimeFormats" json
                    |> Option.bind Json.tryGetStringArray
                    |> Option.defaultValue [ "u"; "yyyy-MM-dd" ]
                  IndexFileName = Json.tryGetStringProperty "indexFileName" json |> Option.defaultValue "index.md" }
                |> Ok
            | None, _, _ -> Error "Missing `storePath` property"
            | _, None, _ -> Error "Missing `articlesRoot` property"
            | _, _, None -> Error "Missing `resourcesRoot` property"

    type ImportResult = { Path: string; Result: AddResult }

    type ImportDirectoryResult =
        | Success of ImportDirectorySuccessResult
        | Skipped of Path: string * Reason: string

    and ImportDirectorySuccessResult =
        { Path: string
          IndexResult: AddResult
          Results: ImportResult list
          ChildrenResults: ImportDirectoryResult list }

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
    
