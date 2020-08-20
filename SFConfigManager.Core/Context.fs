module SFConfigManager.Core.Context

open SFConfigManager.Core.Parsers
open FSharpPlus

type Context =
    { SfProj: SFProjParser.SFProjParseResult
      Settings: SettingsParser.SettingsParseResult option
      Parameters: ParameterParser.ParametersParseResult list
      Manifest: ManifestParser.ManifestParseResult }

exception IncompleteContextBuildRequestException

module ContextBuilder =
    type ContextBuilderImpl =
        { SfProj: SFProjParser.SFProjParseResult option
          Settings: SettingsParser.SettingsParseResult option
          Parameters: ParameterParser.ParametersParseResult list option
          Manifest: ManifestParser.ManifestParseResult option }

    let newContext () =
        { SfProj = None
          Settings = None
          Parameters = None
          Manifest = None }

    let withSfProj (ctx: ContextBuilderImpl) sfProj = { ctx with SfProj = Some sfProj }
    let withSettings (ctx: ContextBuilderImpl) settings = { ctx with Settings = settings }

    let withParameters (ctx: ContextBuilderImpl) parameters =
        { ctx with
              Parameters = Some parameters }

    let withManifest (ctx: ContextBuilderImpl) manifest = { ctx with Manifest = Some manifest }

    let build ctx: Result<Context, exn> =
        let vals =
            [ ctx.SfProj |> Option.map ignore
              ctx.Parameters |> Option.map ignore
              ctx.Manifest |> Option.map ignore ]

        if List.exists Option.isNone vals then
            Error IncompleteContextBuildRequestException
        else
            Ok
                { SfProj = ctx.SfProj.Value
                  Settings = ctx.Settings
                  Parameters = ctx.Parameters.Value
                  Manifest = ctx.Manifest.Value }
