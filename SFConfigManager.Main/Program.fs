module SFConfigManager.Main.Program

open Argu
open SFConfigManager.Main.Arguments
open FSharpPlus
open SFConfigManager.Extensions.ResultExtensions
open SFConfigManager.Main.CommandLine


let mainBody (arguments: ParseResults<SfConfigArgs>) =
    match arguments.GetSubCommand() with
    | Add r -> processCommand add r arguments
    | Get r -> processCommand get r arguments
    | Set r -> processCommand set r arguments
    | SetDefault r -> processCommand setDefault r arguments
    // ignore after
    | Sln _ -> Ok()
    | Version -> Ok()


[<EntryPoint>]
let main argv =
    let fn() =
        let parser = ArgumentParser.Create<SfConfigArgs>(programName = "sfconfig.exe", errorHandler = ProcessExiter())
        let result = parser.ParseCommandLine argv
        match result.Contains Version with
        | true ->
            printfn "1.0.0"
            Ok()
        | false -> mainBody result

    let result = () |> (Result.protect fn) |> Result.flatten

    match result with
    | Ok _ -> 0
    | Error e ->
        eprintfn "Error: %s" e.Message
        1
