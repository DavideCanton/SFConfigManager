module SFConfigManager.Extensions.MaybeComputationExpression

type MaybeBuilder internal () =
    member _.Bind(x, f) = Option.bind f x

    member _.Return v = Some v

    member _.ReturnFrom(v: 'a option) = v

    member _.Zero() = None

    member _.Combine(a, b) =
        match a with
        | Some _ -> b
        | None -> None

    member _.Delay f = f ()

let maybe = MaybeBuilder()
