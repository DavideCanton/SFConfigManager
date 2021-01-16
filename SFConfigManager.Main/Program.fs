module SFConfigManager.Main.Program

open Argu
open SFConfigManager.Main.Arguments
open FSharpPlus
open SFConfigManager.Core.Common
open SFConfigManager.Extensions.ResultExtensions
open SFConfigManager.Main.CommandLine
open SFConfigManager.Main.CommandLineHandlers.AddHandler
open SFConfigManager.Main.CommandLineHandlers.GetHandler
open SFConfigManager.Main.CommandLineHandlers.SetDefaultHandler
open SFConfigManager.Main.CommandLineHandlers.SetHandler

let version = "1.0.0"

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
    let fn () =
        let parser =
            ArgumentParser.Create<SfConfigArgs>(programName = "sfconfig.exe", errorHandler = ProcessExiter())

        let result = parser.ParseCommandLine argv

        if (result.Contains Version) then
            printfn "%s" version
            Ok()
        else
            mainBody result

    let result = protectAndRun fn |> Result.flatten

    let exitWithError message =
        eprintfn "%s" message
        exit 1

    match result with
    | Ok _ -> 0
    | Error NoSolutionFoundException -> exitWithError "No .sln found"
    | Error MultipleSolutionsFoundException ->
        exitWithError "Multiple .sln found, please specify which to use with the -s parameter"
    | Error ProjectNotFoundException -> exitWithError "No .sfproj found"
    | Error e -> exitWithError e.Message