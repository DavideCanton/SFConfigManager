module SFConfigManager.Core.Parsers.ManifestParser

open System.IO
open SFConfigManager.Data
open SFConfigManager.Core.Common
open FSharpPlus

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
    | None -> Error <| InvalidFileException path

let parseManifest path =
    let resFn = Result.protect (fun () ->
        path
        |> File.ReadAllText
        |> FabricTypes.Parse
        |> tryParseManifestData path)

    let mapError (ex: exn) =
        match ex with
        | :? System.IO.FileNotFoundException as e -> FileNotFoundException e.FileName
        | e -> e

    resFn () 
    |> Result.flatten
    |> Result.mapError mapError
