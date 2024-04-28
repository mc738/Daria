namespace Daria.V2.Operations.Build

open System.IO
open Microsoft.FSharp.Core

module ExportResources =

    open System.IO
    open Freql.Sqlite
    open Daria.V2.DataStore.Common
    open Daria.V2.DataStore

    let exportImages (ctx: SqliteContext) (rootPath: string) (directoryName: string) =

        let imgPath = Path.Combine(rootPath, directoryName)

        Directory.CreateDirectory(imgPath) |> ignore

        Images.fetchExportImageList ctx
        |> List.iter (fun ili ->
            ili.Versions
            |> List.iter (fun ilv ->
                Resources.getExportVersion ctx ilv.ResourceVersionId
                |> Option.iter (fun rve ->
                    File.WriteAllBytes(
                        Path.Combine(
                            imgPath,
                            ``create image version name`` ili.Name ilv.Version (rve.FileType.GetExtension())
                        ),
                        rve.Blob
                    ))

                ilv.PreviewResourceVersionId
                |> Option.bind (Resources.getExportVersion ctx)
                |> Option.iter (fun rve ->
                    File.WriteAllBytes(
                        Path.Combine(
                            imgPath,
                            ``create image version name`` ili.Name ilv.Version (rve.FileType.GetExtension())
                        ),
                        rve.Blob
                    ))))

    let exportResourceBucket (ctx: SqliteContext) (bucketName: string) (rootPath: string) (directoryName: string) =
        let outputPath = Path.Combine(rootPath, directoryName)

        Resources.getBucket ctx bucketName
        |> List.iter (fun ro ->
            let latestVersion = ro.Versions |> List.maxBy (fun v -> v.Version)

            let name = Path.GetFileNameWithoutExtension(ro.Name)

            // Could use Resources.fetchLatestVersionAsBytes?
            Resources.fetchVersionDataAsBytes ctx latestVersion.Id
            |> Option.iter (fun b ->
                File.WriteAllBytes(Path.Combine(outputPath, $"{name}{latestVersion.FileType.GetExtension()}"), b)))
