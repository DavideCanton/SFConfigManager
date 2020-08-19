module SFConfigManager.Core.Common

open FSharpPlus
open SFConfigManager.Data

exception InvalidFileException

let contains (query: string) (value: string) = value.Contains query

type ParameterResultEntry =
    { ServiceName: string
      ParamName: string
      ParamValue: string }

let private splitTwo (sep: string) (value: string) =
    let list = String.split [ sep ] value |> Seq.toList
    match list with
    | [] -> None
    | (sn :: pn) -> Some(sn, String.concat "_" pn)

let private extract (p: string) = splitTwo "_" p

type Parameters = 
    | P1 of FabricTypes.Parameter
    | P2 of FabricTypes.Parameter2

let private getParamName p =
    match p with
    | P1 p -> p.Name
    | P2 p -> p.Name

let private getParamValue p =
    match p with
    | P1 p -> p.Value
    | P2 p -> p.DefaultValue

let mapParam (param: Parameters): ParameterResultEntry option =
    let value = getParamValue param

    getParamName param
        |> extract
        |> Option.map (fun (sn, pn) ->
              { ServiceName = sn
                ParamName = pn
                ParamValue = value })