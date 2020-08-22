module SFConfigManager.Main.Utils

open SFConfigManager.Core.Parsers
open FSharpPlus
open SFConfigManager.Core
open System.IO
open SFConfigManager.Core.Context
open Argu
open SFConfigManager.Main.Arguments

let getSfProj path =
    let projs = SolutionParser.parseSolution path
    match projs with
    | Ok ({ SfProjList = (s :: _) }) -> SFProjParser.parseSFProj s
    | _ -> Error ProjectNotFoundException

let parseParameters (parameters: SFProjParser.SFProjParseResult) =
    parameters.Parameters
    |> List.map ParameterParser.parseParameters
    |> Result.partition
    |> fun (oks, errors) ->
        match errors with
        | [] -> Ok oks
        | e::_ -> Error e

let private joinParamName parts =
    parts
    |> String.concat "_"
    |> String.trim (List.singleton '_')

let normalizeParamNameWithService service section name =
    let addSection (section: string option) (a, b) =
        match section with
        | Some x -> [ a; x; b ]
        | None -> [ a; b ]

    (service, name)
    |> addSection section
    |> joinParamName

let normalizeParamName section name = [ section; name ] |> joinParamName

let getParamValue name section service (parameters: Common.ParameterResultEntry list) =
    let paramMatcher (p: Common.ParameterResultEntry) =
        p.ServiceName = service
        && p.ParamName = normalizeParamName (Option.defaultValue "" section) name

    parameters |> List.tryFind paramMatcher

let getDefaultSolutionPath () =
    let files = Directory.GetFiles(".", "*.sln")
    match Array.length files with
    | 1 -> Ok files.[0]
    | 0 -> Error NoSolutionFoundException
    | _ -> Error MultipleSolutionsFoundException

let throwIfError (r: Result<'a, exn>) =
    match r with
    | Ok v -> v
    | Error e -> raise e

let parseSettings (sfProjPath: string) services =

    let normalizeAndAppendPath p =
        Path.Combine(Path.GetDirectoryName sfProjPath, p)
        |> Path.GetFullPath
        |> Path.GetDirectoryName

    services
    |> List.map
        (normalizeAndAppendPath
         >> SettingsParser.parseSettings)
    |> Result.partition
    |> fun (oks, errors) ->
        match errors with
        | [] -> Ok oks
        | e::_ -> Error e

let buildContext path service =
    ContextBuilder.newContext ()
    |> Ok
    |> Result.bind (fun ctx ->
        getSfProj path
        |> Result.map (ContextBuilder.withSfProj ctx))
    |> Result.bind (fun ctx ->
        parseParameters ctx.SfProj.Value
        |> Result.map (ContextBuilder.withParameters ctx))
    |> Result.bind (fun ctx ->
        ManifestParser.parseManifest ctx.SfProj.Value.ManifestPath
        |> Result.map (ContextBuilder.withManifest ctx))
    |> Result.bind (fun ctx ->
        parseSettings ctx.SfProj.Value.FilePath ctx.SfProj.Value.Services
        |> Result.map (List.tryFind (fun x -> x.Service = service))
        |> Result.map (ContextBuilder.withSettings ctx))
    |> Result.bind ContextBuilder.build

let getSolutionPath (arguments: ParseResults<SfConfigArgs>) =
    arguments.TryGetResult Sln
    |> Option.defaultWith (getDefaultSolutionPath >> throwIfError)
