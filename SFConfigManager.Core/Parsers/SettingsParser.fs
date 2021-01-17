module SFConfigManager.Core.Parsers.SettingsParser

open System.IO
open FSharpPlus
open SFConfigManager.Data
open SFConfigManager.Data.Parsers.ParserTypes
open SFConfigManager.Core.Common

let private extractEntry (section: FabricTypes.Section2) =
    let name = section.Name

    let p =
        section.Parameters
        |> Seq.map (fun p -> (p.Name, p.Value))
        |> Seq.toList

    (name, p)

let private extractSettings (settings: FabricTypes.Settings2) settingsPath (manifest: FabricTypes.ServiceManifest) serviceManifestPath =
    let sections =
        settings.Sections
        |> Seq.map extractEntry
        |> Map.ofSeq

    { Sections = sections
      ServiceFilePath = serviceManifestPath
      Service = manifest.ServiceTypes.StatelessServiceTypes.[0].ServiceTypeName
      ServicePkgName = manifest.Name
      SettingsFilePath = settingsPath
      RootServiceElement = manifest
      RootSettingsElement = settings }

let private buildResult (settings: FabricTypes.Choice) settingsPath (serviceManifest: FabricTypes.Choice) serviceManifestPath =
    match (settings.Settings, serviceManifest.ServiceManifest) with
    | (Some settingsRoot, Some manifestRoot) -> Ok <| extractSettings settingsRoot settingsPath manifestRoot serviceManifestPath
    | (Some _, None) -> Error <| InvalidFileException serviceManifestPath
    | _ -> Error <| InvalidFileException settingsPath


let parseSettings path =
    let settingsFile =
        Path.Combine("PackageRoot", "Config", "Settings.xml")

    let serviceManifestFile =
        Path.Combine("PackageRoot", "ServiceManifest.xml")

    let appendPath = flip <| curry Path.Combine
    try
        let settingsPath = appendPath settingsFile path
        let settings =
            settingsPath
            |> File.ReadAllText
            |> FabricTypes.Parse

        let serviceManifestPath = appendPath serviceManifestFile path
        let serviceManifest =
            serviceManifestPath
            |> File.ReadAllText
            |> FabricTypes.Parse


        buildResult settings settingsPath serviceManifest serviceManifestPath
    with e -> Error e
