namespace Daria.V2.DataStore

[<RequireQualifiedAccess>]
module Series =

    open Freql.Sqlite
    open Daria.V2.DataStore.Persistence




    let fetchLatestVersion (ctx: SqliteContext) (seriesId: string) (includeDrafts: bool) =
        Operations.selectSeriesVersionRecord
            ctx
            [ "WHERE series_id = @0"
              match includeDrafts with
              | true -> ()
              | false -> "AND draft = 0" ]
            [ seriesId ]

        ()
