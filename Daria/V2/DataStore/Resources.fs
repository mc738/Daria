namespace Daria.V2.DataStore

// No warn for "internal use" warnings.
#nowarn "100001"

open System
open Daria.V2.Common.Domain
open Daria.V2.DataStore.Common
open Freql.Core.Common.Types
open FsToolbox.Core
open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module Resources =

    open System.IO
    open Microsoft.FSharp.Core
    open FsToolbox.Extensions.Streams
    open FsToolbox.Extensions.Strings
    open Freql.Sqlite
    open Daria.V2.DataStore.Common
    open Daria.V2.DataStore.Models
    open Daria.V2.DataStore.Persistence

    module Internal =

        type ResourceVersionListingItem =
            { Id: string
              Version: int
              Hash: string }

            static member SelectSql() =
                "SELECT id, version, hash FROM resource_versions"

        let fetchLatestVersion (ctx: SqliteContext) (resourceId: string) =
            Operations.selectResourceVersionRecord
                ctx
                [ "WHERE resource_id = @0"; "ORDER BY version DESC"; "LIMIT 1" ]
                [ resourceId ]

        let fetchLatestVersionListing (ctx: SqliteContext) (resourceId: string) =

            let sql =
                [ ResourceVersionListingItem.SelectSql()
                  "WHERE resource_id = @0"
                  "ORDER BY version DESC"
                  "LIMIT 1" ]
                |> toSql

            ctx.SelectSingleAnon<ResourceVersionListingItem>(sql, [ resourceId ])

        let fetchVersionListingById (ctx: SqliteContext) (versionId: string) =
            let sql = [ ResourceVersionListingItem.SelectSql(); "WHERE id = @0" ] |> toSql

            ctx.SelectSingleAnon<ResourceVersionListingItem>(sql, [ versionId ])

    let fetchLatestVersionDataAsBytes (ctx: SqliteContext) (resourceId: string) =
        Internal.fetchLatestVersion ctx resourceId
        |> Option.map (fun r -> r.RawBlob.ToString())

    let versionExists (ctx: SqliteContext) (versionId: string) =
        Internal.fetchVersionListingById ctx versionId |> Option.isSome

    let exists (ctx: SqliteContext) (id: string) =
        Operations.selectResourceRecord ctx [ "WHERE id = @0" ] [ id ] |> Option.isSome

    let add (ctx: SqliteContext) (newResource: NewResource) =
        let id = newResource.Id.ToString()

        match exists ctx id |> not with
        | true ->
            ({ Id = id; Name = newResource.Name; Bucket = newResource.Bucket }: Parameters.NewResource)
            |> Operations.insertResource ctx

            AddResult.Success id
        | false -> AddResult.AlreadyExists id

    let addVersion (ctx: SqliteContext) (force: bool) (newVersion: NewResourceVersion) =

        let id = newVersion.Id.ToString()

        // TODO might need to check this gets properly disposed.
        use ms =
            match newVersion.ResourceBlob with
            | Blob.Prepared(memoryStream, hash) -> memoryStream
            | Blob.Stream stream ->
                let ms = stream |> toMemoryStream
                ms
            | Blob.Text t ->
                // Uses let or else the memory stream gets disposed before being used later.
                let ms = new MemoryStream(t.ToUtf8Bytes())
                ms
            | Blob.Bytes b ->
                // Uses let or else the memory stream gets disposed before being used later.
                let ms = new MemoryStream(b)
                ms

        let hash =
            match newVersion.ResourceBlob with
            | Blob.Prepared(_, hash) -> hash
            | _ -> ms.GetSHA256Hash()

        let version, prevHash =
            match Internal.fetchLatestVersionListing ctx newVersion.ResourceId with
            | Some pv -> pv.Version + 1, Some pv.Hash
            | None -> 1, None

        match versionExists ctx id |> not, force || ``hash has changed`` prevHash hash with
        | true, true ->
            ({ Id = id
               ResourceId = newVersion.ResourceId
               Version = version
               RawBlob = BlobField.FromStream ms
               Hash = hash
               CreatedOn = newVersion.CreatedOn |> Option.defaultValue DateTime.UtcNow
               FileType = newVersion.FileType.Serialize()
               EncryptionType = newVersion.EncryptionType.Serialize()
               CompressionType = newVersion.CompressionType.Serialize() }
            : Parameters.NewResourceVersion)
            |> Operations.insertResourceVersion ctx

            AddResult.Success id
        | false, _ -> AddResult.AlreadyExists id
        | _, false -> AddResult.NoChange id

    let getLatestVersionHash (ctx: SqliteContext) (resourceId: string) =
        Internal.fetchLatestVersionListing ctx resourceId
        |> Option.map (fun rv -> rv.Hash)

    let getVersionHash (ctx: SqliteContext) (resourceVersionId: string) =
        Internal.fetchVersionListingById ctx resourceVersionId
        |> Option.map (fun rv -> rv.Hash)


    let getExportVersion (ctx: SqliteContext) (versionId: string) =
        Operations.selectResourceVersionRecord ctx [ "WHERE id = @0" ] [ versionId ]
        |> Option.map (fun rv ->
            ({ Id = rv.Id
               ResourceId = rv.ResourceId
               Version = rv.Version
               Blob = rv.RawBlob.ToBytes()
               Hash = rv.Hash
               FileType = rv.FileType |> FileType.Deserialize |> Option.defaultValue FileType.Binary
               EncryptionType =
                 rv.EncryptionType
                 |> EncryptionType.Deserialize
                 |> Option.defaultValue EncryptionType.None
               CompressionType =
                 rv.CompressionType
                 |> CompressionType.Deserialize
                 |> Option.defaultValue CompressionType.None }
            : ExportResourceVersion))
