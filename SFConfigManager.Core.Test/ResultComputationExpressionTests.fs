module SFConfigManager.Core.Test.ResultComputationExpressionTests

open NUnit.Framework
open FsUnit
open FSharpPlus
open SFConfigManager.Extensions.ResultComputationExpression
open SFConfigManager.Extensions.ResultExtensions

[<TestFixture>]
[<Category("Result Computation Expression")>]
type ResultComputationExpressionTests() =
    [<SetUp>]
    member _.Setup() = ()

    [<Test>]
    member _.ReturnTest1() =
        let r = resultExpr { return 1 }

        Result.isOk r |> should equal true
        Result.get r |> should equal 1

    [<Test>]
    member _.BindTest1() =
        let r =
            resultExpr {
                let! r = Ok 1
                return r
            }

        Result.isOk r |> should equal true
        Result.get r |> should equal 1

    [<Test>]
    member _.BindTest2() =
        let r =
            resultExpr {
                let! r = Error 3
                return r
            }

        Result.isError r |> should equal true
        Result.getError r |> should equal 3

    [<Test>]
    member _.ReturnFromTest1() =
        let r = resultExpr { return! Ok 1 }

        Result.isOk r |> should equal true
        Result.get r |> should equal 1

    [<Test>]
    member _.ReturnFromTest2() =
        let r = resultExpr { return! Error 3 }

        Result.isError r |> should equal true
        Result.getError r |> should equal 3

    [<Test>]
    member _.CombineTest1() =
        let r =
            resultExpr {
                yield! Ok 1
                yield! Ok 2
                yield! Ok 3
            }

        Result.isOk r |> should equal true
        Result.get r |> should equal 3

    [<Test>]
    member _.CombineTest2() =
        let mutable values = []

        let r =
            resultExpr {
                values <- values @ [ 1 ]
                yield! Ok 1
                values <- values @ [ 2 ]
                yield! Error 2
                values <- values @ [ 3 ]
                yield! Ok 3
            }

        Result.isError r |> should equal true
        Result.getError r |> should equal 2
        values |> should equal [ 1; 2 ]

    [<Test>]
    member _.ForTest1() =
        let mutable values = []

        let r =
            resultExpr {
                for i in [ 1; 2; 3 ] do
                    values <- values @ [ i ]
                    yield ()
            }

        Result.isOk r |> should equal true
        Result.get r |> should equal ()
        values |> should equal [ 1; 2; 3 ]

    [<Test>]
    member _.ForTest2() =
        let mutable values = []

        let r =
            resultExpr {
                for i in [ 1; 2; 3 ] do
                    values <- values @ [ i ]
                    if i = 2 then yield! Error 2 else yield ()
            }

        Result.isError r |> should equal true
        Result.getError r |> should equal 2
        values |> should equal [ 1; 2 ]
