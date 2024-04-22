namespace Daria.V2.Common

open System

[<AutoOpen>]
module Utils =

    let resultChoose<'T, 'E> (results: Result<'T, 'E> list) =
        results
        |> List.fold
            (fun acc r ->
                match r with
                | Ok v -> v :: acc
                | Error _ -> acc)
            []
        |> List.rev


    let resultCollect<'T> (results: Result<'T, string> list) =
        results
        |> List.fold
            (fun (succ, err) r ->
                match r with
                | Ok v -> v :: succ, err
                | Error e -> succ, e :: err)
            ([], [])
        |> fun (succ, err) ->
            match err.IsEmpty with
            | true -> succ |> List.rev |> Ok
            | false ->
                Error(
                    "There following errors occured:" :: (err |> List.rev)
                    |> String.concat Environment.NewLine
                )
