module SFConfigManager.Main.Program

open System.IO
open Argu
open SFConfigManager.Main.Arguments
open FSharpPlus
open SFConfigManager.Core
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

    let getParamValue name section service (parameters: Common.ParameterResultEntry list) =
        let paramMatcher (p: Common.ParameterResultEntry) =
            p.ServiceName = service &&
            p.ParamName = (String.concat "_" [section; name] |> String.trim (List.singleton '_'))

        parameters
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

    let parseSettings sfProjPath services =
        let reducer = flip <| Result.map2 List.cons
        let normalizeAndAppendPath p =
            Path.Combine (Path.GetDirectoryName sfProjPath, p) |> Path.GetFullPath |> Path.GetDirectoryName
        
        services
        |> List.map normalizeAndAppendPath
        |> List.map SettingsParser.parseSettings
        |> List.fold reducer (Ok [])

    let buildContext path service =
        ContextBuilder.newContext()
        |> Ok
        |> Result.bind (fun ctx -> getSfProj path |> Result.map (ContextBuilder.withSfProj ctx))
        |> Result.bind (fun ctx -> parseParameters ctx.sfProj.Value |> Result.map (ContextBuilder.withParameters ctx))
        |> Result.bind (fun ctx -> ManifestParser.parseManifest ctx.sfProj.Value.ManifestPath |> Result.map (ContextBuilder.withManifest ctx))
        |> Result.bind (fun ctx -> 
            parseSettings ctx.sfProj.Value.FilePath ctx.sfProj.Value.Services
            |> Result.map (List.tryFind (fun x -> x.Service = service))
            |> Result.map (ContextBuilder.withSettings ctx)
        )
        |> Result.bind ContextBuilder.build

    let getSolutionPath (arguments: ParseResults<SfConfigArgs>) =
        arguments.TryGetResult Sln 
        |> Option.defaultWith (getDefaultSolutionPath >> throwIfError)
   
module CommandLine =
    let processCommand command arguments = command arguments
    
open Utils
open CommandLine
open System.Xml.Linq

let mainBody (arguments: ParseResults<SfConfigArgs>) =

    let get (g: ParseResults<GetArgs>) (root: ParseResults<SfConfigArgs>) =
        let name = g.GetResult (Name, defaultValue= "")
        let section = g.GetResult (Section, defaultValue= "")
        let service = g.GetResult (Service, defaultValue= "")
        let path = getSolutionPath root

        match buildContext path service with
        | Ok context ->
            let paramPrinter fileName parameters =
                let value = getParamValue name section service parameters
                match value with
                | Some v -> printfn "%s: %s" fileName v.ParamValue
                | None -> printfn "%s: not found" fileName

            // write file try
            //using (File.Open("./prova.xml", FileMode.OpenOrCreate)) (fun stream ->
            //    let q = Array.last context.manifest.RootElement.Parameters.Value.Parameters
            //    let child = XElement(q.XElement)
            //    child.SetAttributeValue(XName.Get "Name", "GNIIII")
            //    child.SetAttributeValue(XName.Get "DefaultValue", "GNEEEEE")                
            //    q.XElement.AddAfterSelf(
            //        XText("\r\n\t"),
            //        child,
            //        XText("\r\n\t")
            //    )
            //    context.manifest.RootElement.XElement.Save(stream)
            //)
    
            printfn "Value of [Service=%s; Section=%s; Name=%s]" service section name
            context.parameters |> List.iter (fun x -> paramPrinter x.FileName x.Params)
            paramPrinter "Default Value" context.manifest.Parameters
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