module SFConfigManager.Core.Parsers.ManifestParser

open System.IO
open SFConfigManager.Data
open SFConfigManager.Core.Common

type ManifestParseResult =
    { Parameters: ParameterResultEntry list
      ManifestPath: string
      RootElement: FabricTypes.ApplicationManifest }

let private parseManifestData path (root: FabricTypes.ApplicationManifest) =
    { Parameters =
          root.Parameters
          |> Option.map (fun x -> x.Parameters |> Seq.ofArray)
          |> Option.orElse (Some Seq.empty)
          |> Option.get
          |> Seq.map (Parameters.P2 >> mapParam)
          |> Seq.choose id
          |> Seq.toList
      ManifestPath = path
      RootElement = root }

let private tryParseManifestData path (root: FabricTypes.Choice) =
    match root.ApplicationManifest with
    | Some root -> Ok <| parseManifestData path root
    | None -> Error InvalidFileException

let parseManifest path =
    path
    |> File.ReadAllText
    |> FabricTypes.Parse
    |> tryParseManifestData path
