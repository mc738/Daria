namespace Daria.V2.DataStore

open System
open System.IO
open System.Security.Cryptography
open Freql.Sqlite
open FsToolbox.Core
open FsToolbox.Extensions.Strings
open FsToolbox.Extensions.Streams

#nowarn "100001"

module Common =

    let storeVersion = "2"

    [<CompilerMessage("Type should only be used for internal use", 100001)>]
    type IdType =
        | Generated
        | Specific of string
        | Bespoke of (unit -> string)

        member id.Generate() =
            match id with
            | Generated -> Guid.NewGuid().ToString("n") |> IdType.Specific
            | Specific _ -> id
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
            | Generated -> Guid.NewGuid().ToString("n")
            | Specific s -> s
            | Bespoke unitFunc -> unitFunc ()

    [<CompilerMessage("Type should only be used for internal use", 100001)>]
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

    [<CompilerMessage("Type should only be used for internal use", 100001)>]
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
            | DraftStatus.Draft -> Some $"{pf}draft_version IS NOT NULL"
            | DraftStatus.NotDraft -> Some $"{pf}draft_version IS NULL"
            | DraftStatus.All -> None

    [<RequireQualifiedAccess>]
    type AddResult =
        | Success of Id: string
        /// <summary>
        /// Represents and item that has no changes to the previous version.
        /// </summary>
        | NoChange of Id: string
        /// <summary>
        /// Represents an item that already exists. Normally this will make the item's id already exists.
        /// </summary>
        | AlreadyExists of Id: string
        | MissingRelatedEntity of EntityType: string * Id: string
        | Failure of Message: string * Exception: exn option

    /// <summary>
    /// Represents data to be saved as a blob.
    /// It can either be in a prepared or unprepared state.
    /// A prepared blob blob will have data in a memory stream and a precomputed hash.
    /// A unprepared blob will be a raw string
    /// </summary>
    [<CompilerMessage("Type should only be used for internal use", 100001)>]
    [<RequireQualifiedAccess>]
    type Blob =
        | Prepared of Stream: MemoryStream * Hash: string
        | Stream of Stream
        | Text of string
        | Bytes of Byte array
        
        
        member b.TryGetHash() =
            match b with
            | Blob.Prepared(stream, hash) -> hash |> Ok
            | Blob.Stream stream ->
                match stream.CanSeek with
                | true ->
                     use ms = new MemoryStream()
                     stream.CopyTo(ms)
                     stream.Position <- 0L
                     ms.GetSHA256Hash() |> Ok
                | false -> Error "Stream is not seekable"
            | Blob.Text s -> s.GetSHA256Hash() |> Ok
            | Blob.Bytes bytes -> Hashing.generateHash (SHA256.Create()) bytes |> Ok
            
        
        /// <summary>
        /// Get the hash of the blob.
        /// Be warned, if the blob is Blob.Stream and the related stream is not seekable this will fail.
        /// This is because generating the hash will advance the stream position and it can not be reset.
        /// </summary>
        [<CompilerMessage("Method can have fail, have undeterministic results or cause corruption and should only be used for internal use.", 100002)>]
        member b.GetHash() =
            match b with
            | Blob.Prepared(stream, hash) -> hash
            | Blob.Stream stream ->
                // NOTE this is a bit wasteful but
                use ms = new MemoryStream()
                stream.CopyTo(ms)
                // Attempt to reset the stream. If this is not possible then 
                if stream.CanSeek then stream.Position <- 0L else failwith "Stream is not seekable."
                
                ms.GetSHA256Hash()
            | Blob.Text s -> s.GetSHA256Hash()
            | Blob.Bytes bytes -> Hashing.generateHash (SHA256.Create()) bytes

    
    
    
    [<CompilerMessage("Type should only be used for internal use", 100001)>]
    type EntityVersion =
        | Specific of Id: string * Version: int
        | Latest of Id: string

    /// <summary>
    /// A union type to definite related entities.
    /// Because a related entity could be predefined or require a look up this is used to model relationships
    /// </summary>
    [<CompilerMessage("Type should only be used for internal use", 100001)>]
    [<RequireQualifiedAccess>]
    type RelatedEntityVersion =
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

    let ``hash has changed`` (strA: string option) (strB: string) =
        match strA with
        | Some s when s.Equals(strB, StringComparison.OrdinalIgnoreCase) |> not -> true
        | Some _ -> false
        | None -> true

    let ``option hash has changed`` (strA: string option) (strB: string option) =
        match strA, strB with
        | Some s1, Some s2 when s1.Equals(s2,  StringComparison.OrdinalIgnoreCase) |> not -> true
        | Some _, None
        | None, Some _ -> true
        | Some _, Some _
        | None, None -> false
