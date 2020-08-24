module SFConfigManager.Core.Context

open FSharpPlus
open SFConfigManager.Data.Parsers.ParserTypes

type Context =
    { SfProj: SFProjParseResult
      Settings: SettingsParseResult option
      Parameters: ParametersParseResult list
      Manifest: ManifestParseResult }

exception IncompleteContextBuildRequestException

module ContextBuilder =
    type ContextBuilderImpl =
        { SfProj: SFProjParseResult option
          Settings: SettingsParseResult option
          Parameters: ParametersParseResult list option
          Manifest: ManifestParseResult option }

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

        let existsNone = List.exists Option.isNone

        match existsNone vals with
        | true -> Error IncompleteContextBuildRequestException
        | false ->
            Ok
                { SfProj = ctx.SfProj.Value
                  Settings = ctx.Settings
                  Parameters = ctx.Parameters.Value
                  Manifest = ctx.Manifest.Value }
