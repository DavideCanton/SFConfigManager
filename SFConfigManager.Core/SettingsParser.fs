module SFConfigManager.Core.SettingsParser

open System.IO
open FSharpPlus
open SFConfigManager.Data
open SFConfigManager.Core.Common

type SettingsParseResult =
    { Sections: Map<string, (string * string) list> }

let private extractEntry (section: FabricTypes.Section2) =
    let name = section.Name
    let p =
        section.Parameters
        |> Seq.map (fun p -> (p.Name, p.Value))
        |> Seq.toList
    (name, p)

let private extractSettings (root: FabricTypes.Settings2) =
    let sections = 
        root.Sections
        |> Seq.map extractEntry
        |> Map.ofSeq
    
    { Sections = sections }

let private buildResult (x: FabricTypes.Choice) =
    match x.Settings with
    | None -> Error InvalidFileException
    | Some root -> Ok <| extractSettings root


let parseSettings path =
    let settingsFile = Path.Combine("PackageRoot", "Config", "Settings.xml")
    let appendPath = flip <| curry Path.Combine
    try
        Path.GetDirectoryName path            
        |> appendPath settingsFile
        |> File.ReadAllText
        |> FabricTypes.Parse
        |> buildResult
    with e -> Error e
