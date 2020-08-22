module SFConfigManager.Extensions.ResultExtensions

module Result =
    let (|ResultOk|) =
        function
        | Ok _ -> true
        | Error _ -> false

    let isOk (ResultOk isOk) = isOk

    let isError (ResultOk isOk) = not isOk

    let getError =
        function
        | Ok _ -> invalidArg "result" "Invalid argument"
        | Error e -> e
