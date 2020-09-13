module SFConfigManager.Extensions.ResultComputationExpression

type ResultBuilder internal () =
    member _.Bind (x, f) = Result.bind f x

    member _.Return v = Ok v

    member _.ReturnFrom v = v

    member _.Zero () = Ok ()

    member _.Combine (a, b) =
        match a with
        | Ok _ -> b
        | Error _ as e -> e

    member _.Delay f = f ()

let resultExpr = ResultBuilder()