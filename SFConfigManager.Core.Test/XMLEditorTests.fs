module SFConfigManager.Core.Test

open NUnit.Framework
open FsUnit
open FSharpPlus
open System.Xml.Linq
open System.IO
open SFConfigManager.Core.Editors.XMLEditor
open System.Text
open System

let inline (!?) name = XName.Get name

[<SetUp>]
let Setup () = ()

[<Test>]
let ``addTagAfter should work correctly`` () =
    let path = "./Data/Test01.xml"
    use stream = File.OpenRead path
    let xml = XDocument.Load(stream)
    let listNode = xml.Root.Element(!? "list")
    let lastChild = Seq.last <| listNode.Elements(!? "item")
    let child = XElement(lastChild)
    child.SetAttributeValue(!? "id", 4)
    addTagAfter lastChild child 1

    let expectedXml = """<?xml version="1.0" encoding="utf-8"?>
<root>
  <list>
    <item id="1" />
    <item id="2" />
    <item id="3" />
    <item id="4" />
  </list>
  <list>
    <item id="1" />
    <item id="2" />
    </list>
</root>
"""

    let expected =
        String.replace "\n" Environment.NewLine expectedXml

    use writeStream =
        { new StringWriter() with
            member this.Encoding = Encoding.UTF8 }

    xml.Save(writeStream)
    writeStream.ToString() |> should equal expected
