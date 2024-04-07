namespace Daria.V2.Operations.Import

open System.Text.Json
open FsToolbox.Core

module Resources =


    type ImageManifestItem =
        { Directory: string
          ImageName: string
          PreviewName: string option
          ThanksName: string option
          PreviewUrl: string option }


        static member TryDeserialize(json: JsonElement) =
            match
                Json.tryGetStringProperty "directory" json,
                Json.tryGetStringProperty "imageName" json
            with
            | Some 
                


            ()
