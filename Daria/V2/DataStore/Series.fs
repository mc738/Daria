namespace Daria.V2.DataStore

open Daria.V2.DataStore.Common
open Daria.V2.DataStore.Models

[<RequireQualifiedAccess>]
module Series =

    open Freql.Sqlite
    open Daria.V2.DataStore.Persistence

    module private Internal =

        /// <summary>
        /// An internal record representing a series version.
        /// This contains the minimum data required for internal operations.
        /// </summary>
        type SeriesVersionListingItem =
            { Id: string
              Version: int
              Active: bool
              Draft: bool }

        let fetchTopLevelSeries (ctx: SqliteContext) =
            Operations.selectSeriesRecords ctx [ "WHERE parent_series_id IS NULL" ] []

        let fetchSeriesByParent (ctx: SqliteContext) (parentSeriesId: string) =
            Operations.selectSeriesRecords ctx [ "WHERE parent_series_id = @0" ] [ parentSeriesId ]

        /// <summary>
        /// This used a bespoke query bypassing `Operations` because the version blob is not needed.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="seriesId"></param>
        /// <param name="activeStatus"></param>
        /// <param name="draftStatus"></param>
        let fetchSeriesVersions
            (ctx: SqliteContext)
            (seriesId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =
            let sql =
                [ "SELECT id, series_id, version, title, title_slug, description, hash, created_on, active, draft FROM series_versions"
                  "WHERE series_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> () ]
                |> toSql

            ctx.SelectAnon<SeriesVersionOverview>(sql, [ seriesId ])
            
        let fetchSeriesVersionListings
            (ctx: SqliteContext)
            (seriesId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =
            let sql =
                [ "SELECT id, version, active, draft FROM series_versions"
                  "WHERE series_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> () ]
                |> toSql

            ctx.SelectAnon<SeriesVersionOverview>(sql, [ seriesId ])
            
        let fetchLatestVersionListing
            (ctx: SqliteContext)
            (seriesId: string)
            (activeStatus: ActiveStatus)
            (draftStatus: DraftStatus)
            =
            
            let sql =
                [ "SELECT id, version, active, draft FROM series_versions"
                  "WHERE series_id = @0"
                  match activeStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  match draftStatus.ToSqlOption("AND ") with
                  | Some v -> v
                  | None -> ()
                  "ORDER BY version DESC"
                  "LIMIT 1" ]
                |> toSql

            ctx.SelectSingleAnon<SeriesVersionOverview>(sql, [ seriesId ])

    let rec fetchSeriesVersionOverviews (ctx: SqliteContext) (seriesId: string) = Internal.fetchSeriesVersions

    let list (ctx: SqliteContext) =
        let rec build (series: Records.Series list) =
            series
            |> List.map (fun s ->
                ({ Id = s.Id
                   Name = s.Name
                   Order = s.SeriesOrder
                   CreatedOn = s.CreatedOn
                   Active = s.Active
                   Children = Internal.fetchSeriesByParent ctx s.Id |> build
                   Versions = Internal.fetchSeriesVersions ctx s.Id ActiveStatus.All DraftStatus.All }
                : SeriesListingItem))

        Internal.fetchTopLevelSeries ctx |> build

    let fetchLatestVersion (ctx: SqliteContext) (seriesId: string) (includeDrafts: bool) =
        Operations.selectSeriesVersionRecord
            ctx
            [ "WHERE series_id = @0"
              match includeDrafts with
              | true -> ()
              | false -> "AND draft = 0"
              "ORDER BY version DESC"
              "LIMIT 1" ]
            [ seriesId ]

    
    let addNewDraftVersion (ctx: SqliteContext) ()
    
    let addOrReplaceDraftVersion (ctx: SqliteContext) (series: string) =

        ()
