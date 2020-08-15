module SFConfigManager.Main.Main

open SFConfigManager.Core
open System.IO
open Argu

exception ProjectNotFoundException


type AddArgs =
    | Section of sectionName:string
    | Parameter of addParams:string * string * string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Section _ -> "Adds a section"
            | Parameter _ -> "Adds a parameter to an existing section"

and RemoveArgs =
    | Section of sectionName:string
    | Parameter of name:string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Section _ -> "Removes a section"
            | Parameter _ -> "Removes a parameter from an existing section"

and GetArgs =
    | Parameter of getParams:string * string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Parameter _ -> "Gets parameter value"

and SfConfigArgs =
    | Version
    | [<EqualsAssignment>] SolutionPath of path:string option
    | [<CliPrefix(CliPrefix.None)>] Add of ParseResults<AddArgs>
    | [<CliPrefix(CliPrefix.None)>] Get of ParseResults<GetArgs>
    | [<CliPrefix(CliPrefix.None)>] Remove of ParseResults<RemoveArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Version -> "Prints version"
            | SolutionPath _ -> "Solution path"
            | Add _ -> "Adds"
            | Get _ -> "Gets"
            | Remove _ -> "Removes"

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

let parseParameterFile (path: string) =
    let parsed = ParameterParser.parseParameters path
    printfn "File: %s" (Path.GetFileName path)
    match parsed with
    | Ok result -> result.Params |> List.iter (printfn "%A")
    | Error e -> printfn "Error: %A" e
    ()

let handleParameters (parsed: Result<SFProjParser.SFProjParseResult, exn>) =
    match parsed with
    | Ok result -> result.Parameters |> List.iter parseParameterFile
    | _ -> ()

let mainBody path =
    getSfProj path
    |> Result.bind SFProjParser.parseSFProj
    |> fun x ->
        handleParsedSfProj x
        x
    |> handleParameters

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<SfConfigArgs>(programName = "sfconfig.exe")

    try
        let result = parser.ParseCommandLine argv
        let path = result.GetResult SolutionPath
        mainBody path.Value
        0
    with _ ->
        printfn "%s" <| parser.PrintUsage()
        1
