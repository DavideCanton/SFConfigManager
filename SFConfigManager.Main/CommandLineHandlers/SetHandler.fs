module SFConfigManager.Main.CommandLineHandlers.SetHandler

open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Main.Utils
open SFConfigManager.Core.Editors

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

    let editor c =
        SetParameterValueEditor.setParamValueEditor c service section name value environments

    buildContextAndExecute path service editor