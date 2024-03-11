namespace Daria.V2.Common

open System
open System.Globalization

module Metadata =


    module Keys =

        let prefix = "daria"

        let seriesId = $"{prefix}:series_id"
        
        let seriesName = $"{prefix}:series_name"
        
        let articleId = $"{prefix}:article_id"

        let articleName = $"{prefix}:article_name"
        
        let draft = $"{prefix}:draft"

        let imageId = $"{prefix}:image_id"

        let imageVersion = $"{prefix}:image_version"

        let imageVersionId = $"{prefix}:image_version_id"

        let tags = $"{prefix}:tags"

        let title = $"{prefix}:title"

        let titleSlug = $"{prefix}:title_slug"

        let order = $"{prefix}:order"
        
        let createdOn = $"{prefix}:created_on"

    let tryToBool (value: string) =
        match value.ToLower() with
        | "true"
        | "1"
        | "ok"
        | "yes" -> Some true
        | "false"
        | "0"
        | "no" -> Some false
        | _ -> None

    let tryToInt (value: string) =
        match Int32.TryParse value with
        | true, v -> Some v
        | false, _ -> None

    let splitValues (value: string) =
        value.Split([| ';'; ',' |], StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)
        |> List.ofArray

    let tryToDateTime (formats: string list) (value: string) =
        match
            DateTime.TryParseExact(
                value,
                formats |> Array.ofList,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal
            )
        with
        | true, v -> Some v
        | false, _ -> None
