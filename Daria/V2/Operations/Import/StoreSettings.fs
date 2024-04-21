namespace Daria.V2.Operations.Import

open Daria.V2.DataStore
open Daria.V2.Operations.Common
open Freql.Sqlite

module StoreSettings =
    
    let import (ctx: SqliteContext) (settings: ImportSettings) =
        settings.StoreSettings
        |> List.iter (fun s -> Settings.add ctx s.Key s.Value)
        

