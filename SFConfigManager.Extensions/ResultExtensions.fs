module SFConfigManager.Extensions.ResultExtensions

module Result =
    let (|ResultOk|) = function
    | Ok _ -> true
    | Error _ -> false

    let isOk (ResultOk isOk) = isOk

    let isError (ResultOk isOk) = not isOk
