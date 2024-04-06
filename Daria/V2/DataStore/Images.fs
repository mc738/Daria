namespace Daria.V2.DataStore

open Daria.V2.DataStore.Common
open Daria.V2.DataStore.Models

module Images =

    open Daria.V2.DataStore.Models
    open Daria.V2.DataStore.Persistence
    open Freql.Sqlite
    open Daria.V2.DataStore.Common
    open FsToolbox.Extensions.Strings

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


    let versionExists (ctx: SqliteContext) (versionId: string) =
        Operations.selectImageVersionRecord ctx [ "WHERE id = @0" ] [ versionId ]
        |> Option.isSome

    let exists (ctx: SqliteContext) (id: string) =
        Operations.selectImageRecord ctx [ "WHERE id = @0" ] [ id ] |> Option.isSome

    let add (ctx: SqliteContext) (newImage: NewImage) =
        let id = newImage.Id.ToString()

        match exists ctx id |> not with
        | true ->
            ({ Id = id; Name = newImage.Name }: Parameters.NewImage)
            |> Operations.insertImage ctx

            AddResult.Success id
        | false -> AddResult.AlreadyExists id

    
    let addVersion (ctx: SqliteContext) (newVersion: NewImageVersion) =

        let id = newVersion.Id.ToString()

        // First check if the version exists (or what the latest version is)

        match versionExists ctx id, Internal.fetchLatestVersion ctx newVersion.ImageId with
        | true, _ -> AddResult.AlreadyExists id
        | false, Some lv ->
            // Check if either the main or preview blob has changed and
            let prevResourceHash = Resources.getVersionHash ctx lv.ResourceVersionId

            let prevPreviewResourceHash =
                lv.PreviewResourceVersionId |> Option.bind (Resources.getVersionHash ctx)

            // Try and get the resource version hash.
            // If the blob is Blob.Stream and stream is not seekable this will return None.
            // Which means a new version will always be created.
            let rvh =
                match newVersion.ResourceVersion.ResourceBlob.TryGetHash() with
                | Ok h -> Some h
                | Error _ -> None

            // Try and get the resource preview version hash.
            // If the blob is Blob.Stream and stream is not seekable this will return None.
            // Which means a new version will always be created.
            let pvh =
                newVersion.PreviewResourceVersion
                |> Option.bind (fun prv ->
                    match prv.ResourceBlob.TryGetHash() with
                    | Ok h -> Some h
                    | Error _ -> None)

            // Check if there has been any changes.
            // This will mean individual changes might need to be checked again.
            // However this is ok because it makes things a bit simpler.
            match
                [ prevResourceHash, rvh
                  prevPreviewResourceHash, pvh
                  Some lv.Url, Some newVersion.Url
                  lv.PreviewUrl, newVersion.PreviewUrl
                  lv.ThanksHtml, newVersion.ThanksHtml ]
                |> ``optional strings have changed``
            with
            | true ->
                // Add (if needed) a new version of the resource blob.
                let resourceVersionId =
                    match ``optional string has changed`` prevResourceHash rvh with
                    | true ->
                        match Resources.exists ctx newVersion.ResourceVersion.ResourceId with
                        | true -> ()
                        | false ->
                            match
                                Resources.add
                                    ctx
                                    { Id = IdType.Specific newVersion.ResourceVersion.ResourceId
                                      Name = newVersion.ResourceVersion.ResourceId }
                            with
                            | AddResult.Success id -> ()
                            | AddResult.NoChange id -> ()
                            | AddResult.AlreadyExists id -> ()
                            | AddResult.MissingRelatedEntity(entityType, id) ->
                                // TODO handle
                                failwith "todo"
                            | AddResult.Failure(message, ``exception``) ->
                                // TODO handle
                                failwith "todo"


                        match newVersion.ResourceVersion |> Resources.addVersion ctx false with
                        | AddResult.Success id -> id
                        | AddResult.NoChange id -> id
                        | AddResult.AlreadyExists id -> id
                        | AddResult.MissingRelatedEntity(entityType, id) ->
                            // TODO handle
                            failwith "todo"
                        | AddResult.Failure(message, ``exception``) ->
                            // TODO handle
                            failwith "todo"
                    | false -> lv.ResourceVersionId

                let previewResourceVersionId =
                    match ``optional string has changed`` prevResourceHash rvh with
                    | true ->
                        newVersion.PreviewResourceVersion
                        |> Option.map (fun nv ->
                            match
                                Resources.add
                                    ctx
                                    { Id = IdType.Specific nv.ResourceId
                                      Name = nv.ResourceId }
                            with
                            | AddResult.Success id -> ()
                            | AddResult.NoChange id -> ()
                            | AddResult.AlreadyExists id -> ()
                            | AddResult.MissingRelatedEntity(entityType, id) ->
                                // TODO handle
                                failwith "todo"
                            | AddResult.Failure(message, ``exception``) ->
                                // TODO handle
                                failwith "todo"

                            match Resources.addVersion ctx false nv with
                            | AddResult.Success id -> id
                            | AddResult.NoChange id -> id
                            | AddResult.AlreadyExists id -> id
                            | AddResult.MissingRelatedEntity(entityType, id) ->
                                // TODO handle
                                failwith "todo"
                            | AddResult.Failure(message, ``exception``) ->
                                // TODO handle
                                failwith "todo")
                    | false -> lv.PreviewResourceVersionId

                ({ Id = id
                   ImageId = newVersion.ImageId
                   Version = lv.Version + 1
                   ResourceVersionId = resourceVersionId
                   PreviewResourceVersionId = previewResourceVersionId
                   Url = newVersion.Url
                   PreviewUrl = newVersion.PreviewUrl
                   ThanksHtml = newVersion.ThanksHtml }
                : Parameters.NewImageVersion)
                |> Operations.insertImageVersion ctx

                AddResult.Success id
            | false -> AddResult.NoChange lv.Id
        | false, None ->
            
            match
                Resources.add
                    ctx
                    { Id = IdType.Specific newVersion.ResourceVersion.ResourceId
                      Name = newVersion.ResourceVersion.ResourceId }
            with
            | AddResult.Success id -> ()
            | AddResult.NoChange id -> ()
            | AddResult.AlreadyExists id -> ()
            | AddResult.MissingRelatedEntity(entityType, id) ->
                // TODO handle
                failwith "todo"
            | AddResult.Failure(message, ``exception``) ->
                // TODO handle
                failwith "todo"

            let resourceVersionId =
                match newVersion.ResourceVersion |> Resources.addVersion ctx false with
                | AddResult.Success id -> id
                | AddResult.NoChange id -> id
                | AddResult.AlreadyExists id -> id
                | AddResult.MissingRelatedEntity(entityType, id) ->
                    // TODO handle
                    failwith "todo"
                | AddResult.Failure(message, ``exception``) ->
                    // TODO handle
                    failwith "todo"

            let previewResourceVersionId =
                newVersion.PreviewResourceVersion
                |> Option.map (fun nv ->
                    match
                        Resources.add
                            ctx
                            { Id = IdType.Specific nv.ResourceId
                              Name = nv.ResourceId }
                    with
                    | AddResult.Success id -> ()
                    | AddResult.NoChange id -> ()
                    | AddResult.AlreadyExists id -> ()
                    | AddResult.MissingRelatedEntity(entityType, id) ->
                        // TODO handle
                        failwith "todo"
                    | AddResult.Failure(message, ``exception``) ->
                        // TODO handle
                        failwith "todo"
                    
                    match Resources.addVersion ctx false nv with
                    | AddResult.Success id -> id
                    | AddResult.NoChange id -> id
                    | AddResult.AlreadyExists id -> id
                    | AddResult.MissingRelatedEntity(entityType, id) ->
                        // TODO handle
                        failwith "todo"
                    | AddResult.Failure(message, ``exception``) ->
                        // TODO handle
                        failwith "todo")

            ({ Id = id
               ImageId = newVersion.ImageId
               Version = 1
               ResourceVersionId = resourceVersionId
               PreviewResourceVersionId = previewResourceVersionId
               Url = newVersion.Url
               PreviewUrl = newVersion.PreviewUrl
               ThanksHtml = newVersion.ThanksHtml }
            : Parameters.NewImageVersion)
            |> Operations.insertImageVersion ctx

            AddResult.Success id
