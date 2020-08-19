module SFConfigManager.Core.Parsers.ParameterParser

open SFConfigManager.Data
open FSharpPlus
open System.IO
open SFConfigManager.Core
open System.Xml.Linq


type ParametersParseResult = { Params: Common.ParameterResultEntry list; FileName: string; RootElement: FabricTypes.Application }

let private extractParams fileName (root: FabricTypes.Application) =
    let p = root.Parameters
            |> Seq.map (Common.Parameters.P1 >> Common.mapParam)
            |> Seq.choose id
            |> Seq.toList
    Ok { Params = p; FileName = fileName; RootElement = root }

let private getParameters fileName (x: FabricTypes.Choice) =
    match x.Application with
    | None -> Error Common.InvalidFileException
    | Some root -> extractParams fileName root
        

let parseParameters (path: string) =
    try
        let fileName = Path.GetFileNameWithoutExtension path
        path |> File.ReadAllText |> FabricTypes.Parse |> getParameters fileName
    with e -> Error e
