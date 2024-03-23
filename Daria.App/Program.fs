open Daria.App.Actions
open Daria.App.Common
open Daria.App.Common.Options

match getOptions () with
| Ok(AppOptions.Import io) -> ImportAction.run io
| Error _ -> failwith "Failed to parse options."
