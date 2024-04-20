namespace Daria.V2.Operations.Common

open System.IO
open System.Text.Json
open System.Text.RegularExpressions
open FsToolbox.Core

[<AutoOpen>]
module Settings =

    type OperationSettings =
        { Common: CommonSettings
          Import: ImportSettings }

        static member Load(path: string) =
            try
                match File.Exists path with
                | true ->
                    (File.ReadAllText path |> JsonDocument.Parse).RootElement
                    |> OperationSettings.TryFromJson
                | false -> Error $"File `{path}` does not exist"
            with ex ->
                Error $"Unhandled exception while loading settings: {ex.Message}"

        static member TryFromJson(json: JsonElement) =
            match
                Json.tryGetProperty "common" json
                |> Option.map CommonSettings.TryFromJson
                |> Option.defaultValue (Error "Missing `common` property"),
                Json.tryGetProperty "import" json
                |> Option.map ImportSettings.TryFromJson
                |> Option.defaultValue (Error "Missing `import` property")
            with
            | Ok cs, Ok imp -> { Common = cs; Import = imp } |> Ok
            | Error e, _
            | _, Error e -> Error e

    and CommonSettings =
        { StorePath: string }

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "storePath" json with
            | Some sp -> { StorePath = sp } |> Ok
            | None -> Error "Missing `storePath` property"


    and ImportSettings =
        { ArticlesRoot: string
          ResourcesRoot: string
          DirectoryIgnorePatterns: Regex list
          FileIgnorePatterns: Regex list
          DateTimeFormats: string list
          IndexFileName: string
          StoreSettings: StoreSetting list }

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "articlesRoot" json, Json.tryGetStringProperty "resourcesRoot" json with
            | Some ar, Some rr ->
                { ArticlesRoot = ar
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
                  IndexFileName = Json.tryGetStringProperty "indexFileName" json |> Option.defaultValue "index.md"
                  StoreSettings = [] }
                |> Ok
            | None, _ -> Error "Missing `articlesRoot` property"
            | _, None -> Error "Missing `resourcesRoot` property"

    and StoreSetting =
        { Key: string
          Value: string }

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "key" json, Json.tryGetStringProperty "value" json with
            | Some k, Some v -> Ok { Key = k; Value = v }
            | None, _ -> Error "Missing `key` property"
            | _, None -> Error "Missing `value` property"
