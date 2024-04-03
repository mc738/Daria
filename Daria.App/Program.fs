open Daria.App.Actions
open Daria.App.Common
open Daria.App.Common.Options

match getOptions () with
| Ok(AppOptions.Import io) -> ImportAction.run io
| Ok(AppOptions.Build bo) -> BuildAction.run bo
| Error _ -> failwith "Failed to parse options."
