namespace Daria.V2.DataStore

open System
open System.IO
open FsToolbox.Extensions.Strings

module Common =

    let storeVersion = "2"


    type IdType =
        | Generated
        | Specific of string
        | Bespoke of (unit -> string)

        member id.Generate() =
            match id with
            | Generated -> System.Guid.NewGuid().ToString("n") |> IdType.Specific
            | Specific s -> id
            | Bespoke unitFunc -> unitFunc () |> IdType.Specific

        member id.IsSpecified() =
            match id with
            | Generated -> false
            | Specific _ -> true
            | Bespoke _ -> false

        member id.IsDynamic() = id.IsSpecified() |> not

        override id.ToString() =
            match id with
            | Generated -> System.Guid.NewGuid().ToString("n")
            | Specific s -> s
            | Bespoke unitFunc -> unitFunc ()

    [<RequireQualifiedAccess>]
    type ActiveStatus =
        | Active
        | Inactive
        | All

        member a.ToSqlOption(?prefix: string) =
            let pf =
                prefix
                |> Option.map (fun pf ->
                    match pf.EndsWith(" ") with
                    | true -> pf
                    | false -> $"{pf} ")
                |> Option.defaultValue ""

            match a with
            | ActiveStatus.Active -> Some $"{pf}active = TRUE"
            | ActiveStatus.Inactive -> Some $"{pf}active = FALSE"
            | ActiveStatus.All -> None

    [<RequireQualifiedAccess>]
    type DraftStatus =
        | Draft
        | NotDraft
        | All


        member d.ToSqlOption(?prefix: string) =
            let pf =
                prefix
                |> Option.map (fun pf ->
                    match pf.EndsWith(" ") with
                    | true -> pf
                    | false -> $"{pf} ")
                |> Option.defaultValue ""

            match d with
            | DraftStatus.Draft -> Some $"{pf}draft = TRUE"
            | DraftStatus.NotDraft -> Some $"{pf}draft = FALSE"
            | DraftStatus.All -> None

    let toSql (parts: string list) = parts |> String.concat " "

    let toMemoryStream (stream: Stream) =
        match stream with
        | :? MemoryStream -> stream :?> MemoryStream
        | _ ->
            use ms = new MemoryStream()
            stream.CopyTo(ms)
            ms

    let slugify (str: string) =
        str
        |> Seq.choose (fun c ->
            match c with
            | c when Char.IsLetterOrDigit(c) || c = ' ' -> Some c
            | _ -> None)
        |> Array.ofSeq
        |> String
        |> fun s -> s.ToSnakeCase()
