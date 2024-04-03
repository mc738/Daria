﻿namespace Daria.V2.DataStore

#nowarn "100001"

open Daria.V2.Common.Domain

module Models =

    open System
    open Daria.V2.DataStore.Common
    open Daria.V2.Common.Domain

    type ImageVersionDetails = { Id: string }

    type SeriesListingItem =
        { Id: string
          Name: string
          Order: int
          CreatedOn: DateTime
          Active: bool
          Children: SeriesListingItem list
          Versions: SeriesVersionOverview list }

    and SeriesOverview =
        { Id: string
          Name: string
          Order: int
          CreatedOn: DateTime
          Active: bool
          ParentSeriesId: string option }

    and SeriesVersionOverview =
        { Id: string
          SeriesId: string
          Version: int
          DraftVersion: int option
          Title: string
          TitleSlug: string
          Description: string
          Hash: string
          ImageVersionId: string option
          CreatedOn: DateTime
          Active: bool }

    type NewSeries =
        { Id: IdType
          Name: string
          ParentId: string option
          SeriesOrder: int
          CreatedOn: DateTime option }

    type NewSeriesVersion =
        { Id: IdType
          SeriesId: string
          Title: string
          TitleSlug: string option
          Description: string
          IndexBlob: Blob
          ImageVersion: RelatedEntityVersion option
          CreatedOn: DateTime option
          Tags: string list
          Metadata: Map<string, string> }

    type ArticleListingItem =
        { Id: string
          Name: string
          SeriesId: string
          Order: int
          CreatedOn: DateTime
          Active: bool
          Versions: ArticleVersionOverview list }

    and ArticleOverview =
        { Id: string
          Name: string
          SeriesId: string
          Order: int
          CreatedOn: DateTime
          Active: bool }

    and ArticleVersionOverview =
        { Id: string
          ArticleId: string
          Version: int
          DraftVersion: int option
          Title: string
          TitleSlug: string
          Description: string
          Hash: string
          ImageVersionId: string option
          RawLink: string option
          OverrideCssName: string option
          CreatedOn: DateTime
          PublishedOn: DateTime option
          Active: bool }

    type NewArticle =
        { Id: IdType
          Name: string
          SeriesId: string
          ArticleOrder: int
          CreatedOn: DateTime option }

    type NewArticleVersion =
        { Id: IdType
          ArticleId: string
          Title: string
          TitleSlug: string option
          Description: string
          ArticleBlob: Blob
          ImageVersion: RelatedEntityVersion option
          RawLink: string option
          OverrideCss: string option
          CreatedOn: DateTime option
          PublishedOn: DateTime option
          Tags: string list
          Metadata: Map<string, string> }

    type ResourceVersionOverview =
        { Id: string
          ResourceId: string
          Version: int
          Hash: string
          FileType: FileType
          EncryptionType: EncryptionType
          CompressionType: CompressionType }

    type NewResourceVersion =
        { Id: IdType
          ResourceId: string
          ResourceBlob: Blob
          CreatedOn: DateTime option
          FileType: FileType
          EncryptionType: EncryptionType
          CompressionType: CompressionType }

    type ArticleLink =
        { Title: string
          Description: string
          Url: string }


    type RenderableArticle =
        { Id: string
          VersionId: string
          Version: int
          Title: string
          TitleSlug: string
          Description: string
          CreatedOn: DateTime
          PublishedOn: DateTime option
          RawLink: string option
          OverrideCssName: string option
          Image: RenderableArticleImage option
          Tags: string list
          NextPart: RenderableArticlePart option
          PreviousPart: RenderableArticlePart option
          AllParts: RenderableArticlePart list
          Links: ArticleLink list }

    and RenderableArticleImage =
        { Name: string
          Extension: string
          PreviewUrl: string option
          Thanks: string }

    and RenderableArticlePart = { Title: string; TitleSlug: string }

    type RenderableSeriesIndex =
        { Id: string
          VersionId: string
          Version: int
          Title: string
          TitleSlug: string
          Description: string
          CreatedOn: DateTime
          Articles: RenderableSeriesIndexArticlePart list
          Series: RenderableSeriesIndexSeriesPart list
          Image: RenderableSeriesIndexImage option
          Tags: string list }

    and RenderableSeriesIndexImage =
        { Name: string
          Extension: string
          PreviewUrl: string option
          Thanks: string }

    and RenderableSeriesIndexArticlePart =
        { Title: string
          TitleSlug: string
          Description: string }

    and RenderableSeriesIndexSeriesPart =
        { Title: string
          TitleSlug: string
          Description: string }
