namespace Daria.V2.Operations

#nowarn "100001"

open System
open System.IO
open Daria.V2.DataStore.Common
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

    let rec scanDirectory (ctx: SqliteContext) (settings: Settings) (parentId: string option) (path: string) =
        // First look for an index file.
        let indexPath = Path.Combine(path, settings.IndexFileName)

        match File.Exists(indexPath) with
        | true ->
            let ifc = File.ReadAllText(indexPath)
            let (imd, _) = ifc.Split Environment.NewLine |> List.ofSeq |> Parser.ExtractMetadata
            let dirName = DirectoryInfo(path).Name


            // Handle the metadata.

            let seriesId =
                imd.TryFind Keys.seriesId
                |> Option.orElseWith (fun _ -> imd.TryFind Keys.titleSlug)
                |> Option.orElseWith (fun _ -> imd.TryFind Keys.title |> Option.map slugify)
                |> Option.defaultValue (slugify dirName)

            let seriesResult =
                ({ Id = IdType.Specific seriesId
                   Name = failwith "todo"
                   ParentId = parentId
                   SeriesOrder = imd.TryFind Keys.order |> Option.bind tryToInt |> Option.defaultValue 99999
                   CreatedOn =
                     imd.TryFind Keys.createdOn
                     |> Option.bind (tryToDateTime settings.DateTimeFormats) }
                : Models.NewSeries)
                |> Series.add ctx

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

            let indexResult =
                ({ Id = IdType.Generated
                   SeriesId = seriesId
                   Title = imd.TryFind Keys.title |> Option.defaultValue dirName
                   TitleSlug = imd.TryFind Keys.titleSlug
                   Description = failwith "todo"
                   IndexBlob = Blob.Text ifc
                   ImageVersion = imageVersion
                   CreatedOn = None
                   Tags = imd.TryFind Keys.tags |> Option.map splitValues |> Option.defaultValue []
                   Metadata = imd }
                : Models.NewSeriesVersion)
                |> Series.addVersion ctx false

            let fileResults =
                Directory.EnumerateFiles(path)
                |> Seq.filter (fun fi -> settings.IgnorePatterns |> List.exists (fun ip -> ip.IsMatch fi) |> not)
                |> Seq.map (fun fi ->
                    let fileContents = File.ReadAllLines fi |> List.ofArray

                    let (metadata, _) = Parser.ExtractMetadata fileContents



                    ())

            let directoryResults =
                Directory.EnumerateDirectories(path)
                |> List.ofSeq
                |> List.map (scanDirectory ctx settings (Some seriesId))


            ({ IndexResult = indexResult
               Results = fileResults
               ChildrenResults = directoryResults }: ImportDirectorySuccessResult)
            |> ImportDirectoryResult.Success
        | false -> ImportDirectoryResult.Skipped $"Missing `{settings.IndexFileName}` file."


    let run _ =

        ()
