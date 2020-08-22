module SFConfigManager.Core.Common

open FSharpPlus
open SFConfigManager.Data
open System.Xml.Linq

exception InvalidFileException of name:string
exception FileNotFoundException of name:string

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

let (|Param|) p =
    match p with
    | P1 p -> (p.Name, p.Value)
    | P2 p -> (p.Name, p.DefaultValue)

let private getParamName (Param (name, _)) = name
let private getParamValue (Param (_, value)) = value

let mapParam (param: Parameters): ParameterResultEntry option =
    let value = getParamValue param

    getParamName param
    |> extract
    |> Option.map (fun (sn, pn) ->
        { ServiceName = sn
          ParamName = pn
          ParamValue = value })

let inline (!?) name = XName.Get name

let mapFirst fn (a, b) = (fn a, b)
let mapSecond fn (a, b) = (a, fn b)
