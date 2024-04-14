﻿namespace Daria.V2.Operations.Build

open Microsoft.FSharp.Core

module ExportResources =

    open System.IO
    open Freql.Sqlite
    open Daria.V2.DataStore.Common
    open Daria.V2.DataStore

    let exportImages (ctx: SqliteContext) (rootPath: string) =

        Images.fetchExportImageList ctx
        |> List.iter (fun ili ->
            ili.Versions
            |> List.iter (fun ilv ->
                Resources.getExportVersion ctx ilv.ResourceVersionId
                |> Option.iter (fun rve ->
                    File.WriteAllBytes(
                        Path.Combine(
                            rootPath,
                            ``create image version name`` ili.Name ilv.Version (rve.FileType.GetExtension())
                        ),
                        rve.Blob
                    ))

                ilv.PreviewResourceVersionId
                |> Option.bind (Resources.getExportVersion ctx)
                |> Option.iter (fun rve ->
                    File.WriteAllBytes(
                        Path.Combine(
                            rootPath,
                            ``create image version name`` ili.Name ilv.Version (rve.FileType.GetExtension())
                        ),
                        rve.Blob
                    )))





            ())
