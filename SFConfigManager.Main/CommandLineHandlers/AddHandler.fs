module SFConfigManager.Main.CommandLineHandlers.AddHandler

open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Main.Utils
open SFConfigManager.Core.Editors.AddParameterEditor

let add (a: ParseResults<AddArgs>) (root: ParseResults<SfConfigArgs>) =
    let p = a.GetResult(AddArgs.Parameter)

    let name =
        p.GetResult(AddParameterArgs.Name, defaultValue = "")

    let value =
        p.GetResult(AddParameterArgs.Value, defaultValue = "")

    let section = p.GetResult(AddParameterArgs.Section)

    let service =
        p.GetResult(AddParameterArgs.Service, defaultValue = "")

    let path = getSolutionPath root

    let editor c =
        addParameterEditor c section name value

    buildContextAndExecute path service editor
