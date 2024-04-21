namespace Daria.V2.Operations.Import

#nowarn "100001"

open Daria.V2.Operations.Common

module Series =
    
    open Freql.Sqlite
    open Daria.V2.Common.Metadata
    open Daria.V2.DataStore
    open Daria.V2.DataStore.Common
        
    let addSeries
        (ctx: SqliteContext)
        (settings: ImportSettings)
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
        (settings: ImportSettings)
        (metadata: Map<string, string>)
        (seriesId: string)
        (directoryName: string)
        (rawText: string)
        (lines: string list)
        =
        let rawIndexTitle, rawIndexDescription = tryGetTitleAndDescription lines

        let imageVersion =
            metadata.TryFind Keys.imageVersionId
            |> Option.map RelatedEntityVersion.Specified
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
    