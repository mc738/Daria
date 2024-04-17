namespace Daria.V2.DataStore

open System



[<RequireQualifiedAccess>]
module Initialization =

    open Freql.Sqlite
    open Daria.V2.Common.Domain
    open Daria.V2.DataStore.Persistence

    let seedFileTypes (ctx: SqliteContext) =
        FileType.All()
        |> List.iter (fun ft ->
            ({ Name = ft.Serialize()
               FileExtension = ft.GetExtension()
               ContentType = ft.GetContentType() }
            : Parameters.NewFileType)
            |> Operations.insertFileType ctx)

    let seedEncryptionTypes (ctx: SqliteContext) =
        EncryptionType.All()
        |> List.iter (fun et ->
            ({ Name = et.Serialize() }: Parameters.NewEncryptionType)
            |> Operations.insertEncryptionType ctx)

    let seedCompressionTypes (ctx: SqliteContext) =
        CompressionType.All()
        |> List.iter (fun ct ->
            ({ Name = ct.Serialize() }: Parameters.NewCompressionType)
            |> Operations.insertCompressionType ctx)

    let setInitializedMetadata ctx =
        let attempt (fn: unit -> string) =
            try
                fn ()
            with _ ->
                "[unknown]"

        [ "store-version", Common.storeVersion
          "initialized", "true"
          "initialized-on", DateTime.UtcNow |> string
          "initialize-user-name", Environment.UserName
          "initialize-machine-name", attempt (fun _ -> Environment.MachineName)
          "initialize-user-domain-name", attempt (fun _ -> Environment.UserDomainName)
          "initialize-dotnet-version", Environment.Version.ToString()
          "initialize-os-version", attempt Environment.OSVersion.ToString ]
        |> List.iter (fun (k, v) ->
            ({ ItemKey = k; ItemValue = v }: Parameters.NewMetadataItem)
            |> Operations.insertMetadataItem ctx)

    let run (ctx: SqliteContext) =
        // Create tables
        [ Records.SettingKeyValue.CreateTableSql()
          Records.MetadataItem.CreateTableSql()
          Records.CompressionType.CreateTableSql()
          Records.EncryptionType.CreateTableSql()
          Records.Tag.CreateTableSql()
          Records.Resource.CreateTableSql()
          Records.ResourceVersion.CreateTableSql()
          Records.Image.CreateTableSql()
          Records.ImageVersion.CreateTableSql()
          Records.Template.CreateTableSql()
          Records.TemplateVersion.CreateTableSql()
          Records.Artifact.CreateTableSql()
          Records.ArtifactMetadataItem.CreateTableSql()
          Records.FileType.CreateTableSql()
          Records.Series.CreateTableSql()
          Records.SeriesVersion.CreateTableSql()
          Records.SeriesVersionMetadataItem.CreateTableSql()
          Records.SeriesVersionTag.CreateTableSql()
          Records.Article.CreateTableSql()
          Records.ArticleVersion.CreateTableSql()
          Records.ArticleVersionLink.CreateTableSql()
          Records.ArticleVersionMetadataItem.CreateTableSql()
          Records.ArticleVersionTag.CreateTableSql() ]
        |> List.iter (ctx.ExecuteSqlNonQuery >> ignore)

        // Seed data
        seedFileTypes ctx
        seedCompressionTypes ctx
        seedEncryptionTypes ctx

        // Set data
        setInitializedMetadata ctx

    let runInTransaction (ctx: SqliteContext) = ctx.ExecuteInTransaction run
