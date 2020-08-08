namespace SFConfigManager.Main

open SFConfigManager.Core

exception NoProjectFoundException

module Main =
    let getSfProj path =
        let projs = SolutionParser.getSfProjs path
        match projs with
        | Ok ({ SfProjList = (s :: _) }) -> Ok s
        | _ -> Error NoProjectFoundException

    let parseSfProj proj =
        let sfProjParser = SFProjParser.SFProjParser()
        sfProjParser.Parse proj

    let handle (parsed: Result<SFProjParser.SFProjParseResult, exn>) =
        match parsed with
        | Ok result ->
            printfn "Services: %A" result.Services
            printfn "Params: %A" result.Parameters
            printfn "Manifest Path: %s" result.ManifestPath
        | Error e -> printfn "Error: %s" e.Message

    let mainBody path =
        getSfProj path
        |> Result.bind parseSfProj
        |> handle

    [<EntryPoint>]
    let main argv =
        match argv |> Array.toList with
        | path :: _ ->
            mainBody path
            0
        | _ ->
            eprintfn "No arguments provided"
            1
