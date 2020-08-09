namespace SFConfigManager.Main

open SFConfigManager.Core
open System.IO

exception ProjectNotFoundException

module Main =

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
        let parsed = ParameterParser.parseParameters path
        printfn "File: %s" (Path.GetFileName path)
        match parsed with
        | Ok result ->
            result.Params |> List.iter (printfn "%A")
        | Error e -> printfn "Error: %s" e.Message
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
        |> fun x -> handleParsedSfProj x; x
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
