module SFConfigManager.Main.CommandLineHandlers.SetDefaultHandler

open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Main.Utils
open SFConfigManager.Core.Editors

let setDefault (g: ParseResults<SetDefaultArgs>) (root: ParseResults<SfConfigArgs>) =
    let name =
        g.GetResult(SetDefaultArgs.Name, defaultValue = "")

    let value =
        g.GetResult(SetDefaultArgs.Value, defaultValue = "")

    let section = g.GetResult(SetDefaultArgs.Section, defaultValue = "")

    let service =
        g.GetResult(SetDefaultArgs.Service, defaultValue = "")

    let path = getSolutionPath root

    let editor c =
        SetParameterDefaultValueEditor.setParameterDefaultValueEditor c section name value

    buildContextAndExecute path service editor
