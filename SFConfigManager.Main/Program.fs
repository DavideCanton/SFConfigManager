module SFConfigManager.Main.Program

open System.IO
open Argu
open SFConfigManager.Main.Arguments
open FSharpPlus
open SFConfigManager.Core.Parsers
open SFConfigManager.Core.Context
open SFConfigManager.Extensions.OptionExtensions
open SFConfigManager.Extensions.ResultExtensions

exception ProjectNotFoundException
exception NoSolutionFoundException
exception MultipleSolutionsFoundException

module Utils =
    let getSfProj path =
        let projs = SolutionParser.parseSolution path
        match projs with
        | Ok ({ SfProjList = (s :: _) }) -> SFProjParser.parseSFProj s
        | _ -> Error ProjectNotFoundException

    let parseParameters (parameters: SFProjParser.SFProjParseResult) =
        let reducer = flip <| Result.map2 List.cons
    
        parameters.Parameters
        |> List.map ParameterParser.parseParameters
        |> List.fold reducer (Ok [])

    let getParamValue name section service (parameters: ParameterParser.ParametersParseResult) =
        let paramMatcher (p: ParameterParser.ParameterResultEntry) =
            p.ServiceName = service &&
            p.ParamName = (String.concat "_" [section; name] |> String.trim (List.singleton '_'))

        parameters.Params
        |> List.tryFind paramMatcher

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

    //let servicesIndexer csProjPaths services =
    //    let normalizeAndAppendPath p =
    //        Path.Combine (Path.GetDirectoryName slnPath, 
    //    services
    //    |> List.map 

    let buildContext path service =
        ContextBuilder.newContext()
        |> Ok
        |> Result.bind (fun ctx -> getSfProj path |> Result.map (flip ContextBuilder.withSfProj ctx))
        |> Result.bind (fun ctx -> parseParameters ctx.sfProj.Value |> Result.map (flip ContextBuilder.withParameters ctx))
        // TODO missing parse settings
        |> Result.bind (fun x -> ContextBuilder.build x)

    let getSolutionPath (arguments: ParseResults<SfConfigArgs>) =
        arguments.TryGetResult Sln 
        |> Option.defaultWith (getDefaultSolutionPath >> throwIfError)
   
module CommandLine =
    let processCommand command arguments = command arguments
    
open Utils
open CommandLine

let mainBody (arguments: ParseResults<SfConfigArgs>) =

    let get (g: ParseResults<GetArgs>) (root: ParseResults<SfConfigArgs>) =
        let name = g.GetResult (Name, defaultValue= "")
        let section = g.GetResult (Section, defaultValue= "")
        let service = g.GetResult (Service, defaultValue= "")
        let path = getSolutionPath root

        match buildContext path service with
        | Ok context ->
            let paramPrinter (paramFile: ParameterParser.ParametersParseResult) =
                let value = getParamValue name section service paramFile
                match value with
                | Some v -> printfn "%s: %s" paramFile.FileName v.ParamValue
                | None -> printfn "%s: not found" paramFile.FileName
    
            printfn "Value of [Service=%s; Section=%s; Name=%s]" service section name
            List.iter paramPrinter context.parameters
            Ok ()
        | Error e -> Error e
    
    let add (a: ParseResults<AddArgs>) (root: ParseResults<SfConfigArgs>) =
        Ok ()

    match arguments.GetSubCommand() with
    | Add r -> processCommand add r arguments
    | Get r -> processCommand get r arguments
    // ignore after
    | Sln _ -> Ok ()
    | Version -> Ok ()


[<EntryPoint>]
let main argv =
    let res = () |> Result.protect (fun () ->
        let parser = ArgumentParser.Create<SfConfigArgs>(programName = "sfconfig.exe", errorHandler = ProcessExiter())
        let result = parser.ParseCommandLine argv

        if result.Contains Version then
            printfn "1.0.0"
            Ok ()
        else
            mainBody result
    )

    match Result.flatten res with
    | Ok _ -> 0
    | Error e ->
        eprintfn "Error: %s" e.Message
        1