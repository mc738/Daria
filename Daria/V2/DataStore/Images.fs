namespace Daria.V2.DataStore

open Daria.V2.DataStore.Persistence
open Freql.Sqlite

module Images =


    module Internal =


        let getSpecificVersion (ctx: SqliteContext) (imageId: string) (version: int) =
            Operations.selectImageVersionRecord ctx [ "WHERE image_id = @0 AND version = @1" ] [ imageId; version ]

        let getLatestVersion (ctx: SqliteContext) (imageId: string) =
            Operations.selectImageVersionRecord
                ctx
                [ "WHERE image_id = @0"; "ORDER BY version DESC"; "LIMIT 1" ]
                [ imageId ]



    let getLatestVersion (ctx: SqliteContext) (imageId: string) =



        ()
