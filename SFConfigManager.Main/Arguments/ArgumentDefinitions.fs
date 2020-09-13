namespace SFConfigManager.Main.Arguments

open Argu

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
    | [<CustomCommandLine("param")>] Parameter of ParseResults<AddParameterArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Parameter _ -> "Adds a parameter to a section"

type GetArgs =
    | [<ExactlyOnce; MainCommand>] Service of string
    | [<ExactlyOnce>] Name of string
    | Section of string option

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Parameter name"
            | Section _ -> "Parameter section name"
            | Service _ -> "Service name"

type SetArgs =
    | [<ExactlyOnce; MainCommand>] Service of string
    | [<ExactlyOnce>] Name of string
    | Section of string option
    | [<ExactlyOnce>] Value of string
    | Environments of string list

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Parameter name"
            | Section _ -> "Parameter section name"
            | Service _ -> "Service name"
            | Value _ -> "Parameter value"
            | Environments _ -> "Environments to update, if empty updates all"

type SetDefaultArgs =
    | [<ExactlyOnce; MainCommand>] Service of string
    | [<ExactlyOnce>] Name of string
    | Section of string option
    | [<ExactlyOnce>] Value of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Parameter name"
            | Section _ -> "Parameter section name"
            | Service _ -> "Service name"
            | Value _ -> "Parameter value"

type SfConfigArgs =
    | [<Unique; AltCommandLine("-V")>] Version
    | [<AltCommandLine("-s")>] Sln of path: string
    | [<CustomCommandLine("add")>] Add of ParseResults<AddArgs>
    | [<CustomCommandLine("get")>] Get of ParseResults<GetArgs>
    | [<CustomCommandLine("set")>] Set of ParseResults<SetArgs>
    | [<CustomCommandLine("set-default")>] SetDefault of ParseResults<SetDefaultArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Version -> "Prints version"
            | Sln _ -> "Solution path"
            | Add _ -> "Adds"
            | Get _ -> "Gets"
            | Set _ -> "Sets"
            | SetDefault _ -> "Set default"
