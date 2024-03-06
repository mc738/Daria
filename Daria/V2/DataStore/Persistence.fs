namespace Daria.V2.DataStore.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.Sqlite

/// Module generated on 05/03/2024 19:38:49 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Records =
    /// A record representing a row in the table `article_version_links`.
    type ArticleVersionLink =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("articleVersionId")>] ArticleVersionId: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("url")>] Url: string }
    
        static member Blank() =
            { Id = String.Empty
              ArticleVersionId = String.Empty
              Name = String.Empty
              Description = String.Empty
              Url = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE article_version_links (
	id TEXT NOT NULL,
    article_version_id TEXT NOT NULL,
	name TEXT NOT NULL,
	description TEXT NOT NULL,
	url TEXT NOT NULL,
	CONSTRAINT article_version_links_PK PRIMARY KEY (id),
	CONSTRAINT article_version_links_FK FOREIGN KEY (article_version_id) REFERENCES article_versions(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              article_version_links.`id`,
              article_version_links.`article_version_id`,
              article_version_links.`name`,
              article_version_links.`description`,
              article_version_links.`url`
        FROM article_version_links
        """
    
        static member TableName() = "article_version_links"
    
    /// A record representing a row in the table `article_version_metadata`.
    type ArticleVersionMetadataItem =
        { [<JsonPropertyName("articleVersionId")>] ArticleVersionId: string
          [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { ArticleVersionId = String.Empty
              ItemKey = String.Empty
              ItemValue = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE article_version_metadata (
	article_version_id TEXT NOT NULL,
	item_key TEXT NOT NULL,
	item_value TEXT NOT NULL,
	CONSTRAINT article_version_metadata_PK PRIMARY KEY (article_version_id,item_key),
	CONSTRAINT article_version_metadata_FK FOREIGN KEY (article_version_id) REFERENCES article_versions(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              article_version_metadata.`article_version_id`,
              article_version_metadata.`item_key`,
              article_version_metadata.`item_value`
        FROM article_version_metadata
        """
    
        static member TableName() = "article_version_metadata"
    
    /// A record representing a row in the table `article_version_tags`.
    type ArticleVersionTag =
        { [<JsonPropertyName("articleVersionId")>] ArticleVersionId: string
          [<JsonPropertyName("tag")>] Tag: string }
    
        static member Blank() =
            { ArticleVersionId = String.Empty
              Tag = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE article_version_tags (
	article_version_id TEXT NOT NULL,
	tag TEXT NOT NULL,
	CONSTRAINT article_version_tags_PK PRIMARY KEY (article_version_id,tag),
	CONSTRAINT article_version_tags_FK FOREIGN KEY (article_version_id) REFERENCES article_versions(id),
	CONSTRAINT article_version_tags_FK_1 FOREIGN KEY (tag) REFERENCES tags(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              article_version_tags.`article_version_id`,
              article_version_tags.`tag`
        FROM article_version_tags
        """
    
        static member TableName() = "article_version_tags"
    
    /// A record representing a row in the table `article_versions`.
    type ArticleVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("articleId")>] ArticleId: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("title")>] Title: string
          [<JsonPropertyName("titleSlug")>] TitleSlug: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("articleBlob")>] ArticleBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("imageVersionId")>] ImageVersionId: string option
          [<JsonPropertyName("rawLink")>] RawLink: string option
          [<JsonPropertyName("overrideCssName")>] OverrideCssName: string option
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("publishedOn")>] PublishedOn: bool option
          [<JsonPropertyName("active")>] Active: bool
          [<JsonPropertyName("draft")>] Draft: bool }
    
        static member Blank() =
            { Id = String.Empty
              ArticleId = String.Empty
              Version = 0
              Title = String.Empty
              TitleSlug = String.Empty
              Description = String.Empty
              ArticleBlob = BlobField.Empty()
              Hash = String.Empty
              ImageVersionId = None
              RawLink = None
              OverrideCssName = None
              CreatedOn = DateTime.UtcNow
              PublishedOn = None
              Active = true
              Draft = true }
    
        static member CreateTableSql() = """
        CREATE TABLE article_versions (
	id TEXT NOT NULL,
	article_id TEXT NOT NULL,
	version INTEGER NOT NULL,
	title TEXT NOT NULL,
	title_slug TEXT NOT NULL,
	description TEXT NOT NULL,
	article_blob BLOB NOT NULL,
	hash TEXT NOT NULL,
	image_version_id TEXT,
	raw_link TEXT,
	override_css_name TEXT,
	created_on TEXT NOT NULL,
	published_on TEXT,
	active INTEGER NOT NULL,
	draft INTEGER NOT NULL,
	CONSTRAINT article_versions_PK PRIMARY KEY (id),
	CONSTRAINT article_versions_UN UNIQUE (article_id,version,draft),
	CONSTRAINT article_versions_FK FOREIGN KEY (article_id) REFERENCES articles(id),
	CONSTRAINT article_versions_FK_1 FOREIGN KEY (image_version_id) REFERENCES imagine_versions(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              article_versions.`id`,
              article_versions.`article_id`,
              article_versions.`version`,
              article_versions.`title`,
              article_versions.`title_slug`,
              article_versions.`description`,
              article_versions.`article_blob`,
              article_versions.`hash`,
              article_versions.`image_version_id`,
              article_versions.`raw_link`,
              article_versions.`override_css_name`,
              article_versions.`created_on`,
              article_versions.`published_on`,
              article_versions.`active`,
              article_versions.`draft`
        FROM article_versions
        """
    
        static member TableName() = "article_versions"
    
    /// A record representing a row in the table `articles`.
    type Article =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("seriesId")>] SeriesId: string
          [<JsonPropertyName("articleOrder")>] ArticleOrder: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty
              SeriesId = String.Empty
              ArticleOrder = 0
              CreatedOn = DateTime.UtcNow
              Active = true }
    
        static member CreateTableSql() = """
        CREATE TABLE articles (
	id TEXT NOT NULL,
	name TEXT NOT NULL,
	series_id TEXT NOT NULL,
	article_order INTEGER NOT NULL,
	created_on TEXT NOT NULL,
	active INTEGER NOT NULL,
	CONSTRAINT articles_PK PRIMARY KEY (id),
	CONSTRAINT articles_FK FOREIGN KEY (series_id) REFERENCES series(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              articles.`id`,
              articles.`name`,
              articles.`series_id`,
              articles.`article_order`,
              articles.`created_on`,
              articles.`active`
        FROM articles
        """
    
        static member TableName() = "articles"
    
    /// A record representing a row in the table `artifact_metadata`.
    type ArtifactMetadataItem =
        { [<JsonPropertyName("artifactId")>] ArtifactId: string
          [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { ArtifactId = String.Empty
              ItemKey = String.Empty
              ItemValue = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE artifact_metadata (
	artifact_id TEXT NOT NULL,
	item_key TEXT NOT NULL,
	item_value TEXT NOT NULL,
	CONSTRAINT artifact_metadata_PK PRIMARY KEY (artifact_id,item_key),
	CONSTRAINT artifact_metadata_FK FOREIGN KEY (artifact_id) REFERENCES artifacts(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              artifact_metadata.`artifact_id`,
              artifact_metadata.`item_key`,
              artifact_metadata.`item_value`
        FROM artifact_metadata
        """
    
        static member TableName() = "artifact_metadata"
    
    /// A record representing a row in the table `artifacts`.
    type Artifact =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("description")>] Description: int64 option
          [<JsonPropertyName("resourceVersionId")>] ResourceVersionId: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty
              Description = None
              ResourceVersionId = String.Empty
              CreatedOn = DateTime.UtcNow }
    
        static member CreateTableSql() = """
        CREATE TABLE artifacts (
	id TEXT NOT NULL,
	name TEXT NOT NULL,
	description INTEGER,
	resource_version_id TEXT NOT NULL,
	created_on TEXT NOT NULL,
	CONSTRAINT artifacts_PK PRIMARY KEY (id),
	CONSTRAINT artifacts_FK FOREIGN KEY (resource_version_id) REFERENCES resource_versions(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              artifacts.`id`,
              artifacts.`name`,
              artifacts.`description`,
              artifacts.`resource_version_id`,
              artifacts.`created_on`
        FROM artifacts
        """
    
        static member TableName() = "artifacts"
    
    /// A record representing a row in the table `compression_type`.
    type CompressionType =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE compression_type (
	name TEXT NOT NULL,
	CONSTRAINT compression_type_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              compression_type.`name`
        FROM compression_type
        """
    
        static member TableName() = "compression_type"
    
    /// A record representing a row in the table `encryption_types`.
    type EncryptionType =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE encryption_types (
	name TEXT NOT NULL,
	CONSTRAINT encryption_types_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              encryption_types.`name`
        FROM encryption_types
        """
    
        static member TableName() = "encryption_types"
    
    /// A record representing a row in the table `file_types`.
    type FileType =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("fileExtension")>] FileExtension: string
          [<JsonPropertyName("contentType")>] ContentType: string }
    
        static member Blank() =
            { Name = String.Empty
              FileExtension = String.Empty
              ContentType = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE file_types (
	name TEXT NOT NULL,
	file_extension TEXT NOT NULL,
	content_type TEXT NOT NULL,
	CONSTRAINT file_types_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              file_types.`name`,
              file_types.`file_extension`,
              file_types.`content_type`
        FROM file_types
        """
    
        static member TableName() = "file_types"
    
    /// A record representing a row in the table `images`.
    type Image =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE images (
	id TEXT NOT NULL,
	name TEXT NOT NULL,
	CONSTRAINT images_PK PRIMARY KEY (id)
)
        """
    
        static member SelectSql() = """
        SELECT
              images.`id`,
              images.`name`
        FROM images
        """
    
        static member TableName() = "images"
    
    /// A record representing a row in the table `imagine_versions`.
    type ImagineVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("imageId")>] ImageId: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("resourceVersionId")>] ResourceVersionId: string
          [<JsonPropertyName("previewResourceVersionId")>] PreviewResourceVersionId: string option
          [<JsonPropertyName("url")>] Url: string
          [<JsonPropertyName("previewUrl")>] PreviewUrl: string option }
    
        static member Blank() =
            { Id = String.Empty
              ImageId = String.Empty
              Version = 0
              ResourceVersionId = String.Empty
              PreviewResourceVersionId = None
              Url = String.Empty
              PreviewUrl = None }
    
        static member CreateTableSql() = """
        CREATE TABLE imagine_versions (
	id TEXT NOT NULL,
	image_id TEXT NOT NULL,
	version INTEGER NOT NULL,
	resource_version_id TEXT NOT NULL,
	preview_resource_version_id TEXT,
	url TEXT NOT NULL,
	preview_url TEXT,
	CONSTRAINT imagine_versions_PK PRIMARY KEY (id),
	CONSTRAINT imagine_versions_UN UNIQUE (image_id,version),
	CONSTRAINT imagine_versions_FK FOREIGN KEY (resource_version_id) REFERENCES resource_versions(id),
	CONSTRAINT imagine_versions_FK_1 FOREIGN KEY (resource_version_id) REFERENCES resource_versions(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              imagine_versions.`id`,
              imagine_versions.`image_id`,
              imagine_versions.`version`,
              imagine_versions.`resource_version_id`,
              imagine_versions.`preview_resource_version_id`,
              imagine_versions.`url`,
              imagine_versions.`preview_url`
        FROM imagine_versions
        """
    
        static member TableName() = "imagine_versions"
    
    /// A record representing a row in the table `metadata`.
    type MetadataItem =
        { [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { ItemKey = String.Empty
              ItemValue = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE metadata (
	item_key TEXT NOT NULL,
	item_value TEXT NOT NULL,
	CONSTRAINT metadata_PK PRIMARY KEY (item_key)
)
        """
    
        static member SelectSql() = """
        SELECT
              metadata.`item_key`,
              metadata.`item_value`
        FROM metadata
        """
    
        static member TableName() = "metadata"
    
    /// A record representing a row in the table `resource_versions`.
    type ResourceVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("resourceId")>] ResourceId: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("rawBlob")>] RawBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("fileType")>] FileType: string
          [<JsonPropertyName("encryptionType")>] EncryptionType: string
          [<JsonPropertyName("compressionType")>] CompressionType: string }
    
        static member Blank() =
            { Id = String.Empty
              ResourceId = String.Empty
              Version = 0
              RawBlob = BlobField.Empty()
              Hash = String.Empty
              CreatedOn = DateTime.UtcNow
              FileType = String.Empty
              EncryptionType = String.Empty
              CompressionType = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE resource_versions (
	id TEXT NOT NULL,
	resource_id TEXT NOT NULL,
	version INTEGER NOT NULL,
	raw_blob BLOB NOT NULL,
	hash TEXT NOT NULL,
	created_on TEXT NOT NULL,
	file_type TEXT NOT NULL,
	encryption_type TEXT NOT NULL,
	compression_type TEXT NOT NULL,
	CONSTRAINT resource_versions_PK PRIMARY KEY (id),
	CONSTRAINT resource_versions_UN UNIQUE (resource_id,version),
	CONSTRAINT resource_versions_FK FOREIGN KEY (resource_id) REFERENCES resources(id),
	CONSTRAINT resource_versions_FK_1 FOREIGN KEY (file_type) REFERENCES file_types(name),
	CONSTRAINT resource_versions_FK_2 FOREIGN KEY (encryption_type) REFERENCES encryption_types(name),
	CONSTRAINT resource_versions_FK_3 FOREIGN KEY (compression_type) REFERENCES compression_type(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              resource_versions.`id`,
              resource_versions.`resource_id`,
              resource_versions.`version`,
              resource_versions.`raw_blob`,
              resource_versions.`hash`,
              resource_versions.`created_on`,
              resource_versions.`file_type`,
              resource_versions.`encryption_type`,
              resource_versions.`compression_type`
        FROM resource_versions
        """
    
        static member TableName() = "resource_versions"
    
    /// A record representing a row in the table `resources`.
    type Resource =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE resources (
	id TEXT NOT NULL,
	name TEXT NOT NULL,
	CONSTRAINT resources_PK PRIMARY KEY (id)
)
        """
    
        static member SelectSql() = """
        SELECT
              resources.`id`,
              resources.`name`
        FROM resources
        """
    
        static member TableName() = "resources"
    
    /// A record representing a row in the table `series`.
    type Series =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("parentSeriesId")>] ParentSeriesId: string option
          [<JsonPropertyName("seriesOrder")>] SeriesOrder: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty
              ParentSeriesId = None
              SeriesOrder = 0
              CreatedOn = DateTime.UtcNow
              Active = true }
    
        static member CreateTableSql() = """
        CREATE TABLE series (
	id TEXT NOT NULL,
	name TEXT NOT NULL,
	parent_series_id TEXT,
	series_order INTEGER NOT NULL,
	created_on TEXT NOT NULL, active INTEGER NOT NULL,
	CONSTRAINT series_PK PRIMARY KEY (id),
	CONSTRAINT series_FK FOREIGN KEY (parent_series_id) REFERENCES series(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              series.`id`,
              series.`name`,
              series.`parent_series_id`,
              series.`series_order`,
              series.`created_on`,
              series.`active`
        FROM series
        """
    
        static member TableName() = "series"
    
    /// A record representing a row in the table `series_version_metadata`.
    type SeriesVersionMetadataItem =
        { [<JsonPropertyName("seriesVersionId")>] SeriesVersionId: string
          [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { SeriesVersionId = String.Empty
              ItemKey = String.Empty
              ItemValue = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE series_version_metadata (
	series_version_id TEXT NOT NULL,
	item_key TEXT NOT NULL,
	item_value TEXT NOT NULL,
	CONSTRAINT series_version_metadata_PK PRIMARY KEY (series_version_id,item_key),
	CONSTRAINT series_version_metadata_FK FOREIGN KEY (series_version_id) REFERENCES series_versions(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              series_version_metadata.`series_version_id`,
              series_version_metadata.`item_key`,
              series_version_metadata.`item_value`
        FROM series_version_metadata
        """
    
        static member TableName() = "series_version_metadata"
    
    /// A record representing a row in the table `series_version_tags`.
    type SeriesVersionTag =
        { [<JsonPropertyName("seriesVersionId")>] SeriesVersionId: string
          [<JsonPropertyName("tag")>] Tag: string }
    
        static member Blank() =
            { SeriesVersionId = String.Empty
              Tag = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE series_version_tags (
	series_version_id TEXT NOT NULL,
	tag TEXT NOT NULL,
	CONSTRAINT series_version_tags_PK PRIMARY KEY (series_version_id,tag),
	CONSTRAINT series_version_tags_FK FOREIGN KEY (series_version_id) REFERENCES series_versions(id),
	CONSTRAINT series_version_tags_FK_1 FOREIGN KEY (tag) REFERENCES tags(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              series_version_tags.`series_version_id`,
              series_version_tags.`tag`
        FROM series_version_tags
        """
    
        static member TableName() = "series_version_tags"
    
    /// A record representing a row in the table `series_versions`.
    type SeriesVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("seriesId")>] SeriesId: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("title")>] Title: string
          [<JsonPropertyName("titleSlug")>] TitleSlug: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("indexBlob")>] IndexBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("imageVersionId")>] ImageVersionId: string option
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("active")>] Active: bool
          [<JsonPropertyName("draft")>] Draft: bool }
    
        static member Blank() =
            { Id = String.Empty
              SeriesId = String.Empty
              Version = 0
              Title = String.Empty
              TitleSlug = String.Empty
              Description = String.Empty
              IndexBlob = BlobField.Empty()
              Hash = String.Empty
              ImageVersionId = None
              CreatedOn = DateTime.UtcNow
              Active = true
              Draft = true }
    
        static member CreateTableSql() = """
        CREATE TABLE series_versions (
	id TEXT NOT NULL,
	series_id TEXT NOT NULL,
	version INTEGER NOT NULL,
	title TEXT NOT NULL,
	title_slug TEXT NOT NULL,
	description TEXT NOT NULL,
	index_blob BLOB NOT NULL,
	hash TEXT NOT NULL,
	image_version_id TEXT,
	created_on TEXT NOT NULL, 
	active INTEGER NOT NULL, 
	draft INTEGER NOT NULL,
	CONSTRAINT series_versions_PK PRIMARY KEY (id),
	CONSTRAINT series_versions_UN UNIQUE (series_id,version,draft),
	CONSTRAINT series_versions_FK FOREIGN KEY (series_id) REFERENCES series(id),
	CONSTRAINT series_versions_FK_1 FOREIGN KEY (image_version_id) REFERENCES imagine_versions(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              series_versions.`id`,
              series_versions.`series_id`,
              series_versions.`version`,
              series_versions.`title`,
              series_versions.`title_slug`,
              series_versions.`description`,
              series_versions.`index_blob`,
              series_versions.`hash`,
              series_versions.`image_version_id`,
              series_versions.`created_on`,
              series_versions.`active`,
              series_versions.`draft`
        FROM series_versions
        """
    
        static member TableName() = "series_versions"
    
    /// A record representing a row in the table `setting_key_values`.
    type SettingKeyValue =
        { [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { ItemKey = String.Empty
              ItemValue = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE setting_key_values (
	item_key TEXT NOT NULL,
	item_value TEXT NOT NULL,
	CONSTRAINT setting_key_values_PK PRIMARY KEY (item_key)
)
        """
    
        static member SelectSql() = """
        SELECT
              setting_key_values.`item_key`,
              setting_key_values.`item_value`
        FROM setting_key_values
        """
    
        static member TableName() = "setting_key_values"
    
    /// A record representing a row in the table `tags`.
    type Tag =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE tags (
	name TEXT NOT NULL,
	CONSTRAINT tags_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              tags.`name`
        FROM tags
        """
    
        static member TableName() = "tags"
    
    /// A record representing a row in the table `template_versions`.
    type TemplateVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("templateId")>] TemplateId: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("resourceVersionId")>] ResourceVersionId: string }
    
        static member Blank() =
            { Id = String.Empty
              TemplateId = String.Empty
              Version = 0
              ResourceVersionId = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE template_versions (
	id TEXT NOT NULL,
	template_id TEXT NOT NULL,
	version TEXT NOT NULL,
	resource_version_id TEXT NOT NULL,
	CONSTRAINT template_versions_PK PRIMARY KEY (id),
	CONSTRAINT template_versions_UN UNIQUE (template_id,version),
	CONSTRAINT template_versions_FK FOREIGN KEY (resource_version_id) REFERENCES resource_versions(id)
)
        """
    
        static member SelectSql() = """
        SELECT
              template_versions.`id`,
              template_versions.`template_id`,
              template_versions.`version`,
              template_versions.`resource_version_id`
        FROM template_versions
        """
    
        static member TableName() = "template_versions"
    
    /// A record representing a row in the table `templates`.
    type Template =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty
              CreatedOn = DateTime.UtcNow }
    
        static member CreateTableSql() = """
        CREATE TABLE templates (
	id TEXT NOT NULL,
	name TEXT NOT NULL,
	created_on TEXT NOT NULL,
	CONSTRAINT templates_PK PRIMARY KEY (id)
)
        """
    
        static member SelectSql() = """
        SELECT
              templates.`id`,
              templates.`name`,
              templates.`created_on`
        FROM templates
        """
    
        static member TableName() = "templates"
    

/// Module generated on 05/03/2024 19:38:49 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Parameters =
    /// A record representing a new row in the table `article_version_links`.
    type NewArticleVersionLink =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("articleVersionId")>] ArticleVersionId: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("url")>] Url: string }
    
        static member Blank() =
            { Id = String.Empty
              ArticleVersionId = String.Empty
              Name = String.Empty
              Description = String.Empty
              Url = String.Empty }
    
    
    /// A record representing a new row in the table `article_version_metadata`.
    type NewArticleVersionMetadataItem =
        { [<JsonPropertyName("articleVersionId")>] ArticleVersionId: string
          [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { ArticleVersionId = String.Empty
              ItemKey = String.Empty
              ItemValue = String.Empty }
    
    
    /// A record representing a new row in the table `article_version_tags`.
    type NewArticleVersionTag =
        { [<JsonPropertyName("articleVersionId")>] ArticleVersionId: string
          [<JsonPropertyName("tag")>] Tag: string }
    
        static member Blank() =
            { ArticleVersionId = String.Empty
              Tag = String.Empty }
    
    
    /// A record representing a new row in the table `article_versions`.
    type NewArticleVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("articleId")>] ArticleId: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("title")>] Title: string
          [<JsonPropertyName("titleSlug")>] TitleSlug: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("articleBlob")>] ArticleBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("imageVersionId")>] ImageVersionId: string option
          [<JsonPropertyName("rawLink")>] RawLink: string option
          [<JsonPropertyName("overrideCssName")>] OverrideCssName: string option
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("publishedOn")>] PublishedOn: bool option
          [<JsonPropertyName("active")>] Active: bool
          [<JsonPropertyName("draft")>] Draft: bool }
    
        static member Blank() =
            { Id = String.Empty
              ArticleId = String.Empty
              Version = 0
              Title = String.Empty
              TitleSlug = String.Empty
              Description = String.Empty
              ArticleBlob = BlobField.Empty()
              Hash = String.Empty
              ImageVersionId = None
              RawLink = None
              OverrideCssName = None
              CreatedOn = DateTime.UtcNow
              PublishedOn = None
              Active = true
              Draft = true }
    
    
    /// A record representing a new row in the table `articles`.
    type NewArticle =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("seriesId")>] SeriesId: string
          [<JsonPropertyName("articleOrder")>] ArticleOrder: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty
              SeriesId = String.Empty
              ArticleOrder = 0
              CreatedOn = DateTime.UtcNow
              Active = true }
    
    
    /// A record representing a new row in the table `artifact_metadata`.
    type NewArtifactMetadataItem =
        { [<JsonPropertyName("artifactId")>] ArtifactId: string
          [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { ArtifactId = String.Empty
              ItemKey = String.Empty
              ItemValue = String.Empty }
    
    
    /// A record representing a new row in the table `artifacts`.
    type NewArtifact =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("description")>] Description: int64 option
          [<JsonPropertyName("resourceVersionId")>] ResourceVersionId: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty
              Description = None
              ResourceVersionId = String.Empty
              CreatedOn = DateTime.UtcNow }
    
    
    /// A record representing a new row in the table `compression_type`.
    type NewCompressionType =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
    
    /// A record representing a new row in the table `encryption_types`.
    type NewEncryptionType =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
    
    /// A record representing a new row in the table `file_types`.
    type NewFileType =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("fileExtension")>] FileExtension: string
          [<JsonPropertyName("contentType")>] ContentType: string }
    
        static member Blank() =
            { Name = String.Empty
              FileExtension = String.Empty
              ContentType = String.Empty }
    
    
    /// A record representing a new row in the table `images`.
    type NewImage =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty }
    
    
    /// A record representing a new row in the table `imagine_versions`.
    type NewImagineVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("imageId")>] ImageId: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("resourceVersionId")>] ResourceVersionId: string
          [<JsonPropertyName("previewResourceVersionId")>] PreviewResourceVersionId: string option
          [<JsonPropertyName("url")>] Url: string
          [<JsonPropertyName("previewUrl")>] PreviewUrl: string option }
    
        static member Blank() =
            { Id = String.Empty
              ImageId = String.Empty
              Version = 0
              ResourceVersionId = String.Empty
              PreviewResourceVersionId = None
              Url = String.Empty
              PreviewUrl = None }
    
    
    /// A record representing a new row in the table `metadata`.
    type NewMetadataItem =
        { [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { ItemKey = String.Empty
              ItemValue = String.Empty }
    
    
    /// A record representing a new row in the table `resource_versions`.
    type NewResourceVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("resourceId")>] ResourceId: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("rawBlob")>] RawBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("fileType")>] FileType: string
          [<JsonPropertyName("encryptionType")>] EncryptionType: string
          [<JsonPropertyName("compressionType")>] CompressionType: string }
    
        static member Blank() =
            { Id = String.Empty
              ResourceId = String.Empty
              Version = 0
              RawBlob = BlobField.Empty()
              Hash = String.Empty
              CreatedOn = DateTime.UtcNow
              FileType = String.Empty
              EncryptionType = String.Empty
              CompressionType = String.Empty }
    
    
    /// A record representing a new row in the table `resources`.
    type NewResource =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty }
    
    
    /// A record representing a new row in the table `series`.
    type NewSeries =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("parentSeriesId")>] ParentSeriesId: string option
          [<JsonPropertyName("seriesOrder")>] SeriesOrder: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty
              ParentSeriesId = None
              SeriesOrder = 0
              CreatedOn = DateTime.UtcNow
              Active = true }
    
    
    /// A record representing a new row in the table `series_version_metadata`.
    type NewSeriesVersionMetadataItem =
        { [<JsonPropertyName("seriesVersionId")>] SeriesVersionId: string
          [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { SeriesVersionId = String.Empty
              ItemKey = String.Empty
              ItemValue = String.Empty }
    
    
    /// A record representing a new row in the table `series_version_tags`.
    type NewSeriesVersionTag =
        { [<JsonPropertyName("seriesVersionId")>] SeriesVersionId: string
          [<JsonPropertyName("tag")>] Tag: string }
    
        static member Blank() =
            { SeriesVersionId = String.Empty
              Tag = String.Empty }
    
    
    /// A record representing a new row in the table `series_versions`.
    type NewSeriesVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("seriesId")>] SeriesId: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("title")>] Title: string
          [<JsonPropertyName("titleSlug")>] TitleSlug: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("indexBlob")>] IndexBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("imageVersionId")>] ImageVersionId: string option
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("active")>] Active: bool
          [<JsonPropertyName("draft")>] Draft: bool }
    
        static member Blank() =
            { Id = String.Empty
              SeriesId = String.Empty
              Version = 0
              Title = String.Empty
              TitleSlug = String.Empty
              Description = String.Empty
              IndexBlob = BlobField.Empty()
              Hash = String.Empty
              ImageVersionId = None
              CreatedOn = DateTime.UtcNow
              Active = true
              Draft = true }
    
    
    /// A record representing a new row in the table `setting_key_values`.
    type NewSettingKeyValue =
        { [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { ItemKey = String.Empty
              ItemValue = String.Empty }
    
    
    /// A record representing a new row in the table `tags`.
    type NewTag =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
    
    /// A record representing a new row in the table `template_versions`.
    type NewTemplateVersion =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("templateId")>] TemplateId: string
          [<JsonPropertyName("version")>] Version: int
          [<JsonPropertyName("resourceVersionId")>] ResourceVersionId: string }
    
        static member Blank() =
            { Id = String.Empty
              TemplateId = String.Empty
              Version = 0
              ResourceVersionId = String.Empty }
    
    
    /// A record representing a new row in the table `templates`.
    type NewTemplate =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime }
    
        static member Blank() =
            { Id = String.Empty
              Name = String.Empty
              CreatedOn = DateTime.UtcNow }
    
    
