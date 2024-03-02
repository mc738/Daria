namespace Daria.V2

open Daria.V2

module DataStore =

    open Freql.Sqlite
    open Daria.V2
    open Daria.V2.Persistence

    [<AutoOpen>]
    module Common =
        
        let storeVersions = "2"
    
    [<RequireQualifiedAccess>]
    module Initialization =

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
            
        let setInitializ

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
              Records.ImagineVersion.CreateTableSql()
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
              Records.ArtifactMetadataItem.CreateTableSql()
              Records.ArticleVersionTag.CreateTableSql() ]
            |> List.iter (ctx.ExecuteSqlNonQuery >> ignore)


            // Seed data
            seedFileTypes ctx
            seedCompressionTypes ctx
            seedEncryptionTypes ctx
            
            // Set data

        let runInTransaction (ctx: SqliteContext) =
            ctx.ExecuteInTransaction run

        ()

    ()
