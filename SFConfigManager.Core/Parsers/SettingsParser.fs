module SFConfigManager.Core.Parsers.SettingsParser

open System.IO
open FSharpPlus
open SFConfigManager.Data
open SFConfigManager.Core.Common

type SettingsParseResult =
    { 
        Service: string
        Sections: Map<string, (string * string) list> 
    }

let private extractEntry (section: FabricTypes.Section2) =
    let name = section.Name
    let p =
        section.Parameters
        |> Seq.map (fun p -> (p.Name, p.Value))
        |> Seq.toList
    (name, p)

let private extractSettings (settings: FabricTypes.Settings2) (manifest: FabricTypes.ServiceManifest) =
    let sections = 
        settings.Sections
        |> Seq.map extractEntry
        |> Map.ofSeq
    
    { 
        Sections = sections
        Service = manifest.ServiceTypes.StatelessServiceTypes.[0].ServiceTypeName
    }

let private buildResult (settings: FabricTypes.Choice) (serviceManifest: FabricTypes.Choice) =
    match (settings.Settings, serviceManifest.ServiceManifest) with
    | (Some settingsRoot, Some manifestRoot) -> Ok <| extractSettings settingsRoot manifestRoot
    | _ -> Error InvalidFileException


let parseSettings path =
    let settingsFile = Path.Combine("PackageRoot", "Config", "Settings.xml")
    let serviceManifestFile = Path.Combine("PackageRoot", "ServiceManifest.xml")
    let appendPath = flip <| curry Path.Combine
    try
        let settings = path            
                       |> appendPath settingsFile
                       |> File.ReadAllText
                       |> FabricTypes.Parse

        let serviceManifest = path            
                              |> appendPath serviceManifestFile
                              |> File.ReadAllText
                              |> FabricTypes.Parse


        buildResult settings serviceManifest
    with e -> Error e
