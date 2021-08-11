module SFConfigManager.Main.CommandLine

open SFConfigManager.Core
open SFConfigManager.Main.Arguments

let logger =
    LogUtils.getLoggerFromString "CommandLine"

let processCommand (command: SfConfigArgs) commandHandler arguments =
    LogUtils.info logger "Starting command %A" command
    let ret = commandHandler arguments
    LogUtils.info logger "End command %A" command
    ret
