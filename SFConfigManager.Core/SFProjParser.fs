module SFConfigManager.Core.SFProjParser

open System.IO
open System.Xml.Linq
open SFConfigManager.Core.Common

type SFProjParseResult =
    { Parameters: string list
      ManifestPath: string
      Services: string list }

let private buildResult baseFolder (document: XDocument) =
    let relativeToBase p =
        Path.Combine(baseFolder, p) |> Path.GetFullPath

    let xNameBuilder' =
        xNameBuilder (document.Root.GetDefaultNamespace())

    let parameters =
        xNameBuilder' "None"
        |> document.Descendants
        |> Seq.filter
            (getAttrValue "Include"
             >> contains "ApplicationParameters")
        |> Seq.map (getAttrValue "Include" >> relativeToBase)
        |> Seq.toList

    let node =
        xNameBuilder' "None"
        |> document.Descendants
        |> Seq.find
            (getAttrValue "Include"
             >> contains "ApplicationManifest")

    let manifestPath =
        node |> getAttrValue "Include" |> relativeToBase

    let services =
        xNameBuilder' "ProjectReference"
        |> document.Descendants
        |> Seq.map (getAttrValue "Include" >> relativeToBase)
        |> Seq.toList

    { Parameters = parameters
      ManifestPath = manifestPath
      Services = services }


let parseSFProj (path: string) =
    try
        path
        |> XDocument.Load
        |> buildResult (Path.GetDirectoryName(path))
        |> Ok
    with e -> Error e
