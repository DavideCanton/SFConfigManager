module SFConfigManager.Main.CommandLineHandlers.AddHandler

open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Main.Utils
open SFConfigManager.Core.Editors.AddParameterEditor

let add (a: ParseResults<AddArgs>) (root: ParseResults<SfConfigArgs>) =
    let p = a.GetResult(AddArgs.Parameter)

    let name = p.GetResult(AddParameterArgs.Name)

    let tokenName = p.GetResult(AddParameterArgs.TokenName)

    let value = p.GetResult(AddParameterArgs.Value)

    let section = p.GetResult(AddParameterArgs.Section)

    let service = p.GetResult(AddParameterArgs.Service)

    let path = getSolutionPath root

    let editor c =
        addParameterEditor c section name tokenName value

    buildContextAndExecute path service editor
