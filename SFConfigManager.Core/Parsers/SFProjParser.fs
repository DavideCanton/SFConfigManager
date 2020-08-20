module SFConfigManager.Core.Parsers.SFProjParser

open SFConfigManager.Core
open System.IO
open Microsoft.Build.Construction

type SFProjParseResult =
    { FilePath: string
      Parameters: string list
      ManifestPath: string
      Services: string list }

let private getItemInclude (x: ProjectItemElement) = x.Include

let private ofType itemType (x: ProjectItemElement) = x.ItemType = itemType

let private buildResult sfProjPath (document: ProjectRootElement) =
    let baseFolder = Path.GetDirectoryName sfProjPath

    let relativeToBase p =
        Path.Combine(baseFolder, p) |> Path.GetFullPath

    let parameters =
        document.Items
        |> Seq.filter (ofType "None")
        |> Seq.map getItemInclude
        |> Seq.filter (Common.contains "ApplicationParameters")
        |> Seq.map relativeToBase
        |> Seq.toList

    let manifestPath =
        document.Items
        |> Seq.filter (ofType "None")
        |> Seq.map getItemInclude
        |> Seq.find (Common.contains "ApplicationManifest")
        |> relativeToBase

    let services =
        document.Items
        |> Seq.filter (ofType "ProjectReference")
        |> Seq.map (getItemInclude >> relativeToBase)
        |> Seq.toList

    { FilePath = sfProjPath
      Parameters = parameters
      ManifestPath = manifestPath
      Services = services }


let parseSFProj (path: string) =
    try
        path
        |> ProjectRootElement.Open
        |> buildResult path
        |> Ok
    with e -> Error e
