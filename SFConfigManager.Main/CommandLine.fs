module SFConfigManager.Main.CommandLine

open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Main.Utils
open SFConfigManager.Core.Editors.XMLEditor
open SFConfigManager.Core.Context
open FSharpPlus
open SFConfigManager.Core.Parsers.ParameterParser

let processCommand command arguments = command arguments

let private buildContextAndExecute path service (fn: Context -> Result<unit, exn>) =
    buildContext path service
    |> Result.bind fn
    |> Result.map ignore

let get (g: ParseResults<GetArgs>) (root: ParseResults<SfConfigArgs>) =
    let name =
        g.GetResult(GetArgs.Name, defaultValue = "")

    let section = g.GetResult(GetArgs.Section)

    let service =
        g.GetResult(GetArgs.Service, defaultValue = "")

    let path = getSolutionPath root

    let paramPrinter fileName parameters =
        let value =
            getParamValue name section service parameters

        match value with
        | Some v -> printfn "%s: %s" fileName v.ParamValue
        | None -> printfn "%s: not found" fileName

    let innerBody context =
        printfn "Value of [Service=%s; Section=%s; Name=%s]" service (Option.defaultValue "" section) name
        context.Parameters
        |> List.iter (fun x -> paramPrinter x.FileName x.Params)
        paramPrinter "Default Value" context.Manifest.Parameters
        Ok()

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

    let filterEnvironments (envs: ParametersParseResult list) =
        envs
        |> List.filter (fun e ->
            environments.IsEmpty
            || environments.Contains e.FileName)

    let innerBody context =
        let paramName =
            normalizeParamNameWithService service section name

        let xpath =
            sprintf "/empty:Application/empty:Parameters/empty:Parameter[@Name=\"%s\"]" paramName

        let processEnvironment env =
            let actions =
                [ SetAttribute
                    { Path = xpath
                      Name = "Value"
                      Value = value } ]

            processActionsAndSave actions env.RootElement.XElement env.FilePath

        let reducer l r =
            match r with
            | Ok _ -> l
            | Error e -> e :: l

        context.Parameters
        |> filterEnvironments
        |> List.map processEnvironment
        |> List.fold reducer []
        |> fun l -> if List.isEmpty l then Ok() else Error(SetArgumentsFailedException l)

    buildContextAndExecute path service innerBody

let setDefault (g: ParseResults<SetDefaultArgs>) (root: ParseResults<SfConfigArgs>) =
    let name =
        g.GetResult(SetDefaultArgs.Name, defaultValue = "")

    let value =
        g.GetResult(SetDefaultArgs.Value, defaultValue = "")

    let section = g.GetResult(SetDefaultArgs.Section)

    let service =
        g.GetResult(SetDefaultArgs.Service, defaultValue = "")

    let path = getSolutionPath root

    let innerBody context =
        let manifest = context.Manifest

        let paramName =
            normalizeParamNameWithService service section name

        let xpath =
            sprintf "/empty:ApplicationManifest/empty:Parameters/empty:Parameter[@Name=\"%s\"]" paramName

        let actions =
            [ SetAttribute
                { Path = xpath
                  Name = "DefaultValue"
                  Value = value } ]

        processActionsAndSave actions manifest.RootElement.XElement manifest.ManifestPath

    buildContextAndExecute path service innerBody
