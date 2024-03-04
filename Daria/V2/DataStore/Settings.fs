namespace Daria.V2.DataStore

[<RequireQualifiedAccess>]
module Settings =

    open Freql.Sqlite
    open Daria.V2.DataStore.Persistence

    module Internal =

        let fetch (ctx: SqliteContext) (key: string) =
            Operations.selectSettingKeyValueRecord ctx [ "WHERE item_key = @0" ] [ key ]

        let add (ctx: SqliteContext) (key: string) (value: string) =
            ({ ItemKey = key; ItemValue = value }: Parameters.NewSettingKeyValue)
            |> Operations.insertSettingKeyValue ctx

        let update (ctx: SqliteContext) (key: string) (value: string) =
            ctx.ExecuteVerbatimNonQueryAnon(
                "UPDATE setting_key_values SET item_value = @0 WHERE item_key = @1",
                [ value; key ]
            )
            |> ignore

    let exists (ctx: SqliteContext) (key: string) = Internal.fetch ctx key |> Option.isSome
    
    let fetchAll (ctx: SqliteContext) =
        Operations.selectSettingKeyValueRecords ctx [] []
        |> List.map (fun skv -> skv.ItemKey, skv.ItemValue)
        |> Map.ofList

    let fetchValue (ctx: SqliteContext) (key: string) =
        Operations.selectSettingKeyValueRecord ctx [ "WHERE item_key = @0" ] [ key ]
        |> Option.map (fun skv -> skv.ItemValue)

    let add (ctx: SqliteContext) (key: string) (value: string) =
        match exists ctx key with
        | true -> ()
        | false -> Internal.add ctx key value

    let update (ctx: SqliteContext) (key: string) (value: string) =
        match exists ctx key with
        | true -> Internal.update ctx key value
        | false -> ()

    let addOrUpdate (ctx: SqliteContext) (key: string) (value: string) =
        match exists ctx key with
        | true -> Internal.update ctx key value
        | false -> Internal.add ctx key value
