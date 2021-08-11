module SFConfigManager.Main.Utils

open SFConfigManager.Core.Parsers
open FSharpPlus
open System.IO
open SFConfigManager.Core.Context
open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Data.Parsers.ParserTypes
open SFConfigManager.Data
open SFConfigManager.Extensions.ResultComputationExpression
open SFConfigManager.Extensions.MaybeComputationExpression

exception ServiceNotFoundException of string

let parseSfProj path =
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

    match files with
    | [| f |] -> Ok f
    | [||] -> Error NoSolutionFoundException
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
    |> List.map (
        normalizeAndAppendPath
        >> SettingsParser.parseSettings
    )
    |> Result.partition
    |> fun (oks, errors) ->
        match errors with
        | [] -> Ok oks
        | e :: _ -> Error e

let private buildContext path service =
    resultExpr {

        let! parsedSfProj = parseSfProj path
        let! parameters = parseParameters parsedSfProj
        let! manifest = ManifestParser.parseManifest parsedSfProj.ManifestPath
        let! settings = parseSettings parsedSfProj.FilePath parsedSfProj.Services

        let serviceTypeName =
            maybe {
                let! defaultServices = manifest.RootElement.DefaultServices
                let services = defaultServices.Services |> Seq.ofArray
                let! foundService = Seq.tryFind (fun (x: FabricTypes.Service) -> x.Name = service) services
                let! statelessService = foundService.StatelessService
                return statelessService.ServiceTypeName
            }

        let buildContext serviceTypeName =
            let serviceSettings =
                List.tryFind (fun x -> x.Service = serviceTypeName) settings

            ContextBuilder.newContext ()
            |> ContextBuilder.withSfProj parsedSfProj
            |> ContextBuilder.withParameters parameters
            |> ContextBuilder.withManifest manifest
            |> ContextBuilder.withSettings serviceSettings
            |> ContextBuilder.build

        let context =
            serviceTypeName
            |> Option.toResultWith (ServiceNotFoundException service)
            |> Result.bind buildContext

        return! context
    }


let getSolutionPath (arguments: ParseResults<SfConfigArgs>) =
    arguments.TryGetResult Sln
    |> Option.defaultWith (getDefaultSolutionPath >> throwIfError)


let buildContextAndExecute path service (fn: Context -> Result<unit, exn>) =
    resultExpr {
        let! ctx = buildContext path service
        return! fn ctx
    }
