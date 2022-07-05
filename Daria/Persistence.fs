namespace Daria.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.Sqlite

/// Module generated on 20/06/2022 21:32:12 (utc) via Freql.Sqlite.Tools.
[<RequireQualifiedAccess>]
module Records =
    /// A record representing a row in the table `article_links`.
    type ArticleLink =
        { [<JsonPropertyName("article")>] Article: string
          [<JsonPropertyName("title")>] Title: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("url")>] Url: string }
    
        static member Blank() =
            { Article = String.Empty
              Title = String.Empty
              Description = String.Empty
              Url = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE article_links (
	article TEXT NOT NULL,
	title TEXT NOT NULL,
	description TEXT NOT NULL,
	url TEXT NOT NULL,
	CONSTRAINT article_links_FK FOREIGN KEY (article) REFERENCES articles(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              article,
              title,
              description,
              url
        FROM article_links
        """
    
        static member TableName() = "article_links"
    
    /// A record representing a row in the table `article_share_links`.
    type ArticleShareLink =
        { [<JsonPropertyName("article")>] Article: string
          [<JsonPropertyName("linkType")>] LinkType: string }
    
        static member Blank() =
            { Article = String.Empty
              LinkType = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE article_share_links (
	article TEXT NOT NULL,
	link_type TEXT NOT NULL,
	CONSTRAINT article_share_links_PK PRIMARY KEY (article,link_type),
	CONSTRAINT article_share_links_FK FOREIGN KEY (article) REFERENCES articles(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              article,
              link_type
        FROM article_share_links
        """
    
        static member TableName() = "article_share_links"
    
    /// A record representing a row in the table `article_tags`.
    type ArticleTag =
        { [<JsonPropertyName("article")>] Article: string
          [<JsonPropertyName("tag")>] Tag: string }
    
        static member Blank() =
            { Article = String.Empty
              Tag = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE article_tags (
	article TEXT NOT NULL,
	tag TEXT NOT NULL,
	CONSTRAINT article_tags_PK PRIMARY KEY (article,tag),
	CONSTRAINT article_tags_FK FOREIGN KEY (article) REFERENCES articles(name),
	CONSTRAINT article_tags_FK_1 FOREIGN KEY (tag) REFERENCES tags(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              article,
              tag
        FROM article_tags
        """
    
        static member TableName() = "article_tags"
    
    /// A record representing a row in the table `article_version`.
    type ArticleVersion =
        { [<JsonPropertyName("article")>] Article: string
          [<JsonPropertyName("versionNumber")>] VersionNumber: int
          [<JsonPropertyName("publishDate")>] PublishDate: DateTime
          [<JsonPropertyName("rawBlob")>] RawBlob: BlobField }
    
        static member Blank() =
            { Article = String.Empty
              VersionNumber = 0
              PublishDate = DateTime.UtcNow
              RawBlob = BlobField.Empty() }
    
        static member CreateTableSql() = """
        CREATE TABLE article_version (
	article TEXT NOT NULL,
	version_number INTEGER NOT NULL,
	publish_date TEXT NOT NULL,
	raw_blob BLOB NOT NULL,
	CONSTRAINT article_version_PK PRIMARY KEY (article,version_number),
	CONSTRAINT article_version_FK FOREIGN KEY (article) REFERENCES articles(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              article,
              version_number,
              publish_date,
              raw_blob
        FROM article_version
        """
    
        static member TableName() = "article_version"
    
    /// A record representing a row in the table `articles`.
    type Article =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("url")>] Url: string
          [<JsonPropertyName("partNumber")>] PartNumber: int
          [<JsonPropertyName("series")>] Series: string
          [<JsonPropertyName("title")>] Title: string
          [<JsonPropertyName("titleSlug")>] TitleSlug: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("imagePath")>] ImagePath: string
          [<JsonPropertyName("previewImageUrl")>] PreviewImageUrl: string
          [<JsonPropertyName("thanks")>] Thanks: string option
          [<JsonPropertyName("rawLink")>] RawLink: string option
          [<JsonPropertyName("overrideCssUrl")>] OverrideCssUrl: string option }
    
        static member Blank() =
            { Name = String.Empty
              Url = String.Empty
              PartNumber = 0
              Series = String.Empty
              Title = String.Empty
              TitleSlug = String.Empty
              Description = String.Empty
              ImagePath = String.Empty
              PreviewImageUrl = String.Empty
              Thanks = None
              RawLink = None
              OverrideCssUrl = None }
    
        static member CreateTableSql() = """
        CREATE TABLE articles (
	name TEXT NOT NULL,
	url TEXT NOT NULL,
	part_number INTEGER NOT NULL,
	series TEXT NOT NULL,
	title TEXT NOT NULL,
	title_slug TEXT NOT NULL,
	description TEXT NOT NULL,
	image_path TEXT NOT NULL,
	preview_image_url TEXT NOT NULL,
	thanks TEXT,
	raw_link TEXT,
	override_css_url TEXT,
	CONSTRAINT articles_PK PRIMARY KEY (name),
	CONSTRAINT articles_FK FOREIGN KEY (series) REFERENCES series(name)
)
        """
    
        static member SelectSql() = """
        SELECT
              name,
              url,
              part_number,
              series,
              title,
              title_slug,
              description,
              image_path,
              preview_image_url,
              thanks,
              raw_link,
              override_css_url
        FROM articles
        """
    
        static member TableName() = "articles"
    
    /// A record representing a row in the table `resources`.
    type Resource =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("outputPath")>] OutputPath: string
          [<JsonPropertyName("rawBlob")>] RawBlob: BlobField }
    
        static member Blank() =
            { Name = String.Empty
              OutputPath = String.Empty
              RawBlob = BlobField.Empty() }
    
        static member CreateTableSql() = """
        CREATE TABLE resources (
	name TEXT NOT NULL,
	output_path TEXT NOT NULL,
	raw_blob BLOB NOT NULL,
	CONSTRAINT resources_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              name,
              output_path,
              raw_blob
        FROM resources
        """
    
        static member TableName() = "resources"
    
    /// A record representing a row in the table `series`.
    type Series =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("nameSlug")>] NameSlug: string }
    
        static member Blank() =
            { Name = String.Empty
              NameSlug = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE series (
	name TEXT NOT NULL, name_slug TEXT NOT NULL,
	CONSTRAINT series_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              name,
              name_slug
        FROM series
        """
    
        static member TableName() = "series"
    
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
              name
        FROM tags
        """
    
        static member TableName() = "tags"
    

/// Module generated on 20/06/2022 21:32:12 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Parameters =
    /// A record representing a new row in the table `article_links`.
    type NewArticleLink =
        { [<JsonPropertyName("article")>] Article: string
          [<JsonPropertyName("title")>] Title: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("url")>] Url: string }
    
        static member Blank() =
            { Article = String.Empty
              Title = String.Empty
              Description = String.Empty
              Url = String.Empty }
    
    
    /// A record representing a new row in the table `article_share_links`.
    type NewArticleShareLink =
        { [<JsonPropertyName("article")>] Article: string
          [<JsonPropertyName("linkType")>] LinkType: string }
    
        static member Blank() =
            { Article = String.Empty
              LinkType = String.Empty }
    
    
    /// A record representing a new row in the table `article_tags`.
    type NewArticleTag =
        { [<JsonPropertyName("article")>] Article: string
          [<JsonPropertyName("tag")>] Tag: string }
    
        static member Blank() =
            { Article = String.Empty
              Tag = String.Empty }
    
    
    /// A record representing a new row in the table `article_version`.
    type NewArticleVersion =
        { [<JsonPropertyName("article")>] Article: string
          [<JsonPropertyName("versionNumber")>] VersionNumber: int
          [<JsonPropertyName("publishDate")>] PublishDate: DateTime
          [<JsonPropertyName("rawBlob")>] RawBlob: BlobField }
    
        static member Blank() =
            { Article = String.Empty
              VersionNumber = 0
              PublishDate = DateTime.UtcNow
              RawBlob = BlobField.Empty() }
    
    
    /// A record representing a new row in the table `articles`.
    type NewArticle =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("url")>] Url: string
          [<JsonPropertyName("partNumber")>] PartNumber: int
          [<JsonPropertyName("series")>] Series: string
          [<JsonPropertyName("title")>] Title: string
          [<JsonPropertyName("titleSlug")>] TitleSlug: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("imagePath")>] ImagePath: string
          [<JsonPropertyName("previewImageUrl")>] PreviewImageUrl: string
          [<JsonPropertyName("thanks")>] Thanks: string option
          [<JsonPropertyName("rawLink")>] RawLink: string option
          [<JsonPropertyName("overrideCssUrl")>] OverrideCssUrl: string option }
    
        static member Blank() =
            { Name = String.Empty
              Url = String.Empty
              PartNumber = 0
              Series = String.Empty
              Title = String.Empty
              TitleSlug = String.Empty
              Description = String.Empty
              ImagePath = String.Empty
              PreviewImageUrl = String.Empty
              Thanks = None
              RawLink = None
              OverrideCssUrl = None }
    
    
    /// A record representing a new row in the table `resources`.
    type NewResource =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("outputPath")>] OutputPath: string
          [<JsonPropertyName("rawBlob")>] RawBlob: BlobField }
    
        static member Blank() =
            { Name = String.Empty
              OutputPath = String.Empty
              RawBlob = BlobField.Empty() }
    
    
    /// A record representing a new row in the table `series`.
    type NewSeries =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("nameSlug")>] NameSlug: string }
    
        static member Blank() =
            { Name = String.Empty
              NameSlug = String.Empty }
    
    
    /// A record representing a new row in the table `tags`.
    type NewTag =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
    
/// Module generated on 20/06/2022 21:32:12 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Operations =

    let buildSql (lines: string list) = lines |> String.concat Environment.NewLine

    /// Select a `Records.ArticleLink` from the table `article_links`.
    /// Internally this calls `context.SelectSingleAnon<Records.ArticleLink>` and uses Records.ArticleLink.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleLinkRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleLinkRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleLink.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ArticleLink>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ArticleLink>` and uses Records.ArticleLink.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleLinkRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleLinkRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleLink.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ArticleLink>(sql, parameters)
    
    let insertArticleLink (context: SqliteContext) (parameters: Parameters.NewArticleLink) =
        context.Insert("article_links", parameters)
    
    /// Select a `Records.ArticleShareLink` from the table `article_share_links`.
    /// Internally this calls `context.SelectSingleAnon<Records.ArticleShareLink>` and uses Records.ArticleShareLink.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleShareLinkRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleShareLinkRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleShareLink.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ArticleShareLink>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ArticleShareLink>` and uses Records.ArticleShareLink.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleShareLinkRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleShareLinkRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleShareLink.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ArticleShareLink>(sql, parameters)
    
    let insertArticleShareLink (context: SqliteContext) (parameters: Parameters.NewArticleShareLink) =
        context.Insert("article_share_links", parameters)
    
    /// Select a `Records.ArticleTag` from the table `article_tags`.
    /// Internally this calls `context.SelectSingleAnon<Records.ArticleTag>` and uses Records.ArticleTag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleTagRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleTagRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleTag.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ArticleTag>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ArticleTag>` and uses Records.ArticleTag.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArticleTagRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArticleTagRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ArticleTag.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ArticleTag>(sql, parameters)
    
    let insertArticleTag (context: SqliteContext) (parameters: Parameters.NewArticleTag) =
        context.Insert("article_tags", parameters)
    
    /// Select a `Records.ArticleVersion` from the table `article_version`.
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
        context.Insert("article_version", parameters)
    
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
    