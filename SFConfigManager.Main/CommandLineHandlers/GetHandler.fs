module SFConfigManager.Main.CommandLineHandlers.GetHandler

open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Main.Utils
open SFConfigManager.Core.Common
open SFConfigManager.Extensions.ResultComputationExpression
open SFConfigManager.Core.Context

let get (g: ParseResults<GetArgs>) (root: ParseResults<SfConfigArgs>) =
    let name = g.GetResult(GetArgs.Name)

    let section = g.GetResult(GetArgs.Section)

    let service = g.GetResult(GetArgs.Service)

    let path = getSolutionPath root

    let paramPrinter fileName paramNameForService parameters =
        let value =
            getParamValueFromList paramNameForService parameters

        match value with
        | Some v -> printfn "%s: %s" fileName v.ParamValue
        | None -> printfn "%s: not found" fileName

    let innerBody (context: Context) =
        resultExpr {
            let! paramNameForService = normalizeParamNameWithService context section name
            printfn "Value of [Service=%s; Section=%s; Name=%s]" service section name

            context.Parameters
            |> List.iter (fun x -> paramPrinter x.FileName paramNameForService x.Params)

            paramPrinter "Default Value" paramNameForService context.Manifest.Parameters
        }

    buildContextAndExecute path service innerBody
