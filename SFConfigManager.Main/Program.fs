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
    // ignore after
    | Sln _ -> Ok()
    | Version -> Ok()


[<EntryPoint>]
let main argv =
    let fn =
        Result.protect (fun () ->
            let parser =
                ArgumentParser.Create<SfConfigArgs>(programName = "sfconfig.exe", errorHandler = ProcessExiter())

            let result = parser.ParseCommandLine argv

            if result.Contains Version then
                printfn "1.0.0"
                Ok()
            else
                mainBody result)

    match Result.flatten (fn ()) with
    | Ok _ -> 0
    | Error e ->
        eprintfn "Error: %s" e.Message
        1
