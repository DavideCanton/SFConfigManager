module SFConfigManager.Core.Context

open SFConfigManager.Core.Parsers
open FSharpPlus

type Context = 
    { sfProj: SFProjParser.SFProjParseResult
      settings: SettingsParser.SettingsParseResult
      parameters: ParameterParser.ParametersParseResult list
    }

exception IncompleteContextBuildRequest

module ContextBuilder =
    type ContextBuilderImpl = 
        { sfProj: SFProjParser.SFProjParseResult option
          settings: SettingsParser.SettingsParseResult option
          parameters: ParameterParser.ParametersParseResult list option
        }
    
    let newContext() = { sfProj = None; settings = None; parameters = None }

    let withSfProj sfProj (ctx: ContextBuilderImpl) = { ctx with sfProj = Some sfProj }
    let withSettings settings (ctx: ContextBuilderImpl) = { ctx with settings = Some settings }
    let withParameters parameters (ctx: ContextBuilderImpl) = { ctx with parameters = Some parameters }

    let build ctx: Result<Context, exn> =
        let vals = [
            ctx.sfProj |> Option.map ignore
            ctx.settings |> Option.map ignore
            ctx.parameters |> Option.map ignore
        ]

        if List.exists Option.isNone vals then
            Error IncompleteContextBuildRequest
        else 
            Ok { sfProj = ctx.sfProj.Value
                 settings = ctx.settings.Value
                 parameters = ctx.parameters.Value }