namespace SFConfigManager.Main

open SFConfigManager.Core

exception ProjectNotFoundException

module Main =
    let inline (||>) (x: 'a) (f: 'a -> unit): 'a =
        f x; x


    let getSfProj path =
        let projs = SolutionParser.parseSolution path
        match projs with
        | Ok ({ SfProjList = (s :: _) }) -> Ok s
        | _ -> Error ProjectNotFoundException

    let handleParsedSfProj (parsed: Result<SFProjParser.SFProjParseResult, exn>) =
        match parsed with
        | Ok result ->
            printfn "Services: %A" result.Services
            printfn "Param Files: %A" result.Parameters
            printfn "Manifest Path: %s" result.ManifestPath
        | Error e -> printfn "Error: %s" e.Message

    let parseParameterFile (path: string) =
        let result = ParameterParser.parseParameters
        ()

    let handleParameters (parsed: Result<SFProjParser.SFProjParseResult, exn>) =
        match parsed with
        | Ok result ->
            result.Parameters
            |> List.iter parseParameterFile
        | _ -> ()

    let mainBody path =
        getSfProj path
        |> Result.bind SFProjParser.parseSFProj
        ||> handleParsedSfProj
        |> handleParameters

    [<EntryPoint>]
    let main argv =
        match argv |> Array.toList with
        | path :: _ ->
            mainBody path
            0
        | _ ->
            eprintfn "No arguments provided"
            1
