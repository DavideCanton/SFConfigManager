module SFConfigManager.Extensions.StringExtensions

module String =
    let isSubstring (query: string) (value: string) = value.Contains query
