namespace SFConfigManager.Main

open SFConfigManager.Core

module Main =

    [<EntryPoint>]
    let main argv =
        let path = argv.[0]
        let projs = SolutionParser.getSfProjs path
        let proj = projs.[0]

        let sfProjParser = new SFProjParser.SFProjParser()
        sfProjParser.parse proj
        
        printfn "Services: %A" sfProjParser.Services
        printfn "Params: %A" sfProjParser.Parameters
        printfn "Manifest Path: %s" sfProjParser.ManifestPath

        0