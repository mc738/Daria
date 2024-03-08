namespace Daria.V2.DataStore

open System
open System.IO
open Freql.Sqlite
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

        member id.GetValue() = id.ToString()

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

    [<RequireQualifiedAccess>]
    type AddVersionResult =
        | Success of Id: string
        | NotChange
        | Failure of Message: string * Exception: exn option


    /// <summary>
    /// Represents data to be saved as a blob.
    /// It can either be in a prepared or unprepared state.
    /// A prepared blob blob will have data in a memory stream and a precomputed hash.
    /// A unprepared blob will be a raw string
    /// </summary>
    [<RequireQualifiedAccess>]
    type Blob =
        | Prepared of Stream: MemoryStream * Hash: string
        | Stream of Stream
        | Text of string
        | Bytes of Byte array

    type EntityVersion =
        | Specific of Id: string * Version: int
        | Latest of Id: string

    /// <summary>
    /// A union type to definite related entities.
    /// Because a related entity could be predefined or require a look up this is used to model relationships
    /// </summary>
    [<RequireQualifiedAccess>]
    type RelatedEntity =
        | Lookup of Version: EntityVersion
        | Specified of Id: string
        | Bespoke of (SqliteContext -> string option)

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

    let compareHashes (strA: string option) (strB: string) =
        match strA with
        | Some s when s.Equals(strB, StringComparison.OrdinalIgnoreCase) |> not -> true
        | Some _ -> false
        | None -> true
