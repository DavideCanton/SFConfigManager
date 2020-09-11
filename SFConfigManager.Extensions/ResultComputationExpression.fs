module SFConfigManager.Extensions.ResultComputationExpression

type ResultBuilder internal () =
    member _.Bind (x, f) = Result.bind f x

    member _.Return v = Ok v

    member _.ReturnFrom v = v

    member _.Zero () = Ok ()

let resultExpr = ResultBuilder()