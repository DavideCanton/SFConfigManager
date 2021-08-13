module SFConfigManager.Core.Editors.XMLEditor

open System.Xml.Linq
open System.Text
open System.Xml
open System.IO
open System.Xml.XPath
open SFConfigManager.Core.Common
open SFConfigManager.Extensions.ResultComputationExpression
open SFConfigManager.Core.Editors.Actions
open FSharpPlus

exception ErrorInActionException of message: string

let addTagAfter (element: XElement) (newTag: XElement) = element.AddAfterSelf(newTag)

let addLastChild (element: XElement) (newTag: XElement) = element.Add(newTag)
let DefaultNamespace = "empty"

let getNamespaces (root: XElement) =
    let getName (vs: XAttribute) =
        match vs.Name with
        | a when a.Namespace = XNamespace.None -> ""
        | a -> a.LocalName

    let toManager (namespaces: seq<string * XNamespace>) =
        let manager = XmlNamespaceManager(NameTable())

        let adder (k, v: XNamespace) =
            manager.AddNamespace(k, v.NamespaceName)

        Seq.iter adder namespaces

        manager.AddNamespace(DefaultNamespace, manager.DefaultNamespace)
        manager

    let getAttrNamespace (vs: XAttribute seq) =
        vs
        |> Seq.head
        |> (fun a -> a.Value)
        |> XNamespace.Get

    root.Attributes()
    |> Seq.filter (fun a -> a.IsNamespaceDeclaration)
    |> Seq.groupBy getName
    |> Seq.map (mapSecond getAttrNamespace)
    |> toManager

let findElementByXPath (xpath: string) (root: XElement) =
    let ns = getNamespaces root
    let element = root.XPathSelectElement(xpath, ns)
    Option.ofObj element

let findElementsByXPath (xpath: string) (root: XElement) =
    let ns = getNamespaces root
    let elements = root.XPathSelectElements(xpath, ns)
    List.ofSeq elements

let setAttributeByXPath (path: string) (name: string) (value: string) (root: XElement) =
    match findElementByXPath path root with
    | Some element -> element.SetAttributeValue(!?name, value) |> Ok
    | None -> ErrorInActionException "Tag not found" |> Error

let saveXML (path: string) (element: XElement) =
    let makeWriter (path: string) =
        let settings = XmlWriterSettings()
        settings.Indent <- true
        settings.NewLineHandling <- NewLineHandling.Replace
        settings.Encoding <- Encoding.UTF8
        settings.OmitXmlDeclaration <- false
        XmlWriter.Create(path, settings)

    let elementWithoutWhitespaces (element: XElement) =
        use fs = new StringReader(element.ToString())
        XDocument.Load(fs).Root

    let body () =
        using
            (makeWriter path)
            (fun w ->
                let el = elementWithoutWhitespaces element
                el.Save(w))

    protectAndRun body

let private processAction (root: XElement) action =
    match action with
    | SetAttribute { Path = path
                     Name = name
                     Value = value } -> setAttributeByXPath path name value root

    | AddSibling { Path = path; Element = element } ->
        match findElementByXPath path root with
        | Some elFound -> addTagAfter elFound element |> Ok
        | None -> ErrorInActionException "Tag not found" |> Error

    | AddSiblingElement { Target = target; Element = element } -> addTagAfter target element |> Ok

    | AddLastChild { Path = path; Element = element } ->
        match findElementByXPath path root with
        | Some elFound -> addLastChild elFound element |> Ok
        | None -> ErrorInActionException "Tag not found" |> Error

let processActionsAndSave actions (root: XElement) path =
    resultExpr {
        let folder acc a =
            resultExpr {
                do! acc
                return! processAction root a
            }

        do! List.fold folder (resultExpr.Zero()) actions
        return! saveXML path root
    }
