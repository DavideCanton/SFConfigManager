module SFConfigManager.Core.ParameterParser

open SFConfigManager.Data
open FSharpPlus
open Common
open System.IO

type ParameterResultEntry =
    { ServiceName: string
      ParamName: string
      ParamValue: string }

type ParametersParseResult = { Params: ParameterResultEntry list }

let private splitTwo (sep: string) (value: string) =
    let list = String.split [ sep ] value |> Seq.toList
    match list with
    | [] -> None
    | (sn :: pn) -> Some(sn, String.concat "_" pn)

let private extract (p: string) = splitTwo "_" p

let private mapParam (param: FabricTypes.Parameter) =
    let value = param.Value

    param.Name
        |> extract
        |> Option.map (fun (sn, pn) ->
            { ServiceName = sn
              ParamName = pn
              ParamValue = value })

let private extractParams (root: FabricTypes.Application) =
    let p = root.Parameters
            |> Seq.map mapParam
            |> Seq.choose id
            |> Seq.toList
    Ok { Params = p }

let private getParameters (x: FabricTypes.Choice) =
    match x.Application with
    | None -> Error InvalidFileException
    | Some root -> extractParams root
        

let parseParameters (path: string) =
    try
        path |> File.ReadAllText |> FabricTypes.Parse |> getParameters
    with e -> Error e
