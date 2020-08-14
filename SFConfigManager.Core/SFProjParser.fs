module SFConfigManager.Core.SFProjParser

open System.IO
open SFConfigManager.Core.Common
open Microsoft.Build.Construction

type SFProjParseResult =
    { Parameters: string list
      ManifestPath: string option
      Services: string list }

let private getItemInclude (x: ProjectItemElement) = x.Include

let private ofType itemType (x: ProjectItemElement) = x.ItemType = itemType

let private buildResult baseFolder (document: ProjectRootElement) =
    let relativeToBase p =
        Path.Combine(baseFolder, p) |> Path.GetFullPath

    let parameters =
        document.Items
        |> Seq.filter (ofType "None")
        |> Seq.map getItemInclude
        |> Seq.filter (contains "ApplicationParameters")
        |> Seq.map relativeToBase
        |> Seq.toList

    let manifestPath =
        document.Items
        |> Seq.filter (ofType "None")
        |> Seq.map getItemInclude
        |> Seq.tryFind (contains "ApplicationManifest")
        |> Option.map relativeToBase

    let services =
        document.Items
        |> Seq.filter (ofType "ProjectReference")
        |> Seq.map (getItemInclude >> relativeToBase)
        |> Seq.toList

    { Parameters = parameters
      ManifestPath = manifestPath
      Services = services }


let parseSFProj (path: string) =
    try
        path
        |> ProjectRootElement.Open
        |> buildResult (Path.GetDirectoryName(path))
        |> Ok
    with e -> Error e
