module SFConfigManager.Main.Main

open SFConfigManager.Core
open System.IO
open Argu
open Arguments
open FSharpPlus

exception ProjectNotFoundException
exception NoSolutionFoundException
exception MultipleSolutionsFoundException

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
        printfn "Manifest Path: %s" result.ManifestPath.Value
    | Error e -> printfn "Error: %A" e

let parseParameters (parsed: Result<SFProjParser.SFProjParseResult, exn>) =
    let reducer = flip <| Result.map2 List.cons
    
    match parsed with
    | Ok { Parameters = parameters } -> 
        parameters
        |> List.map ParameterParser.parseParameters
        |> List.fold reducer (Ok [])
    | Error e -> Error e

let getParamValue name sectionName serviceName (parameters: ParameterParser.ParametersParseResult) =
    let paramMatcher (p: ParameterParser.ParameterResultEntry) =
        p.ServiceName = serviceName &&
        p.ParamName = (String.concat "_" [sectionName; name] |> String.trim (List.singleton '_'))

    parameters.Params
    |> List.tryFind paramMatcher
    

let mainBody path (arguments: ParseResults<SfConfigArgs>) =
    let sfProj = getSfProj path
    let parsedSfProj = SFProjParser.parseSFProj (Result.get sfProj)
    let parameters = parseParameters parsedSfProj |> Result.get
    // TODO parse settings
    // TODO parse manifest

    let isGet = arguments.TryGetResult Get
    match isGet with
    | Some g ->
        let name = g.GetResult (Name, defaultValue= "")
        let sectionName = g.GetResult (SectionName, defaultValue= "")
        let serviceName = g.GetResult (ServiceName, defaultValue= "")

        let paramPrinter (paramFile: ParameterParser.ParametersParseResult) =
            let value = getParamValue name sectionName serviceName paramFile
            match value with
            | Some v -> printfn "%s: %s" paramFile.FileName v.ParamValue
            | None -> printfn "%s: not found" paramFile.FileName

        printfn "Value of [Service=%s; Section=%s; Name=%s]" serviceName sectionName name
        List.iter paramPrinter parameters
        
    | _ -> ()


let getDefaultSolutionPath() =
    let files = Directory.GetFiles(".", "*.sln")
    match Array.length files with
    | 1 -> Ok files.[0]
    | 0 -> Error NoSolutionFoundException
    | _ -> Error MultipleSolutionsFoundException

let throwIfError r =
    match r with
    | Ok v -> v
    | Error e -> raise e

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<SfConfigArgs>(programName = "sfconfig.exe")

    try
        let result = parser.ParseCommandLine argv
        let path = result.GetResult Sln |> Option.defaultWith (getDefaultSolutionPath >> throwIfError)
        mainBody path result
        0
    with e ->
        eprintfn "Error: %s" e.Message
        1