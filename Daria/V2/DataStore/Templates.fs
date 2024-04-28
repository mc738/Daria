namespace Daria.V2.DataStore

open Daria.V2.DataStore.Models

module Templates =

    open System
    open FsToolbox.Extensions.Strings
    open Freql.Sqlite
    open Daria.V2.Common
    open Daria.V2.DataStore.Common
    open Daria.V2.DataStore.Persistence
    open Daria.V2.DataStore.Models

    module Internal =

        let fetchSpecificVersion (ctx: SqliteContext) (templateId: string) (version: int) =
            Operations.selectTemplateVersionRecord
                ctx
                [ "WHERE template_id = @0 AND version = @1" ]
                [ templateId; version ]

        let fetchLatestVersion (ctx: SqliteContext) (templateId: string) =
            Operations.selectTemplateVersionRecord
                ctx
                [ "WHERE template_id = @0"; "ORDER BY version DESC"; "LIMIT 1" ]
                [ templateId ]

        let all (ctx: SqliteContext) =
            Operations.selectTemplateRecords ctx [] []


        let allVersionsForTemplateId (ctx: SqliteContext) (imageId: string) =
            Operations.selectTemplateVersionRecords ctx [ "WHERE template_id = @0" ] [ imageId ]

    let getLatestVersionExportListItem (ctx: SqliteContext) (templateId: string) =
        Internal.fetchLatestVersion ctx templateId
        |> Option.map (fun tv ->
            ({ Id = tv.Id
               Version = tv.Version
               ResourceVersionId = tv.ResourceVersionId }
            : ExportTemplateVersionListItem))

    let getSpecificVersionExportListItem (ctx: SqliteContext) (templateId: string) (version: int) =
        Internal.fetchSpecificVersion ctx templateId version
        |> Option.map (fun tv ->
            ({ Id = tv.Id
               Version = tv.Version
               ResourceVersionId = tv.ResourceVersionId }
            : ExportTemplateVersionListItem))




    let versionExists (ctx: SqliteContext) (versionId: string) =
        Operations.selectTemplateVersionRecord ctx [ "WHERE id = @0" ] [ versionId ]
        |> Option.isSome

    let exists (ctx: SqliteContext) (id: string) =
        Operations.selectTemplateRecord ctx [ "WHERE id = @0" ] [ id ] |> Option.isSome

    let add (ctx: SqliteContext) (newTemplate: NewTemplate) =
        let id = newTemplate.Id.ToString()

        match exists ctx id |> not with
        | true ->
            ({ Id = id
               Name = newTemplate.Name
               CreatedOn = DateTime.UtcNow }
            : Parameters.NewTemplate)
            |> Operations.insertTemplate ctx

            AddResult.Success id
        | false -> AddResult.AlreadyExists id


    let addVersion (ctx: SqliteContext) (newVersion: NewTemplateVersion) =

        let id = newVersion.Id.ToString()

        // First check if the version exists (or what the latest version is)

        match versionExists ctx id, Internal.fetchLatestVersion ctx newVersion.TemplateId with
        | true, _ -> AddResult.AlreadyExists id
        | false, Some lv ->
            // Check if either the main or preview blob has changed and
            let prevResourceHash = Resources.getVersionHash ctx lv.ResourceVersionId

            // Try and get the resource version hash.
            // If the blob is Blob.Stream and stream is not seekable this will return None.
            // Which means a new version will always be created.
            let rvh =
                match newVersion.ResourceVersion.ResourceBlob.TryGetHash() with
                | Ok h -> Some h
                | Error _ -> None

            // Check if there has been any changes.
            // This will mean individual changes might need to be checked again.
            // However this is ok because it makes things a bit simpler.
            match ``optional string has changed`` prevResourceHash rvh with
            | true ->
                // Add (if needed) a new version of the resource blob.
                let resourceVersionId =
                    match Resources.exists ctx newVersion.ResourceVersion.ResourceId with
                    | true -> ()
                    | false ->
                        match
                            Resources.add
                                ctx
                                { Id = IdType.Specific newVersion.ResourceVersion.ResourceId
                                  Name = newVersion.ResourceVersion.ResourceId
                                  Bucket = ResourceBuckets.templates }
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

                ({ Id = id
                   TemplateId = newVersion.TemplateId
                   Version = lv.Version + 1
                   ResourceVersionId = resourceVersionId }
                : Parameters.NewTemplateVersion)
                |> Operations.insertTemplateVersion ctx

                AddResult.Success id
            | false -> AddResult.NoChange lv.Id
        | false, None ->

            match
                Resources.add
                    ctx
                    { Id = IdType.Specific newVersion.ResourceVersion.ResourceId
                      Name = newVersion.ResourceVersion.ResourceId
                      Bucket = ResourceBuckets.templates }
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

            ({ Id = id
               TemplateId = newVersion.TemplateId
               Version = 1
               ResourceVersionId = resourceVersionId }
            : Parameters.NewTemplateVersion)
            |> Operations.insertTemplateVersion ctx

            AddResult.Success id

    let fetchExportTemplateList (ctx: SqliteContext) =
        Internal.all ctx
        |> List.map (fun i ->
            ({ Id = i.Id
               Name = i.Name
               Versions =
                 Internal.allVersionsForTemplateId ctx i.Id
                 |> List.map (fun iv ->
                     ({ Id = iv.Id
                        Version = iv.Version
                        ResourceVersionId = iv.ResourceVersionId }
                     : ExportTemplateVersionListItem)) }
            : ExportTemplateListItem))
