namespace Daria.V1

open System
open System.Text
open Daria
open Freql.Core.Common.Types
open Freql.Sqlite

module DataStore =

    open Daria.V1.Persistence

    let getContent (blob: BlobField) =
        blob.ToBytes()
        |> Encoding.UTF8.GetString
        |> fun s -> s.Split Environment.NewLine |> List.ofArray
        |> ArticleContent.Create

    let getLatestArticleVersion (ctx: SqliteContext) (name: string) =

        let article =
            Operations.selectArticleRecord ctx [ "WHERE name = @0" ] [ name ]

        let version =
            Operations.selectArticleVersionRecord ctx [ "WHERE article = @0 ORDER BY version_number DESC LIMIT 1" ] [ name ]

        match article, version with
        | Some a, Some v ->
            let content = getContent v.RawBlob
            // Share links
            let shareLinks =
                Operations.selectArticleShareLinkRecords ctx [ "WHERE article = @0" ] [ a.Name ]
                |> List.map (fun sl ->
                    let slt =
                        ShareLinkType.Deserialize sl.LinkType
                        |> Option.defaultWith (fun _ -> failwith "Unknowing share link type")

                    ({ Icon = slt.GetIcon()
                       Url = slt.GenerateUrl(a.Url, content.TitleText, content.DescriptionText)
                       Title = slt.GetTitle() }: ShareLink))
                |> List.rev

            let links =
                Operations.selectArticleLinkRecords ctx [ "WHERE article = @0" ] [ a.Name ]
                |> List.map (fun l ->
                    ({ Title = l.Title
                       Description = l.Description
                       Url = l.Url }: Link))
                |> List.rev


            let tags =
                Operations.selectArticleTagRecords ctx [ "WHERE article = @0" ] [ a.Name ]
                |> List.map (fun at -> at.Tag)
                |> List.rev


            let parts =
                Operations.selectArticleRecords ctx [ "WHERE series = @0" ] [ a.Series ]
                |> List.sortBy (fun p -> p.PartNumber)
                |> List.map (fun p ->
                    ({ Title = p.Title
                       PartNumber = p.PartNumber
                       Url = p.Url }: Part))

            ({ Content = content
               Version = v.VersionNumber
               Image = a.ImagePath
               ImagePreview = a.PreviewImageUrl
               PublishDate = v.PublishDate
               Url = a.Url
               Parts = parts
               NextPart =
                 parts
                 |> List.tryFind (fun p -> p.PartNumber = a.PartNumber + 1)
               PreviousPart =
                 parts
                 |> List.tryFind (fun p -> p.PartNumber = a.PartNumber - 1)
               ShareLinks = shareLinks
               Links = links
               Tags = tags
               Thanks = a.Thanks |> Option.defaultValue ""
               RawLink = a.RawLink |> Option.defaultValue ""
               OverrideCssUrl = a.OverrideCssUrl }: Article)
            |> Some
        | _ -> None
        
        
    let getOrderedSeries (ctx: SqliteContext) =
        Operations.selectSeriesRecords ctx [ "ORDER BY series_number" ] []
        
        
    let getFirstArticle (ctx: SqliteContext) (series: string) =
        Operations.selectArticleRecord ctx [ "WHERE series = @0"; "ORDER by part_number LIMIT 1" ] [ series ]
        
    let getSeriesTags (ctx: SqliteContext) (series: string) =
        Operations.selectSeriesTagsRecords ctx [ "WHERE series = @0" ] [ series ]
        
        
    let getLatestPosts (ctx: SqliteContext) (count: int) =
        Operations.selectArticleVersionRecords ctx [ "ORDER BY DATE(publish_date) DESC LIMIT @0" ] [ count ]
        |> List.map (fun av -> Operations.selectArticleRecord ctx [ "WHERE name = @0" ] [ av.Article ] |> Option.map (fun a -> a, av))
        |> List.choose id