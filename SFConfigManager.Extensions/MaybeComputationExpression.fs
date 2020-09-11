module SFConfigManager.Extensions.MaybeComputationExpression

type MaybeBuilder internal () =
    member _.Bind (x, f) = Option.bind f x

    member _.Return v = Some v

    member _.ReturnFrom (v: 'a option) = v

    member _.Zero() = None

let maybe = MaybeBuilder()