/// Module generated on 05/03/2024 19:38:49 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Operations =

    let buildSql (lines: string list) = lines |> String.concat Environment.NewLine

    /// Select a `Records.ArticleVersionLink` from the table `article_version_links`.
    /// Internally this calls `context.SelectSingleAnon<Records.ArticleVersionLink>` and uses Records.ArticleVersionLink.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleVersionLinkRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleVersionLinkRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleVersionLink.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ArticleVersionLink>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ArticleVersionLink>` and uses Records.ArticleVersionLink.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleVersionLinkRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleVersionLinkRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleVersionLink.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ArticleVersionLink>(sql, parameters)
    
    let insertArticleVersionLink (context: SqliteContext) (parameters: Parameters.NewArticleVersionLink) =
        context.Insert("article_version_links", parameters)
    
    /// Select a `Records.ArticleVersionMetadataItem` from the table `article_version_metadata`.
    /// Internally this calls `context.SelectSingleAnon<Records.ArticleVersionMetadataItem>` and uses Records.ArticleVersionMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleVersionMetadataItemRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleVersionMetadataItemRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleVersionMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ArticleVersionMetadataItem>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ArticleVersionMetadataItem>` and uses Records.ArticleVersionMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleVersionMetadataItemRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleVersionMetadataItemRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleVersionMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ArticleVersionMetadataItem>(sql, parameters)
    
    let insertArticleVersionMetadataItem (context: SqliteContext) (parameters: Parameters.NewArticleVersionMetadataItem) =
        context.Insert("article_version_metadata", parameters)
    
    /// Select a `Records.ArticleVersionTag` from the table `article_version_tags`.
    /// Internally this calls `context.SelectSingleAnon<Records.ArticleVersionTag>` and uses Records.ArticleVersionTag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleVersionTagRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleVersionTagRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleVersionTag.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ArticleVersionTag>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ArticleVersionTag>` and uses Records.ArticleVersionTag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleVersionTagRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleVersionTagRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleVersionTag.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ArticleVersionTag>(sql, parameters)
    
    let insertArticleVersionTag (context: SqliteContext) (parameters: Parameters.NewArticleVersionTag) =
        context.Insert("article_version_tags", parameters)
    
    /// Select a `Records.ArticleVersion` from the table `article_versions`.
    /// Internally this calls `context.SelectSingleAnon<Records.ArticleVersion>` and uses Records.ArticleVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleVersionRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleVersionRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleVersion.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ArticleVersion>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ArticleVersion>` and uses Records.ArticleVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleVersionRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleVersionRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleVersion.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ArticleVersion>(sql, parameters)
    
    let insertArticleVersion (context: SqliteContext) (parameters: Parameters.NewArticleVersion) =
        context.Insert("article_versions", parameters)
    
    /// Select a `Records.Article` from the table `articles`.
    /// Internally this calls `context.SelectSingleAnon<Records.Article>` and uses Records.Article.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Article.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Article>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Article>` and uses Records.Article.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Article.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Article>(sql, parameters)
    
    let insertArticle (context: SqliteContext) (parameters: Parameters.NewArticle) =
        context.Insert("articles", parameters)
    
    /// Select a `Records.ArtifactMetadataItem` from the table `artifact_metadata`.
    /// Internally this calls `context.SelectSingleAnon<Records.ArtifactMetadataItem>` and uses Records.ArtifactMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArtifactMetadataItemRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArtifactMetadataItemRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArtifactMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ArtifactMetadataItem>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ArtifactMetadataItem>` and uses Records.ArtifactMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArtifactMetadataItemRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArtifactMetadataItemRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArtifactMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ArtifactMetadataItem>(sql, parameters)
    
    let insertArtifactMetadataItem (context: SqliteContext) (parameters: Parameters.NewArtifactMetadataItem) =
        context.Insert("artifact_metadata", parameters)
    
    /// Select a `Records.Artifact` from the table `artifacts`.
    /// Internally this calls `context.SelectSingleAnon<Records.Artifact>` and uses Records.Artifact.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArtifactRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArtifactRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Artifact.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Artifact>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Artifact>` and uses Records.Artifact.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArtifactRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArtifactRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Artifact.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Artifact>(sql, parameters)
    
    let insertArtifact (context: SqliteContext) (parameters: Parameters.NewArtifact) =
        context.Insert("artifacts", parameters)
    
    /// Select a `Records.CompressionType` from the table `compression_type`.
    /// Internally this calls `context.SelectSingleAnon<Records.CompressionType>` and uses Records.CompressionType.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectCompressionTypeRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectCompressionTypeRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.CompressionType.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.CompressionType>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.CompressionType>` and uses Records.CompressionType.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectCompressionTypeRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectCompressionTypeRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.CompressionType.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.CompressionType>(sql, parameters)
    
    let insertCompressionType (context: SqliteContext) (parameters: Parameters.NewCompressionType) =
        context.Insert("compression_type", parameters)
    
    /// Select a `Records.EncryptionType` from the table `encryption_types`.
    /// Internally this calls `context.SelectSingleAnon<Records.EncryptionType>` and uses Records.EncryptionType.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectEncryptionTypeRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectEncryptionTypeRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.EncryptionType.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.EncryptionType>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.EncryptionType>` and uses Records.EncryptionType.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectEncryptionTypeRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectEncryptionTypeRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.EncryptionType.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.EncryptionType>(sql, parameters)
    
    let insertEncryptionType (context: SqliteContext) (parameters: Parameters.NewEncryptionType) =
        context.Insert("encryption_types", parameters)
    
    /// Select a `Records.FileType` from the table `file_types`.
    /// Internally this calls `context.SelectSingleAnon<Records.FileType>` and uses Records.FileType.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectFileTypeRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectFileTypeRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.FileType.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.FileType>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.FileType>` and uses Records.FileType.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectFileTypeRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectFileTypeRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.FileType.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.FileType>(sql, parameters)
    
    let insertFileType (context: SqliteContext) (parameters: Parameters.NewFileType) =
        context.Insert("file_types", parameters)
    
    /// Select a `Records.Image` from the table `images`.
    /// Internally this calls `context.SelectSingleAnon<Records.Image>` and uses Records.Image.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectImageRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectImageRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Image.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Image>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Image>` and uses Records.Image.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectImageRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectImageRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Image.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Image>(sql, parameters)
    
    let insertImage (context: SqliteContext) (parameters: Parameters.NewImage) =
        context.Insert("images", parameters)
    
    /// Select a `Records.ImagineVersion` from the table `imagine_versions`.
    /// Internally this calls `context.SelectSingleAnon<Records.ImagineVersion>` and uses Records.ImagineVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectImagineVersionRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectImagineVersionRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ImagineVersion.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ImagineVersion>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ImagineVersion>` and uses Records.ImagineVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectImagineVersionRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectImagineVersionRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ImagineVersion.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ImagineVersion>(sql, parameters)
    
    let insertImagineVersion (context: SqliteContext) (parameters: Parameters.NewImagineVersion) =
        context.Insert("imagine_versions", parameters)
    
    /// Select a `Records.MetadataItem` from the table `metadata`.
    /// Internally this calls `context.SelectSingleAnon<Records.MetadataItem>` and uses Records.MetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectMetadataItemRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectMetadataItemRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.MetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.MetadataItem>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.MetadataItem>` and uses Records.MetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectMetadataItemRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectMetadataItemRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.MetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.MetadataItem>(sql, parameters)
    
    let insertMetadataItem (context: SqliteContext) (parameters: Parameters.NewMetadataItem) =
        context.Insert("metadata", parameters)
    
    /// Select a `Records.ResourceVersion` from the table `resource_versions`.
    /// Internally this calls `context.SelectSingleAnon<Records.ResourceVersion>` and uses Records.ResourceVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectResourceVersionRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectResourceVersionRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ResourceVersion.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ResourceVersion>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ResourceVersion>` and uses Records.ResourceVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectResourceVersionRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectResourceVersionRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ResourceVersion.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ResourceVersion>(sql, parameters)
    
    let insertResourceVersion (context: SqliteContext) (parameters: Parameters.NewResourceVersion) =
        context.Insert("resource_versions", parameters)
    
    /// Select a `Records.Resource` from the table `resources`.
    /// Internally this calls `context.SelectSingleAnon<Records.Resource>` and uses Records.Resource.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectResourceRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectResourceRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Resource.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Resource>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Resource>` and uses Records.Resource.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectResourceRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectResourceRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Resource.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Resource>(sql, parameters)
    
    let insertResource (context: SqliteContext) (parameters: Parameters.NewResource) =
        context.Insert("resources", parameters)
    
    /// Select a `Records.Series` from the table `series`.
    /// Internally this calls `context.SelectSingleAnon<Records.Series>` and uses Records.Series.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSeriesRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectSeriesRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Series.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Series>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Series>` and uses Records.Series.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSeriesRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectSeriesRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Series.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Series>(sql, parameters)
    
    let insertSeries (context: SqliteContext) (parameters: Parameters.NewSeries) =
        context.Insert("series", parameters)
    
    /// Select a `Records.SeriesVersionMetadataItem` from the table `series_version_metadata`.
    /// Internally this calls `context.SelectSingleAnon<Records.SeriesVersionMetadataItem>` and uses Records.SeriesVersionMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSeriesVersionMetadataItemRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectSeriesVersionMetadataItemRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.SeriesVersionMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.SeriesVersionMetadataItem>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.SeriesVersionMetadataItem>` and uses Records.SeriesVersionMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSeriesVersionMetadataItemRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectSeriesVersionMetadataItemRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.SeriesVersionMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.SeriesVersionMetadataItem>(sql, parameters)
    
    let insertSeriesVersionMetadataItem (context: SqliteContext) (parameters: Parameters.NewSeriesVersionMetadataItem) =
        context.Insert("series_version_metadata", parameters)
    
    /// Select a `Records.SeriesVersionTag` from the table `series_version_tags`.
    /// Internally this calls `context.SelectSingleAnon<Records.SeriesVersionTag>` and uses Records.SeriesVersionTag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSeriesVersionTagRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectSeriesVersionTagRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.SeriesVersionTag.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.SeriesVersionTag>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.SeriesVersionTag>` and uses Records.SeriesVersionTag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSeriesVersionTagRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectSeriesVersionTagRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.SeriesVersionTag.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.SeriesVersionTag>(sql, parameters)
    
    let insertSeriesVersionTag (context: SqliteContext) (parameters: Parameters.NewSeriesVersionTag) =
        context.Insert("series_version_tags", parameters)
    
    /// Select a `Records.SeriesVersion` from the table `series_versions`.
    /// Internally this calls `context.SelectSingleAnon<Records.SeriesVersion>` and uses Records.SeriesVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSeriesVersionRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectSeriesVersionRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.SeriesVersion.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.SeriesVersion>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.SeriesVersion>` and uses Records.SeriesVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSeriesVersionRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectSeriesVersionRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.SeriesVersion.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.SeriesVersion>(sql, parameters)
    
    let insertSeriesVersion (context: SqliteContext) (parameters: Parameters.NewSeriesVersion) =
        context.Insert("series_versions", parameters)
    
    /// Select a `Records.SettingKeyValue` from the table `setting_key_values`.
    /// Internally this calls `context.SelectSingleAnon<Records.SettingKeyValue>` and uses Records.SettingKeyValue.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSettingKeyValueRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectSettingKeyValueRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.SettingKeyValue.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.SettingKeyValue>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.SettingKeyValue>` and uses Records.SettingKeyValue.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSettingKeyValueRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectSettingKeyValueRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.SettingKeyValue.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.SettingKeyValue>(sql, parameters)
    
    let insertSettingKeyValue (context: SqliteContext) (parameters: Parameters.NewSettingKeyValue) =
        context.Insert("setting_key_values", parameters)
    
    /// Select a `Records.Tag` from the table `tags`.
    /// Internally this calls `context.SelectSingleAnon<Records.Tag>` and uses Records.Tag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTagRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectTagRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Tag.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Tag>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Tag>` and uses Records.Tag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTagRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectTagRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Tag.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Tag>(sql, parameters)
    
    let insertTag (context: SqliteContext) (parameters: Parameters.NewTag) =
        context.Insert("tags", parameters)
    
    /// Select a `Records.TemplateVersion` from the table `template_versions`.
    /// Internally this calls `context.SelectSingleAnon<Records.TemplateVersion>` and uses Records.TemplateVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTemplateVersionRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectTemplateVersionRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.TemplateVersion.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.TemplateVersion>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.TemplateVersion>` and uses Records.TemplateVersion.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTemplateVersionRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectTemplateVersionRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.TemplateVersion.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.TemplateVersion>(sql, parameters)
    
    let insertTemplateVersion (context: SqliteContext) (parameters: Parameters.NewTemplateVersion) =
        context.Insert("template_versions", parameters)
    
    /// Select a `Records.Template` from the table `templates`.
    /// Internally this calls `context.SelectSingleAnon<Records.Template>` and uses Records.Template.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTemplateRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectTemplateRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Template.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Template>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Template>` and uses Records.Template.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTemplateRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectTemplateRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Template.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Template>(sql, parameters)
    
    let insertTemplate (context: SqliteContext) (parameters: Parameters.NewTemplate) =
        context.Insert("templates", parameters)
    