module SFConfigManager.Main.Arguments

open Argu

type AddSectionArgs =
    | Name of string
    | ServiceName of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Section name"
            | ServiceName _ -> "Service name"


type AddParameterArgs =
    | Name of string
    | SectionName of string
    | ServiceName of string
    | Value of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Parameter name"
            | SectionName _ -> "Parameter section name"
            | Value _ -> "Parameter value"
            | ServiceName _ -> "Service name"

type AddArgs =
    | [<CliPrefix(CliPrefix.None)>] Section of ParseResults<AddSectionArgs>
    | [<CliPrefix(CliPrefix.None)>] Parameter of ParseResults<AddParameterArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Section _ -> "Adds a section"
            | Parameter _ -> "Adds a parameter to an existing section"

type GetArgs =
    | Name of string
    | SectionName of string
    | ServiceName of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Parameter name"
            | SectionName _ -> "Parameter section name"
            | ServiceName _ -> "Service name"

type SfConfigArgs =
    | Version
    | [<AltCommandLine("-s")>] Sln of path:string option
    | [<CliPrefix(CliPrefix.None)>] Add of ParseResults<AddArgs>
    | [<CliPrefix(CliPrefix.None)>] Get of ParseResults<GetArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Version -> "Prints version"
            | Sln _ -> "Solution path"
            | Add _ -> "Adds"
            | Get _ -> "Gets"