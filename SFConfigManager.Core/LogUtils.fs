module SFConfigManager.Core.LogUtils

open log4net
open System

let getLoggerFromString (s: string) = LogManager.GetLogger(s)
let getLoggerFromType (s: Type) = LogManager.GetLogger(s)

let private callLogger method format = Printf.ksprintf method format

let debug (logger: ILog) format = callLogger logger.Debug format
let info (logger: ILog) format = callLogger logger.Info format
let warning (logger: ILog) format = callLogger logger.Warn format
let error (logger: ILog) format = callLogger logger.Error format