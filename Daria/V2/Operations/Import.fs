namespace Daria.V2.Operations

#nowarn "100001"

open System
open System.IO
open System.Text
open Daria.V2.DataStore.Common
open FDOM.Core.Common.Formatting
open FDOM.Core.Parsing.BlockParser
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
        { IgnorePatterns: Regex list
          DateTimeFormats: string list
          IndexFileName: string }

    type ImportResult = { Path: string; Result: AddResult }

    type ImportDirectoryResult =
        | Success of ImportDirectorySuccessResult
        | Skipped of Reason: string

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


    let addSeriesVersion (ctx: SqliteContext) (settings: Settings) (metadata: Map<string, string>) =
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
                 |> Option.defaultValue dirName
               TitleSlug = metadata.TryFind Keys.titleSlug
               Description = rawIndexDescription |> Option.defaultValue ""
               IndexBlob = Blob.Text ifc
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


    let rec scanDirectory (ctx: SqliteContext) (settings: Settings) (parentId: string option) (path: string) =
        // First look for an index file.
        let indexPath = Path.Combine(path, settings.IndexFileName)

        match File.Exists(indexPath) with
        | true ->
            let ifc = File.ReadAllText(indexPath)

            let (imd, indexLines) =
                ifc.Split Environment.NewLine |> List.ofSeq |> Parser.ExtractMetadata

            let dirName = DirectoryInfo(path).Name

            // Handle the metadata.

            let rawIndexTitle, rawIndexDescription = tryGetTitleAndDescription indexLines

            match addSeries ctx settings imd parentId dirName with
            | AddResult.Success seriesId
            | AddResult.AlreadyExists seriesId
            | AddResult.NoChange seriesId ->



                ()
            | AddResult.MissingRelatedEntity _ -> ()
            | AddResult.Failure(message, exceptionOption) -> ()


            let imageVersion =
                imd.TryFind Keys.imageVersionId
                |> Option.map (RelatedEntityVersion.Specified)
                |> Option.orElseWith (fun _ ->
                    imd.TryFind Keys.imageId
                    |> Option.map (fun iid ->
                        match imd.TryFind Keys.imageVersion |> Option.bind tryToInt with
                        | Some v -> EntityVersion.Specific(iid, v)
                        | None -> EntityVersion.Latest iid
                        |> RelatedEntityVersion.Lookup))

            let newVersion =
                ({ Id = IdType.Generated
                   SeriesId = seriesId
                   Title =
                     imd.TryFind Keys.title
                     |> Option.orElse rawIndexTitle
                     |> Option.defaultValue dirName
                   TitleSlug = imd.TryFind Keys.titleSlug
                   Description = rawIndexDescription |> Option.defaultValue ""
                   IndexBlob = Blob.Text ifc
                   ImageVersion = imageVersion
                   CreatedOn = None
                   Tags = imd.TryFind Keys.tags |> Option.map splitValues |> Option.defaultValue []
                   Metadata = imd }
                : Models.NewSeriesVersion)

            let indexResult =
                match imd.TryFind Keys.draft |> Option.bind tryToBool |> Option.defaultValue false with
                | true -> Series.addDraftVersion ctx false newVersion
                | false -> Series.addVersion ctx false newVersion

            let fileResults =
                Directory.EnumerateFiles(path)
                |> Seq.filter (fun fi ->
                    fi.Equals(settings.IndexFileName) |> not
                    && settings.IgnorePatterns |> List.exists (fun ip -> ip.IsMatch fi) |> not)
                |> List.ofSeq
                |> List.map (fun fi ->
                    let afc = File.ReadAllText fi //|> List.ofArray

                    let (amd, articleLines) =
                        Parser.ExtractMetadata(afc.Split Environment.NewLine |> List.ofArray)

                    let rawArticleTitle, rawArticleDescription = tryGetTitleAndDescription articleLines

                    let fileName = Path.GetFileNameWithoutExtension(fi)

                    let articleId =
                        amd.TryFind Keys.articleId
                        |> Option.orElseWith (fun _ -> amd.TryFind Keys.titleSlug)
                        |> Option.orElseWith (fun _ -> amd.TryFind Keys.title |> Option.map slugify)
                        |> Option.defaultValue (slugify dirName)

                    let articleImageVersion =
                        imd.TryFind Keys.imageVersionId
                        |> Option.map (RelatedEntityVersion.Specified)
                        |> Option.orElseWith (fun _ ->
                            imd.TryFind Keys.imageId
                            |> Option.map (fun iid ->
                                match imd.TryFind Keys.imageVersion |> Option.bind tryToInt with
                                | Some v -> EntityVersion.Specific(iid, v)
                                | None -> EntityVersion.Latest iid
                                |> RelatedEntityVersion.Lookup))

                    let articleResult =
                        ({ Id = IdType.Specific articleId
                           Name =
                             amd.TryFind Keys.articleName
                             |> Option.orElseWith (fun _ -> amd.TryFind Keys.title)
                             |> Option.defaultValue dirName
                           SeriesId = seriesId
                           ArticleOrder = amd.TryFind Keys.order |> Option.bind tryToInt |> Option.defaultValue 99999
                           CreatedOn =
                             amd.TryFind Keys.createdOn
                             |> Option.bind (tryToDateTime settings.DateTimeFormats) }
                        : Models.NewArticle)
                        |> Articles.add ctx

                    let newArticleVersion =
                        ({ Id = IdType.Generated
                           ArticleId = articleId
                           Title =
                             amd.TryFind Keys.title
                             |> Option.orElse rawArticleTitle
                             |> Option.defaultValue fileName
                           TitleSlug = amd.TryFind Keys.titleSlug
                           Description = rawArticleDescription |> Option.defaultValue ""
                           ArticleBlob = Blob.Text afc
                           ImageVersion = articleImageVersion
                           RawLink = amd.TryFind Keys.rawLink
                           OverrideCss = amd.TryFind Keys.overrideCss
                           CreatedOn = None
                           PublishedOn =
                             amd.TryFind Keys.publishedOn
                             |> Option.bind (tryToDateTime settings.DateTimeFormats)
                           Tags = amd.TryFind Keys.tags |> Option.map splitValues |> Option.defaultValue []
                           Metadata = amd }
                        : Models.NewArticleVersion)

                    match amd.TryFind Keys.draft |> Option.bind tryToBool |> Option.defaultValue false with
                    | true -> Articles.addDraftVersion ctx false newArticleVersion
                    | false -> Articles.addVersion ctx false newArticleVersion
                    |> fun r -> { Path = fi; Result = r })

            let directoryResults =
                Directory.EnumerateDirectories(path)
                |> List.ofSeq
                |> List.map (scanDirectory ctx settings (Some seriesId))


            ({ IndexResult = indexResult
               Results = fileResults
               ChildrenResults = directoryResults }
            : ImportDirectorySuccessResult)
            |> ImportDirectoryResult.Success
        | false -> ImportDirectoryResult.Skipped $"Missing `{settings.IndexFileName}` file."

    let run _ =

        ()
