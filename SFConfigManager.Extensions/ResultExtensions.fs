module SFConfigManager.Extensions.ResultExtensions

module Result =
    let isOk result =
        match result with
        | Ok _ -> true
        | Error _ -> false

    let isError r = not << isOk <| r