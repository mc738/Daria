namespace Daria.V2.Operations.Import

open Daria.V2.Operations.Common

[<AutoOpen>]
module Impl =

    open System
    open System.IO
    open FDOM.Core.Parsing
    open Freql.Sqlite
    open Daria.V2.DataStore
    open Daria.V2.DataStore.Common
    open Daria.V2.Operations.Import.Articles
    open Daria.V2.Operations.Import.Series

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
                            let fn = Path.GetFileName(fi)

                            fn.Equals(settings.IndexFileName) |> not
                            && settings.FileIgnorePatterns |> List.exists (fun ip -> ip.IsMatch fn) |> not)
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

                    ({ Path = path
                       IndexResult = indexResult
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
        OperationSettings.Load settingsPath
        |> Result.map (fun settings ->
            use ctx =
                match File.Exists settings.Common.StorePath with
                | true -> SqliteContext.Open settings.Common.StorePath
                | false ->
                    use ctx = SqliteContext.Create settings.Common.StorePath
                    Initialization.run ctx
                    ctx
            
            // Resources need to be imported before series/articles to make sure images are added.
            let resources = Resources.importResources ctx settings.Import
            StoreSettings.import ctx settings.Import
            
            { Directories =
                Directory.EnumerateDirectories(settings.Import.ArticlesRoot)
                |> Seq.filter (fun di ->
                    let dn = DirectoryInfo(di).Name
                    settings.Import.DirectoryIgnorePatterns |> List.exists (fun ip -> ip.IsMatch dn) |> not)
                |> List.ofSeq
                |> List.map (scanDirectory ctx settings.Import None)
              Resources = resources })