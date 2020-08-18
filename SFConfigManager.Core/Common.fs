module SFConfigManager.Core.Common

exception InvalidFileException

let contains (query: string) (value: string) = value.Contains query
