module SFConfigManager.Core.Common

open FSharpPlus
open SFConfigManager.Core.Context
open SFConfigManager.Data
open SFConfigManager.Data.Parsers.ParserTypes
open System.Xml.Linq
open SFConfigManager.Extensions.ResultComputationExpression

exception InvalidFileException of name: string
exception FileNotFoundException of name: string
exception ParamNotFoundException of name: string
exception IllegalStateException of message: string

type Parameters =
    | P1 of FabricTypes.Parameter
    | P2 of FabricTypes.Parameter2

let (|Param|) p =
    match p with
    | P1 p -> (p.Name, p.Value)
    | P2 p -> (p.Name, p.DefaultValue)

let private getParamName (Param (name, _)) = name
let private getParamValue (Param (_, value)) = value

let mapParam (param: Parameters) : ParameterResultEntry =
    { ParamName = getParamName param
      ParamValue = getParamValue param }

let inline (!?) name = XName.Get name

let mapFirst fn (a, b) = (fn a, b)
let mapSecond fn (a, b) = (a, fn b)

let getParamValueFromList name (parameters: ParameterResultEntry list) =
    let paramMatcher (p: ParameterResultEntry) = p.ParamName = name
    parameters |> List.tryFind paramMatcher

let normalizeParamNameWithService (context: Context) section name =
    resultExpr {
        let! pkgName =
            context.Settings
            |> Option.map (fun (s: SettingsParseResult) -> s.ServicePkgName)
            |> Option.toResultWith (IllegalStateException "Settings not found")

        let key: ManifestSectionKey =
            { ServicePkgName = pkgName
              Section = section
              ParamName = name }

        let r =
            context.Manifest.Sections
            |> Map.tryFind key
            |> Option.map (String.trim [ '['; ']' ])
            |> Option.toResultWith (ParamNotFoundException(sprintf "%s %s" section name))

        return! r
    }

let protectAndRun body = Result.protect body ()
