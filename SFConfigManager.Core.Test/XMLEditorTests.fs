module SFConfigManager.Core.Test.XMLEditorTests

open NUnit.Framework
open FsUnit
open FSharpPlus
open System.Xml.Linq
open System.IO
open SFConfigManager.Core.Editors.XMLEditor
open SFConfigManager.Core.Common
open System.Text

let makeTestFolder s =
    Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", s)

let makeInputFolder s = Path.Combine(makeTestFolder s, "Input")
let makeOutputFolder s = Path.Combine(makeTestFolder s, "Output")

let buildTestSource name =
    let inputFolder = makeInputFolder name
    let outputFolder = makeOutputFolder name

    let makeTuple (n: string) =
        (Path.GetFileNameWithoutExtension n, Path.Combine(inputFolder, n), Path.Combine(outputFolder, n))

    let makeTestCase (fileName, inputPath, outputPath) =
        TestCaseData(inputPath, outputPath)
            .SetName(name + " test [" + fileName + "]")
            .SetCategory("XML Manipulation tests")        

    Directory.GetFiles(inputFolder, "*.xml")
    |> List.ofArray
    |> List.map (Path.GetFileName >> makeTuple >> makeTestCase)

let testBody inputPath outputPath callback =
    use stream = File.OpenRead inputPath
    let xml = XDocument.Load(stream)

    let target =
        xml.Descendants()
        |> Seq.tryFind (fun (n: XElement) ->
            ()
            |> Result.protect (fun () -> n.Attribute(!? "id").Value = "target")
            |> Result.either id (konst false))

    target.IsSome |> should equal true

    callback xml target.Value

    let expected = File.ReadAllText outputPath

    use writeStream =
        { new StringWriter() with
            override _.Encoding = Encoding.UTF8 }

    xml.Save(writeStream)
    writeStream.ToString() |> should equal expected

[<TestFixture>]
type XMLEditorTests() =
    [<SetUp>]
    member _.Setup() = ()

    static member AddTagAfterSource = buildTestSource "AddTagAfter"
    static member AddLastChildSource = buildTestSource "AddLastChild"

    [<Test>]
    [<TestCaseSource("AddTagAfterSource")>]
    member _.AddTagAfterTest inputPath outputPath =
        let callback _ (target: XElement) =
            let child = XElement(!? "item")
            child.SetAttributeValue(!? "id", "added")
            addTagAfter target child

        testBody inputPath outputPath callback

    [<Test>]
    [<TestCaseSource("AddLastChildSource")>]
    member _.AddLastChildTest inputPath outputPath =
        let callback _ (target: XElement) =
            let child = XElement(!? "item")
            child.SetAttributeValue(!? "id", "added")
            addLastChild target child

        testBody inputPath outputPath callback
