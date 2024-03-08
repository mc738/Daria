namespace Daria.V2.DataStore

open Freql.Sqlite
open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module Resources =

    open Daria.V2.DataStore.Persistence

    module Internal =

        let fetchLatestVersion (ctx: SqliteContext) (resourceId: string) =
            Operations.selectResourceVersionRecord
                ctx
                [ "WHERE resource_id = @0"; "ORDER BY version DESC"; "LIMIT 1" ]
                [ resourceId ]


    let fetchLatestVersionDataAsBytes (ctx: SqliteContext) (resourceId: string) =
        Internal.fetchLatestVersion ctx resourceId |> Option.map (fun r -> r.RawBlob.ToString())


    ()
