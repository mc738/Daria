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
          Import: ImportSettings
          Build: BuildSettings }

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
                |> Option.defaultValue (Error "Missing `import` property"),
                Json.tryGetProperty "build" json
                |> Option.map BuildSettings.TryFromJson
                |> Option.defaultValue (Error "Missing `build` property")
            with
            | Ok cs, Ok imp, Ok bs ->
                { Common = cs
                  Import = imp
                  Build = bs }
                |> Ok
            | Error e, _, _
            | _, Error e, _
            | _, _, Error e -> Error e

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

    and BuildSettings =
        { Profiles: BuildProfileSettings list }

        static member TryFromJson(json: JsonElement) =
            match
                Json.tryGetArrayProperty "profiles" json
                |> Option.map (List.map BuildProfileSettings.TryFromJson >> resultCollect)
                |> Option.defaultValue (Ok [])
            with
            | Ok p -> { Profiles = p } |> Ok
            | Error e -> Error e

    and BuildProfileSettings =
        { Name: string
          RootPath: string
          ClearDirectoryBeforeBuild: bool
          ArticlesTemplateSource: BuildTemplateSource
          SeriesTemplateSource: BuildTemplateSource
          IndexTemplateSource: BuildTemplateSource
          PreBuildSteps: BuildStep list
          PostBuildSteps: BuildStep list }

        static member TryFromJson(json: JsonElement) =
            match
                Json.tryGetStringProperty "name" json,
                Json.tryGetStringProperty "rootPath" json,
                Json.tryGetProperty "articlesTemplateSource" json
                |> Option.map BuildTemplateSource.TryFromJson
                |> Option.defaultValue (Error "Missing `articlesTemplateSource` property"),
                Json.tryGetProperty "seriesTemplateSource" json
                |> Option.map BuildTemplateSource.TryFromJson
                |> Option.defaultValue (Error "Missing `seriesTemplateSource` property"),
                Json.tryGetProperty "indexTemplateSource" json
                |> Option.map BuildTemplateSource.TryFromJson
                |> Option.defaultValue (Error "Missing `indexTemplateSource` property"),
                Json.tryGetArrayProperty "preBuildSteps" json
                |> Option.map (List.map BuildStep.TryFromJson >> resultCollect)
                |> Option.defaultValue (Ok []),
                Json.tryGetArrayProperty "postBuildSteps" json
                |> Option.map (List.map BuildStep.TryFromJson >> resultCollect)
                |> Option.defaultValue (Ok [])
            with
            | Some n, Some rp, Ok ats, Ok sts, Ok its, Ok pre, Ok pos ->
                { Name = n
                  RootPath = rp
                  ClearDirectoryBeforeBuild = failwith "todo"
                  ArticlesTemplateSource = ats
                  SeriesTemplateSource = sts
                  IndexTemplateSource = its
                  PreBuildSteps = pre
                  PostBuildSteps = pos }
                |> Ok
            | None, _, _, _, _, _, _ -> Error "Missing `name` property"
            | _, None, _, _, _, _, _ -> Error "Missing `rootPath` property"
            | _, _, Error e, _, _, _, _ -> Error e
            | _, _, _, Error e, _, _, _ -> Error e
            | _, _, _, _, Error e, _, _ -> Error e
            | _, _, _, _, _, Error e, _ -> Error e
            | _, _, _, _, _, _, Error e -> Error e

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

    and [<RequireQualifiedAccess>] BuildStep =
        | CreateDirectory of CreateDirectoryStep
        | CopyFile of CopyFileStep
        | ExportImages of ExportImagesStep
        | ExportResourceBucket of ExportResourceBucketStep
        | CreateArtifact
        | UploadArtifact

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "type" json with
            | Some "create-directory" -> CreateDirectoryStep.TryFromJson json |> Result.map BuildStep.CreateDirectory
            | Some "copy-file" -> CopyFileStep.TryFromJson json |> Result.map BuildStep.CopyFile
            | Some "export-images" -> ExportImagesStep.TryFromJson json |> Result.map BuildStep.ExportImages
            | Some "export-resource-bucket" ->
                ExportResourceBucketStep.TryFromJson json
                |> Result.map BuildStep.ExportResourceBucket
            | Some "create-artifact" -> Ok BuildStep.CreateArtifact
            | Some "upload-artifact" -> Ok BuildStep.UploadArtifact
            | Some t -> Error $"Unknown build step type `{t}`"
            | None -> Error "Missing `type` property"

    and CreateDirectoryStep =
        { Name: string }

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "name" json with
            | Some n -> { Name = n } |> Ok
            | None -> Error "Missing `name` property"

    and CopyFileStep =
        { Path: string
          OutputDirectoryName: string
          SkipIfExists: bool }

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "path" json, Json.tryGetStringProperty "outputDirectoryName" json with
            | Some p, Some odn ->
                { Path = p
                  OutputDirectoryName = odn
                  SkipIfExists = Json.tryGetBoolProperty "skipIfExists" json |> Option.defaultValue true }
                |> Ok
            | None, _ -> Error "Missing `path` property"
            | _, None -> Error "Missing `outputDirectoryName`"

    and ExportImagesStep =
        { OutputDirectoryName: string
          SkipIfExists: bool }

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "outputDirectoryName" json with
            | Some odn ->
                { OutputDirectoryName = odn
                  SkipIfExists = Json.tryGetBoolProperty "skipIfExists" json |> Option.defaultValue true }
                |> Ok
            | None -> Error "Missing `outputDirectoryName` property"

    and ExportResourceBucketStep =
        { BucketName: string
          OutputDirectoryName: string
          UseLatestResourceVersion: bool
          SkipIfExists: bool }

        static member TryFromJson(json: JsonElement) =
            match Json.tryGetStringProperty "bucketName" json, Json.tryGetStringProperty "outputDirectoryName" json with
            | Some bn, Some odn ->
                { BucketName = bn
                  OutputDirectoryName = odn
                  UseLatestResourceVersion =
                    Json.tryGetBoolProperty "useLatestResourceVersion" json
                    |> Option.defaultValue true
                  SkipIfExists = Json.tryGetBoolProperty "skipIfExists" json |> Option.defaultValue true }
                |> Ok
            | None, _ -> Error "Missing `bucketName` property"
            | _, None -> Error "Missing `outputDirectoryName` property"
