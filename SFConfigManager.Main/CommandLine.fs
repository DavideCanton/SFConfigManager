module SFConfigManager.Main.CommandLine

open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Main.Utils
open SFConfigManager.Core.Context
open SFConfigManager.Core.Common
open SFConfigManager.Core.Editors
open SFConfigManager.Extensions.ResultComputationExpression
open FSharpPlus

let processCommand command arguments = command arguments

let get (g: ParseResults<GetArgs>) (root: ParseResults<SfConfigArgs>) =
    let name =
        g.GetResult(GetArgs.Name, defaultValue = "")

    let section = g.GetResult(GetArgs.Section)

    let service =
        g.GetResult(GetArgs.Service, defaultValue = "")

    let path = getSolutionPath root

    let paramPrinter fileName parameters =
        let value =
            getParamValueFromList name section service parameters

        match value with
        | Some v -> printfn "%s: %s" fileName v.ParamValue
        | None -> printfn "%s: not found" fileName

    let innerBody context =
        resultExpr {
            let section' = Option.defaultValue "" section
            printfn "Value of [Service=%s; Section=%s; Name=%s]" service section' name
            context.Parameters
            |> List.iter (fun x -> paramPrinter x.FileName x.Params)
            paramPrinter "Default Value" context.Manifest.Parameters
        }

    buildContextAndExecute path service innerBody

let add (a: ParseResults<AddArgs>) (root: ParseResults<SfConfigArgs>) = Ok()

let set (g: ParseResults<SetArgs>) (root: ParseResults<SfConfigArgs>) =
    let name =
        g.GetResult(SetArgs.Name, defaultValue = "")

    let value =
        g.GetResult(SetArgs.Value, defaultValue = "")

    let section = g.GetResult(SetArgs.Section)

    let service =
        g.GetResult(SetArgs.Service, defaultValue = "")

    let environments =
        g.GetResult(SetArgs.Environments, defaultValue = [])
        |> Set.ofList

    let path = getSolutionPath root

    buildContextAndExecute path service
    <| fun c -> SetParameterValueEditor.setParamValueEditor c service section name value environments

let setDefault (g: ParseResults<SetDefaultArgs>) (root: ParseResults<SfConfigArgs>) =
    let name =
        g.GetResult(SetDefaultArgs.Name, defaultValue = "")

    let value =
        g.GetResult(SetDefaultArgs.Value, defaultValue = "")

    let section = g.GetResult(SetDefaultArgs.Section)

    let service =
        g.GetResult(SetDefaultArgs.Service, defaultValue = "")

    let path = getSolutionPath root    

    buildContextAndExecute path service <| fun c -> SetParameterDefaultValueEditor.setParameterDefaultValueEditor c service section name value
