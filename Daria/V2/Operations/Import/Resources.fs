namespace Daria.V2.Operations.Import

open System.Text.Json
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


    let importImage (item: ImageManifestItem) =
        
        
        ()