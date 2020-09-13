module SFConfigManager.Core.Parsers.ManifestParser

open System.IO
open SFConfigManager.Data
open SFConfigManager.Data.Parsers.ParserTypes
open SFConfigManager.Core.Common
open FSharpPlus
open SFConfigManager.Extensions.MaybeComputationExpression

let private parseManifestData path (root: FabricTypes.ApplicationManifest) =
    { Parameters =
          maybe {
              let! parameters = root.Parameters
              return parameters.Parameters |> Seq.ofArray
          }
          |> Option.orElseWith (fun () -> Some Seq.empty)
          |> Option.get
          |> Seq.map (Parameters.P2 >> mapParam)
          |> Seq.choose id
          |> Seq.toList
      ManifestPath = path
      RootElement = root }

let private tryParseManifestData path (root: FabricTypes.Choice) =
    match root.ApplicationManifest with
    | Some root -> Ok <| parseManifestData path root
    | None -> Error <| InvalidFileException path

let parseManifest path =
    let body () =
        path
        |> File.ReadAllText
        |> FabricTypes.Parse
        |> tryParseManifestData path

    let mapError (ex: exn) =
        match ex with
        | :? System.IO.FileNotFoundException as e -> FileNotFoundException e.FileName
        | e -> e

    protectAndRun body
    |> Result.flatten
    |> Result.mapError mapError
