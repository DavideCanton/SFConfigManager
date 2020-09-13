module SFConfigManager.Core.Parsers.ParameterParser

open SFConfigManager.Data
open SFConfigManager.Data.Parsers.ParserTypes
open FSharpPlus
open System.IO
open SFConfigManager.Core
open SFConfigManager.Core.Common

let private extractParams fileName path (root: FabricTypes.Application) =
    let p =
        root.Parameters
        |> Seq.map (Common.Parameters.P1 >> Common.mapParam)
        |> Seq.choose id
        |> Seq.toList

    Ok
        { Params = p
          FileName = fileName
          FilePath = path
          RootElement = root }

let private getParameters fileName path (x: FabricTypes.Choice) =
    match x.Application with
    | None -> Error <| Common.InvalidFileException path
    | Some root -> extractParams fileName path root


let parseParameters (path: string) =
    let body () =
        let fileName = Path.GetFileNameWithoutExtension path

        path
        |> File.ReadAllText
        |> FabricTypes.Parse
        |> getParameters fileName path

    protectAndRun body
