module SFConfigManager.Core.Parsers.SFProjParser

open System.IO
open Microsoft.Build.Construction
open FSharpPlus
open SFConfigManager.Extensions.StringExtensions
open SFConfigManager.Data.Parsers.ParserTypes

let private getItemInclude (x: ProjectItemElement) = x.Include

let private ofType itemType (x: ProjectItemElement) = x.ItemType = itemType

let private buildResult (sfProjPath: string) (document: ProjectRootElement) =
    let baseFolder = Path.GetDirectoryName sfProjPath

    let relativeToBase p =
        Path.Combine(baseFolder, p) |> Path.GetFullPath

    let parameters =
        document.Items
        |> Seq.filter (ofType "None")
        |> Seq.map getItemInclude
        |> Seq.filter (String.isSubstring "ApplicationParameters")
        |> Seq.map relativeToBase
        |> Seq.toList

    let manifestPath =
        document.Items
        |> Seq.filter (ofType "None")
        |> Seq.map getItemInclude
        |> Seq.find (String.isSubstring "ApplicationManifest")
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
