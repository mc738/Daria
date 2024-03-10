﻿namespace Daria.V2.DataStore

open System
open System.IO
open Daria.V2.DataStore.Common

module Models =

    open Daria.V2.DataStore.Persistence

    type ImageVersionDetails =
        { Id: string

        }

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
          Version: int
          DraftVersion: int option
          Title: string
          TitleSlug: string
          Description: string
          Hash: string
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
        
    and ArticleOverview =
        { Id: string
          Name: string
          SeriesId: string
          Order: int
          CreatedOn: DateTime
          Active: bool }

    and ArticleVersionOverview =
        { Id: string
          Version: int
          DraftVersion: int option
          Title: string
          TitleSlug: string
          Description: string
          Hash: string
          CreatedOn: DateTime
          PublishedOn: DateTime
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
          RawLink: string
          OverrideCss: string option
          CreatedOn: DateTime option
          PublishedOn: DateTime option
          Tags: string list
          Metadata: Map<string, string> }
    
    type ResourceVersionOverview = { Id: string }
