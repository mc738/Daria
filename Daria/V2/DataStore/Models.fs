namespace Daria.V2.DataStore

open System
open System.IO
open Daria.V2.DataStore.Common

module Models =

    open Daria.V2.DataStore.Persistence

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
          Version: string
          Title: string
          TitleSlug: string
          Description: string
          Hash: string
          CreatedOn: DateTime
          Active: bool
          Draft: bool }

    type NewSeriesVersion =
        { Id: IdType
          SeriesId: string
          Title: string
          TitleSlug: string option
          Description: string
          IndexBlob: Blob
          ImageVersionId: EntityVersion option
          CreatedOn: DateTime option }

    type ResourceVersionOverview = { Id: string }
