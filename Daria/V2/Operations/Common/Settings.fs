namespace Daria.V2.Operations.Common

open Daria.V2.DataStore.Common


[<AutoOpen>]
module Settings =

    open System.IO
    open System.Text.Json
    open System.Text.RegularExpressions
    open FsToolbox.Core
    open Daria.V2.Common

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
                  StoreSettings =
                    Json.tryGetArrayProperty "storeSettings" json
                    |> Option.defaultValue []
                    |> List.map StoreSetting.TryFromJson
                    |> resultChoose }
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

    and BuildSettings = { Profiles: BuildProfileSettings list }

    and BuildProfileSettings =
        { Name: string
          RootPath: string
          ClearDirectoryBeforeBuild: bool
          ArticlesTemplateSource: BuildTemplateSource
          SeriesTemplateSource: BuildTemplateSource
          IndexTemplateSource: BuildTemplateSource
          PreBuildSteps: BuildStep list
          PostBuildSteps: BuildStep list }

    and BuildTemplate = { Name: string }

    and [<RequireQualifiedAccess>] BuildTemplateSource =
        | Store of Id: string * Version: ItemVersion
        | File of Path: string

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "type" json with
            | Some "store" ->
                match Json.tryGetStringProperty "id" json with
                | Some id ->
                    let version =
                        match Json.tryGetStringProperty "versionType" json with
                        | Some "specific" ->
                            match Json.tryGetIntProperty "version" json with
                            | Some v -> ItemVersion.Specific v
                            | None -> ItemVersion.Latest
                        | Some "latest"
                        | Some _
                        | None -> ItemVersion.Latest

                    BuildTemplateSource.Store(id, version) |> Ok
                | None -> Error "Missing `id` property"
            | Some "file" ->
                match Json.tryGetStringProperty "path" json with
                | Some p -> BuildTemplateSource.File p |> Ok
                | None -> Error "Missing `path` property"
            | Some t -> Error $"Unknown template source type `{json}`"
            | None -> Error $"Missing `type` property"

    and [<RequireQualifiedAccess>] ItemVersion =
        | Latest
        | Specific of Version: int

    and BuildStep =
        | CreateDirectory of CreateDirectoryStep
        | CopyFile of CopyFilePath
        | ExportImages of ExportImagesStep
        | ExportResourceBucket of ExportResourceBucketItem
        | CreateArtifact
        | UploadArtifact

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "type" json with
            | Some "create-directory" -> Ok()
            | Some "copy-file" -> Ok()
            | Some "export-images" -> Ok()
            | Some "export-resource-bucket" -> Ok()
            | Some "create-artifact" -> Ok()
            | Some "upload-artifact" -> Ok()
            | Some t -> Error $"Unknown build step type `{t}`"
            | None -> Error "Missing `type` property"

    and CreateDirectoryStep =
        { Name: string }
        
        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "name" json with
            | Some n -> { Name = n } |> Ok
            | None -> Error "Missing `name` property"

    and CopyFilePath =
        { Path: string
          OutputDirectoryName: string }
        
        static member TryFromJson(json: JsonElement) =
            match
                Json.tryGetStringProperty "path" json,
                Json.tryGetStringProperty "outputDirectoryName" json
            with
            | Some p, Some odn -> { Name = n } |> Ok
            | None, _ -> Error "Missing `path` property"
            | _, None -> Error "Missing `out`"

    and ExportImagesStep =
        { OutputDirectoryName: string
          SkipIfExists: bool }

    and ExportResourceBucketItem =
        { BucketName: string
          OutputDirectoryName: string
          UseLatestResourceVersion: bool
          SkipIfExists: bool }
