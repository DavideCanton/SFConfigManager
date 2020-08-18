namespace SFConfigManager.Main.Arguments

open Argu

type AddSectionArgs =
    | [<ExactlyOnce; MainCommand>] Service of string
    | [<ExactlyOnce>] Name of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Section name"
            | Service _ -> "Service name"


type AddParameterArgs =
    | [<ExactlyOnce; MainCommand>] Service of string
    | [<ExactlyOnce>] Name of string
    | [<ExactlyOnce>] Section of string
    | [<ExactlyOnce>] Value of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Parameter name"
            | Section _ -> "Parameter section name"
            | Value _ -> "Parameter value"
            | Service _ -> "Service name"

type AddArgs =
    | [<CustomCommandLine("section")>] Section of ParseResults<AddSectionArgs>
    | [<CustomCommandLine("param")>] Parameter of ParseResults<AddParameterArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Section _ -> "Adds a section"
            | Parameter _ -> "Adds a parameter to an existing section"

type GetArgs =
    | [<ExactlyOnce; MainCommand>] Service of string
    | [<ExactlyOnce>] Name of string
    | [<ExactlyOnce>] Section of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Parameter name"
            | Section _ -> "Parameter section name"
            | Service _ -> "Service name"

type SfConfigArgs =
    | [<Unique; AltCommandLine("-V")>] Version
    | [<AltCommandLine("-s")>] Sln of path:string
    | [<CustomCommandLine("add")>] Add of ParseResults<AddArgs>
    | [<CustomCommandLine("get")>] Get of ParseResults<GetArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Version -> "Prints version"
            | Sln _ -> "Solution path"
            | Add _ -> "Adds"
            | Get _ -> "Gets"