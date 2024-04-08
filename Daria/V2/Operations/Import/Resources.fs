namespace Daria.V2.Operations.Import

open System
open System.IO
open System.Text.Json
open Daria.V2.Common.Domain
open Daria.V2.DataStore
open Daria.V2.DataStore.Common
open Daria.V2.DataStore.Models
open Freql.Sqlite
open FsToolbox.Core

module Resources =


    type ImageManifestItem =
        { Directory: string
          ImageName: string
          PreviewImageName: string option
          ThanksName: string option
          PreviewUrl: string option }


        static member TryDeserialize(json: JsonElement) =
            match Json.tryGetStringProperty "directory" json, Json.tryGetStringProperty "imageName" json with
            | Some d, Some i ->
                ({ Directory = d
                   ImageName = i
                   PreviewImageName = Json.tryGetStringProperty "previewImageName" json
                   ThanksName = Json.tryGetStringProperty "thanksName" json
                   PreviewUrl = Json.tryGetStringProperty "previewUrl" json }
                : ImageManifestItem)
                |> Ok
            | None, _ -> Error "Missing `directory` property"
            | _, None -> Error "Missing `imageName` property"

    let tryCreateResourceVersion (path: string) =
        match File.Exists path with
        | true ->
            let extension = Path.GetExtension(path)
            let name = Path.GetFileNameWithoutExtension(path)

            { Id = IdType.Generated
              ResourceId = name
              ResourceBlob = File.ReadAllBytes path |> Blob.Bytes
              CreatedOn = Some DateTime.UtcNow
              FileType = FileType.FromExtension extension
              EncryptionType = EncryptionType.None
              CompressionType = CompressionType.None }
            |> Some

        | false -> None

    let importImage (ctx: SqliteContext) (rootPath: string) (item: ImageManifestItem) =

        let dirPath = Path.Combine(rootPath, item.Directory)


        let imagePath = Path.Combine(dirPath, item.ImageName)

        let extension = Path.GetExtension(imagePath)
        let name = Path.GetFileNameWithoutExtension(imagePath)


        match tryCreateResourceVersion imagePath with
        | Some nrv ->
            let previewImagePath = 
            
            let result =
                ({ Id = IdType.Generated
                   ImageId = name
                   ResourceVersion =
                     nrv
                   PreviewResourceVersion =
                       item.PreviewImageName
                       
                       
                   Url = ""
                   PreviewUrl = item.PreviewUrl
                   ThanksHtml =
                     item.ThanksName
                     |> Option.bind (fun tn ->
                         let tnp = Path.Combine(dirPath, tn)

                         match File.Exists tnp with
                         | true -> File.ReadAllText tnp |> Some
                         | false -> None) }
                : NewImageVersion)
                |> Images.addVersion ctx

            ({ Path = imagePath; Result = result }: ImportResult)
        | None ->
            ({ Path = imagePath
               Result = AddResult.Failure($"File `{imagePath}` not found", None) }
            : ImportResult)
