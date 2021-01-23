module SFConfigManager.Main.CommandLineHandlers.SetDefaultHandler

open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Main.Utils
open SFConfigManager.Core.Editors

let setDefault (g: ParseResults<SetDefaultArgs>) (root: ParseResults<SfConfigArgs>) =
    let name = g.GetResult(SetDefaultArgs.Name)

    let value = g.GetResult(SetDefaultArgs.Value)

    let section = g.GetResult(SetDefaultArgs.Section)

    let service = g.GetResult(SetDefaultArgs.Service)

    let path = getSolutionPath root

    let editor c =
        SetParameterDefaultValueEditor.setParameterDefaultValueEditor c section name value

    buildContextAndExecute path service editor
