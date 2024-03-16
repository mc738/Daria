namespace Daria.V2.Operations

#nowarn "100001"

open System
open System.IO
open System.Text
open System.Text.Json
open Daria.V2.DataStore.Common
open FDOM.Core.Common.Formatting
open FDOM.Core.Parsing.BlockParser
open FsToolbox.Core
open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module Import =

    open System.IO
    open System.Text.RegularExpressions
    open Daria.V2.Common.Metadata
    open Daria.V2.DataStore
    open Daria.V2.DataStore.Common
    open FDOM.Core
    open FDOM.Core.Parsing
    open Freql.Sqlite

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
        { IndexResult: AddResult
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

    let addSeries
        (ctx: SqliteContext)
        (settings: Settings)
        (metadata: Map<string, string>)
        (parentId: string option)
        (directoryName: string)
        =
        let seriesId =
            metadata.TryFind Keys.seriesId
            |> Option.orElseWith (fun _ -> metadata.TryFind Keys.titleSlug)
            |> Option.orElseWith (fun _ -> metadata.TryFind Keys.title |> Option.map slugify)
            |> Option.defaultValue (slugify directoryName)

        ({ Id = IdType.Specific seriesId
           Name =
             metadata.TryFind Keys.seriesName
             |> Option.orElseWith (fun _ -> metadata.TryFind Keys.title)
             |> Option.defaultValue directoryName
           ParentId = parentId
           SeriesOrder = metadata.TryFind Keys.order |> Option.bind tryToInt |> Option.defaultValue 99999
           CreatedOn =
             metadata.TryFind Keys.createdOn
             |> Option.bind (tryToDateTime settings.DateTimeFormats) }
        : Models.NewSeries)
        |> Series.add ctx

    let addSeriesVersion
        (ctx: SqliteContext)
        (settings: Settings)
        (metadata: Map<string, string>)
        (seriesId: string)
        (directoryName: string)
        (rawText: string)
        (lines: string list)
        =
        let rawIndexTitle, rawIndexDescription = tryGetTitleAndDescription lines

        let imageVersion =
            metadata.TryFind Keys.imageVersionId
            |> Option.map (RelatedEntityVersion.Specified)
            |> Option.orElseWith (fun _ ->
                metadata.TryFind Keys.imageId
                |> Option.map (fun iid ->
                    match metadata.TryFind Keys.imageVersion |> Option.bind tryToInt with
                    | Some v -> EntityVersion.Specific(iid, v)
                    | None -> EntityVersion.Latest iid
                    |> RelatedEntityVersion.Lookup))

        let newVersion =
            ({ Id = IdType.Generated
               SeriesId = seriesId
               Title =
                 metadata.TryFind Keys.title
                 |> Option.orElse rawIndexTitle
                 |> Option.defaultValue directoryName
               TitleSlug = metadata.TryFind Keys.titleSlug
               Description = rawIndexDescription |> Option.defaultValue ""
               IndexBlob = Blob.Text rawText
               ImageVersion = imageVersion
               CreatedOn = None
               Tags = metadata.TryFind Keys.tags |> Option.map splitValues |> Option.defaultValue []
               Metadata = metadata }
            : Models.NewSeriesVersion)

        match
            metadata.TryFind Keys.draft
            |> Option.bind tryToBool
            |> Option.defaultValue false
        with
        | true -> Series.addDraftVersion ctx false newVersion
        | false -> Series.addVersion ctx false newVersion

    let addArticle
        (ctx: SqliteContext)
        (settings: Settings)
        (metadata: Map<string, string>)
        (seriesId: string)
        (fileName: string)
        =

        let articleId =
            metadata.TryFind Keys.articleId
            |> Option.orElseWith (fun _ -> metadata.TryFind Keys.titleSlug)
            |> Option.orElseWith (fun _ -> metadata.TryFind Keys.title |> Option.map slugify)
            |> Option.defaultValue (slugify fileName)

        ({ Id = IdType.Specific articleId
           Name =
             metadata.TryFind Keys.articleName
             |> Option.orElseWith (fun _ -> metadata.TryFind Keys.title)
             |> Option.defaultValue fileName
           SeriesId = seriesId
           ArticleOrder = metadata.TryFind Keys.order |> Option.bind tryToInt |> Option.defaultValue 99999
           CreatedOn =
             metadata.TryFind Keys.createdOn
             |> Option.bind (tryToDateTime settings.DateTimeFormats) }
        : Models.NewArticle)
        |> Articles.add ctx

    let addArticleVersion
        (ctx: SqliteContext)
        (settings: Settings)
        (metadata: Map<string, string>)
        (articleId: string)
        (fileName: string)
        (filePath: string)
        (rawText: string)
        (lines: string list)
        =
        let rawArticleTitle, rawArticleDescription = tryGetTitleAndDescription lines

        let articleImageVersion =
            metadata.TryFind Keys.imageVersionId
            |> Option.map (RelatedEntityVersion.Specified)
            |> Option.orElseWith (fun _ ->
                metadata.TryFind Keys.imageId
                |> Option.map (fun iid ->
                    match metadata.TryFind Keys.imageVersion |> Option.bind tryToInt with
                    | Some v -> EntityVersion.Specific(iid, v)
                    | None -> EntityVersion.Latest iid
                    |> RelatedEntityVersion.Lookup))

        let newArticleVersion =
            ({ Id = IdType.Generated
               ArticleId = articleId
               Title =
                 metadata.TryFind Keys.title
                 |> Option.orElse rawArticleTitle
                 |> Option.defaultValue fileName
               TitleSlug = metadata.TryFind Keys.titleSlug
               Description = rawArticleDescription |> Option.defaultValue ""
               ArticleBlob = Blob.Text rawText
               ImageVersion = articleImageVersion
               RawLink = metadata.TryFind Keys.rawLink
               OverrideCss = metadata.TryFind Keys.overrideCss
               CreatedOn = None
               PublishedOn =
                 metadata.TryFind Keys.publishedOn
                 |> Option.bind (tryToDateTime settings.DateTimeFormats)
               Tags = metadata.TryFind Keys.tags |> Option.map splitValues |> Option.defaultValue []
               Metadata = metadata }
            : Models.NewArticleVersion)

        match
            metadata.TryFind Keys.draft
            |> Option.bind tryToBool
            |> Option.defaultValue false
        with
        | true -> Articles.addDraftVersion ctx false newArticleVersion
        | false -> Articles.addVersion ctx false newArticleVersion
        |> fun r -> { Path = filePath; Result = r }

    let rec scanDirectory (ctx: SqliteContext) (settings: Settings) (parentId: string option) (path: string) =
        // First look for an index file.
        let indexPath = Path.Combine(path, settings.IndexFileName)

        match File.Exists(indexPath) with
        | true ->
            let ifc = File.ReadAllText(indexPath)

            let (imd, indexLines) =
                ifc.Split Environment.NewLine |> List.ofSeq |> Parser.ExtractMetadata

            let dirName = DirectoryInfo(path).Name

            let indexResult = addSeries ctx settings imd parentId dirName

            match indexResult with
            | AddResult.Success seriesId
            | AddResult.AlreadyExists seriesId
            | AddResult.NoChange seriesId ->

                let indexVersionResult =
                    addSeriesVersion ctx settings imd seriesId dirName ifc indexLines

                match indexVersionResult with
                | AddResult.Success _
                | AddResult.NoChange _
                | AddResult.AlreadyExists _ ->

                    let fileResults =
                        Directory.EnumerateFiles(path)
                        |> Seq.filter (fun fi ->
                            fi.Equals(settings.IndexFileName) |> not
                            && settings.FileIgnorePatterns |> List.exists (fun ip -> ip.IsMatch fi) |> not)
                        |> List.ofSeq
                        |> List.map (fun fi ->

                            let afc = File.ReadAllText fi //|> List.ofArray

                            let (amd, articleLines) =
                                Parser.ExtractMetadata(afc.Split Environment.NewLine |> List.ofArray)

                            let fileName = Path.GetFileNameWithoutExtension(fi)

                            let articleResult = addArticle ctx settings amd seriesId fileName

                            match articleResult with
                            | AddResult.Success articleId
                            | AddResult.NoChange articleId
                            | AddResult.AlreadyExists articleId ->
                                addArticleVersion ctx settings amd articleId fileName fi afc articleLines
                            | AddResult.MissingRelatedEntity _
                            | AddResult.Failure _ -> { Path = fi; Result = articleResult })

                    let directoryResults =
                        Directory.EnumerateDirectories(path)
                        |> Seq.filter (fun di ->
                            let dn = DirectoryInfo(di).Name
                            settings.DirectoryIgnorePatterns |> List.exists (fun ip -> ip.IsMatch dn) |> not)
                        |> List.ofSeq
                        |> List.map (scanDirectory ctx settings (Some seriesId))

                    ({ IndexResult = indexResult
                       Results = fileResults
                       ChildrenResults = directoryResults }
                    : ImportDirectorySuccessResult)
                    |> ImportDirectoryResult.Success
                | AddResult.MissingRelatedEntity(entityType, id) ->
                    ImportDirectoryResult.Skipped(
                        path,
                        $"A related entity was missing while adding the series version. Id: {id} Type: {entityType}"
                    )
                | AddResult.Failure(message, ``exception``) ->
                    ImportDirectoryResult.Skipped(path, $"Failure while adding the series version. Message: {message}")
            | AddResult.MissingRelatedEntity(entityType, missingId) ->
                ImportDirectoryResult.Skipped(
                    path,
                    $"A related entity was missing while adding the series. Id: {missingId} Type: {entityType}"
                )
            | AddResult.Failure(message, exceptionOption) ->
                ImportDirectoryResult.Skipped(path, $"Failure while adding the series version. Message: {message}")
        | false -> ImportDirectoryResult.Skipped("", $"Missing `{settings.IndexFileName}` file.")

    let run (settingsPath: string) =
        Settings.Load settingsPath
        |> Result.map (fun settings ->
            use ctx =
                match File.Exists settings.StorePath with
                | true -> SqliteContext.Open settings.StorePath
                | false ->
                    use ctx = SqliteContext.Create settings.StorePath
                    Initialization.run ctx
                    ctx

            scanDirectory ctx settings None settings.ArticlesRoot)
