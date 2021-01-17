module SFConfigManager.Core.Parsers.ManifestParser

open System.IO
open SFConfigManager.Data
open SFConfigManager.Data.Parsers.ParserTypes
open SFConfigManager.Core.Common
open FSharpPlus
open SFConfigManager.Extensions.MaybeComputationExpression

let private buildParameters (root: FabricTypes.ApplicationManifest) =
    maybe {
        let! parameters = root.Parameters
        return parameters.Parameters |> Seq.ofArray
    }
    |> Option.orElseWith (fun () -> Some Seq.empty)
    |> Option.get
    |> Seq.map (P2 >> mapParam)
    |> Seq.toList

let private buildSectionItem (root: FabricTypes.ServiceManifestImport): (ManifestSectionKey * string) list =
    let pkgName =
        root.ServiceManifestRef.ServiceManifestName

    let buildItem (c: FabricTypes.Section): (ManifestSectionKey * string) seq =
        c.Parameters
        |> Seq.map
            (fun x ->
                ({ ServicePkgName = pkgName
                   Section = c.Name
                   ParamName = x.Name },
                 x.Value))

    let buildItemsFromConfig (c: FabricTypes.ConfigOverrides) =
        c.ConfigOverrides
        |> Seq.collect
            (fun x ->
                x.Settings
                |> Option.map (fun y -> y.Sections |> Seq.collect buildItem)
                |> Option.orElse (Some Seq.empty)
                |> Option.get)

    root.ConfigOverrides
    |> Option.map buildItemsFromConfig
    |> Option.orElse (Some Seq.empty)
    |> Option.get
    |> Seq.toList

let private buildSections (root: FabricTypes.ApplicationManifest) =
    root.ServiceManifestImports
    |> Seq.collect buildSectionItem
    |> Map.ofSeq

let private parseManifestData path (root: FabricTypes.ApplicationManifest) =
    { Parameters = buildParameters root
      Sections = buildSections root
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
