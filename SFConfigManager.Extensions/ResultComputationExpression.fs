module SFConfigManager.Extensions.ResultComputationExpression

type ResultBuilder internal () =
    member _.Bind(x, f) = Result.bind f x

    member _.Return v = Ok v

    member _.ReturnFrom v = v

    member this.Yield v = this.Return v
    member this.YieldFrom v = this.ReturnFrom v

    member _.Zero() = Ok()

    member this.Combine(a, b) =
        match a with
        | Ok _ -> this.Run b
        | Error _ as e -> e

    member _.Delay f = f

    member _.Run f = f ()

    member this.For(vs, f) =
        let z = this.Zero()
        let folder a v = this.Bind(a, (fun _ -> f v))
        List.fold folder z vs

let resultExpr = ResultBuilder()
