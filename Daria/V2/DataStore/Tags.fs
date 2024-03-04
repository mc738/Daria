namespace Daria.V2.DataStore

open Microsoft.FSharp.Core

module Tags =

    open Freql.Sqlite
    open Daria.V2.DataStore.Persistence

    let list (ctx: SqliteContext) =
        Operations.selectTagRecords ctx [] [] |> List.map (fun t -> t.Name)

    let exists (ctx: SqliteContext) (tag: string) =
        Operations.selectTagRecord ctx [ "WHERE name = @0;" ] [ tag ] |> Option.isSome

    let add (ctx: SqliteContext) (tag: string) =
        match exists ctx tag with
        | true -> ()
        | false -> ({ Name = tag }: Parameters.NewTag) |> Operations.insertTag ctx
