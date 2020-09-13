module SFConfigManager.Core.Common

open FSharpPlus
open SFConfigManager.Extensions.MaybeComputationExpression
open SFConfigManager.Data
open SFConfigManager.Data.Parsers.ParserTypes
open System.Xml.Linq

exception InvalidFileException of name: string
exception FileNotFoundException of name: string

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
    maybe {
        let value = getParamValue param
        let name = getParamName param
        let! (sn, pn) = extract name

        return
            { ServiceName = sn
              ParamName = pn
              ParamValue = value }
    }

let inline (!?) name = XName.Get name

let mapFirst fn (a, b) = (fn a, b)
let mapSecond fn (a, b) = (a, fn b)

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

let getParamNamePrefix service section =
    let normalized = normalizeParamNameWithService service section ""
    normalized + "_"

let normalizeParamName section name = [ section; name ] |> joinParamName

let getParamValueFromList name section service (parameters: ParameterResultEntry list) =
    let paramMatcher (p: ParameterResultEntry) =
        p.ServiceName = service
        && p.ParamName = normalizeParamName (Option.defaultValue "" section) name

    parameters |> List.tryFind paramMatcher

let protectAndRun body = Result.protect body ()