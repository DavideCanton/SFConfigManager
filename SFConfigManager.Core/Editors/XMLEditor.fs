module SFConfigManager.Core.Editors.XMLEditor

open System.Xml.Linq
open System.Xml
open System.Xml.XPath
open SFConfigManager.Core.Common
open FSharpPlus

let addTagAfter (element: XElement) (newTag: XElement) = element.AddAfterSelf(newTag)

let addLastChild (element: XElement) (newTag: XElement) = element.Add(newTag)

let getNamespaces (root: XElement) =
    let getName (vs: XAttribute) =
        match vs with
        | a when a.Name.Namespace = XNamespace.None -> ""
        | a -> a.Name.LocalName

    let toManager (namespaces: seq<string * XNamespace>) =
        let manager = XmlNamespaceManager(NameTable())
        namespaces
        |> Seq.iter (fun (k, v) -> manager.AddNamespace(k, v.NamespaceName))

        manager.AddNamespace("empty", manager.DefaultNamespace)
        manager

    root.Attributes()
    |> Seq.filter (fun a -> a.IsNamespaceDeclaration)
    |> Seq.groupBy getName
    |> Seq.map
        (mapSecond
         <| fun vs ->
             vs
             |> Seq.head
             |> (fun a -> a.Value)
             |> XNamespace.Get)
    |> toManager

let setAttributeByXPath (path: string) (name: string) (value: string) (root: XElement) =
    ()
    |> Result.protect (fun () ->
        let ns = getNamespaces root
        let element = root.XPathSelectElement(path, ns)
        element.SetAttributeValue(!?name, value))

let saveXML (path: string) (element: XElement) =
    ()
    |> Result.protect (fun () ->
        use writer = XmlWriter.Create(path)
        element.Save(writer))
