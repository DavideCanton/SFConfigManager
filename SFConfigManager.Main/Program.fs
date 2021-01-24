module SFConfigManager.Main.Program

open Argu
open SFConfigManager.Main.Arguments
open FSharpPlus
open SFConfigManager.Core
open SFConfigManager.Core.Common
open SFConfigManager.Extensions.ResultExtensions
open SFConfigManager.Main.CommandLine
open SFConfigManager.Main.CommandLineHandlers.AddHandler
open SFConfigManager.Main.CommandLineHandlers.GetHandler
open SFConfigManager.Main.CommandLineHandlers.SetDefaultHandler
open SFConfigManager.Main.CommandLineHandlers.SetHandler
open log4net
open log4net.Layout
open log4net.Appender
open log4net.Repository.Hierarchy

let version = "1.0.0"

let logger = LogUtils.getLoggerFromString "Program"

let mainBody (arguments: ParseResults<SfConfigArgs>) =
    let subCommand = arguments.GetSubCommand()
    match subCommand with
    | Add r -> processCommand subCommand add r arguments
    | Get r -> processCommand subCommand get r arguments
    | Set r -> processCommand subCommand set r arguments
    | SetDefault r -> processCommand subCommand setDefault r arguments
    // ignore after
    | Sln _ -> Ok()
    | Version -> Ok()

let configureLogging () =
    let hierarchy = LogManager.GetRepository() :?>  Hierarchy
    let layout = PatternLayout()
    layout.ConversionPattern <- "%date [%thread] %-5level %logger [%property{NDC}] - %message%newline"
    layout.ActivateOptions()

    let appender = FileAppender()
    appender.File <- "log-file.txt"
    appender.AppendToFile <- true
    appender.ActivateOptions()

    hierarchy.Root.AddAppender(appender)
    hierarchy.Configured <- true
    ()


[<EntryPoint>]
let main argv =
    configureLogging ()
    LogUtils.debug logger "Starting program with args %A" argv

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

    let exitWithError (ex: exn) =
        eprintfn "%A" ex
        exit 1

    match result with
    | Ok _ -> 0
    | Error e -> exitWithError e