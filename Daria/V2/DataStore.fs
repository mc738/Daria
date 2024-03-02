namespace Daria.V2

module DataStore =

    open Freql.Sqlite
    open Daria.V2
    open Daria.V2.Persistence

    module Initialization =

        let seedFileTypes (ctx: SqliteContext) =
            FileType.All()
            |> List.iter (fun ft ->
                ({ Name = ft.Serialize()
                   FileExtension = ft.GetExtension()
                   ContentType = ft.GetContentType() }
                : Parameters.NewFileType)
                |> Operations.insertFileType ctx)


        ()

    ()
