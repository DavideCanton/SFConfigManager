module SFConfigManager.Core.Parsers.ParameterParser

open SFConfigManager.Data
open FSharpPlus
open System.IO
open SFConfigManager.Core.Common

type ParameterResultEntry =
    { ServiceName: string
      ParamName: string
      ParamValue: string }

type ParametersParseResult = { Params: ParameterResultEntry list; FileName: string }

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

let private extractParams fileName (root: FabricTypes.Application) =
    let p = root.Parameters
            |> Seq.map mapParam
            |> Seq.choose id
            |> Seq.toList
    Ok { Params = p; FileName = fileName }

let private getParameters fileName (x: FabricTypes.Choice) =
    match x.Application with
    | None -> Error InvalidFileException
    | Some root -> extractParams fileName root
        

let parseParameters (path: string) =
    try
        let fileName = Path.GetFileNameWithoutExtension path
        path |> File.ReadAllText |> FabricTypes.Parse |> getParameters fileName
    with e -> Error e
