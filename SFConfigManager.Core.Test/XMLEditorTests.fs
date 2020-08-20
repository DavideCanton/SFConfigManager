module SFConfigManager.Core.Test

open NUnit.Framework
open FsUnit
open FSharpPlus
open System.Xml.Linq
open System.IO
open SFConfigManager.Core.Editors.XMLEditor
open System.Text

let inline (!?) name = XName.Get name

let makeTestFolder s = Path.Combine("./Data", s)
let makeInputFolder s = Path.Combine(makeTestFolder s, "Input")
let makeOutputFolder s = Path.Combine(makeTestFolder s, "Output")

let buildTestSource name =
    let inputFolder = makeInputFolder name
    let outputFolder = makeOutputFolder name

    let makeTuple (n: string) =
        (Path.GetFileNameWithoutExtension n, Path.Combine(inputFolder, n), Path.Combine(outputFolder, n))

    let makeTestCase (fileName, inputPath, outputPath) =
        TestCaseData(inputPath, outputPath).SetName(name + " test [" + fileName + "]")

    Directory.GetFiles(inputFolder, "*.xml")
    |> List.ofArray
    |> List.map (Path.GetFileName >> makeTuple >> makeTestCase)

[<TestFixture>]
type XMLEditorTests() =
    [<SetUp>]
    member _.Setup() = ()

    static member AddTagAfterSource = buildTestSource "AddTagAfter"

    [<Test>]
    [<TestCaseSource("AddTagAfterSource")>]
    member _.AddTagAfterTest inputPath outputPath =

        use stream = File.OpenRead inputPath
        let xml = XDocument.Load(stream)

        let target =
            xml.Descendants(!? "item")
            |> Seq.tryFind (fun (n: XElement) -> n.Attribute(!? "id").Value = "target")

        target.IsSome |> should equal true

        let target = target.Value

        let child = XElement(target)
        child.SetAttributeValue(!? "id", "added")
        addTagAfter target child

        let expected = File.ReadAllText outputPath

        use writeStream =
            { new StringWriter() with
                override this.Encoding = Encoding.UTF8 }

        xml.Save(writeStream)
        writeStream.ToString() |> should equal expected
