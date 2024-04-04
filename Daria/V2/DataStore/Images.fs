namespace Daria.V2.DataStore

open Daria.V2.DataStore.Models
open Daria.V2.DataStore.Persistence
open Freql.Sqlite

module Images =


    module Internal =


        let fetchSpecificVersion (ctx: SqliteContext) (imageId: string) (version: int) =
            Operations.selectImageVersionRecord ctx [ "WHERE image_id = @0 AND version = @1" ] [ imageId; version ]

        let fetchLatestVersion (ctx: SqliteContext) (imageId: string) =
            Operations.selectImageVersionRecord
                ctx
                [ "WHERE image_id = @0"; "ORDER BY version DESC"; "LIMIT 1" ]
                [ imageId ]



    let getLatestVersion (ctx: SqliteContext) (imageId: string) =



        ()


    let addVersion (ctx: SqliteContext) (newVersion: NewImageVersion) =
        
        let id = newVersion.Id.ToString()

        // First check if the version exists (or what the latest version is)
        
        match Internal.fetchLatestVersion ctx newVersion.ImageId with
        | Some lv ->
            // Check if either the main or preview blob has changed and
            let prevResourceHash = Resources.getVersionHash ctx lv.ResourceVersionId
            let prevPreviewResourceHash = lv.PreviewResourceVersionId |> Option.bind (Resources.getVersionHash ctx) 
            
            newVersion.ResourceVersion.ResourceBlob.TryGetHash()
            newVersion.PreviewResourceVersion |> Option.map (fun prv -> prv.ResourceBlob.TryGetHash())
            
            ()
        | None -> ()
        
        
        ()