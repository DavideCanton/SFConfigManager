module SFConfigManager.Core.Context

open SFConfigManager.Core.Parsers
open FSharpPlus

type Context = 
    { sfProj: SFProjParser.SFProjParseResult
      settings: SettingsParser.SettingsParseResult option
      parameters: ParameterParser.ParametersParseResult list
      manifest: ManifestParser.ManifestParseResult
    }

exception IncompleteContextBuildRequest

module ContextBuilder =
    type ContextBuilderImpl = 
        { sfProj: SFProjParser.SFProjParseResult option
          settings: SettingsParser.SettingsParseResult option
          parameters: ParameterParser.ParametersParseResult list option
          manifest: ManifestParser.ManifestParseResult option
        }
    
    let newContext() = { sfProj = None; settings = None; parameters = None; manifest = None }

    let withSfProj (ctx: ContextBuilderImpl) sfProj = { ctx with sfProj = Some sfProj }
    let withSettings (ctx: ContextBuilderImpl) settings = { ctx with settings = settings }
    let withParameters (ctx: ContextBuilderImpl) parameters = { ctx with parameters = Some parameters }
    let withManifest (ctx: ContextBuilderImpl) manifest = { ctx with manifest = Some manifest }

    let build ctx: Result<Context, exn> =
        let vals = [
            ctx.sfProj |> Option.map ignore
            ctx.parameters |> Option.map ignore
            ctx.manifest |> Option.map ignore
        ]

        if List.exists Option.isNone vals then
            Error IncompleteContextBuildRequest
        else 
            Ok { sfProj = ctx.sfProj.Value
                 settings = ctx.settings
                 parameters = ctx.parameters.Value
                 manifest = ctx.manifest.Value
               }