module SFConfigManager.Main.Utils

open SFConfigManager.Core.Parsers
open FSharpPlus
open System.IO
open SFConfigManager.Core.Context
open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Data.Parsers.ParserTypes
open SFConfigManager.Extensions.ResultComputationExpression

let getSfProj path =
    let projs = SolutionParser.parseSolution path
    match projs with
    | Ok ({ SfProjList = (s :: _) }) -> SFProjParser.parseSFProj s
    | _ -> Error ProjectNotFoundException

let parseParameters (parameters: SFProjParseResult) =
    parameters.Parameters
    |> List.map ParameterParser.parseParameters
    |> Result.partition
    |> fun (oks, errors) ->
        match errors with
        | [] -> Ok oks
        | e :: _ -> Error e

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
        | e :: _ -> Error e

let private buildContext path service =
    resultExpr {
        let builder = ContextBuilder.newContext ()

        let! sfProjPath = getSfProj path

        let builder =
            ContextBuilder.withSfProj builder sfProjPath

        let! parameters = parseParameters builder.SfProj.Value

        let builder =
            ContextBuilder.withParameters builder parameters

        let! manifest = ManifestParser.parseManifest builder.SfProj.Value.ManifestPath

        let builder =
            ContextBuilder.withManifest builder manifest

        let! settings = parseSettings builder.SfProj.Value.FilePath builder.SfProj.Value.Services

        let serviceSettings =
            List.tryFind (fun x -> x.Service = service) settings

        let builder =
            ContextBuilder.withSettings builder serviceSettings

        return! (ContextBuilder.build builder)
    }


let getSolutionPath (arguments: ParseResults<SfConfigArgs>) =
    arguments.TryGetResult Sln
    |> Option.defaultWith (getDefaultSolutionPath >> throwIfError)


let buildContextAndExecute path service (fn: Context -> Result<unit, exn>) =
    resultExpr {
        let! ctx = buildContext path service
        do! fn ctx
    }
