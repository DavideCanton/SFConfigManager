module SFConfigManager.Core.Editors.XMLEditor

open System.Xml.Linq
open System.Xml
open System.Xml.XPath
open SFConfigManager.Core.Common
open SFConfigManager.Extensions.ResultComputationExpression
open SFConfigManager.Core.Editors.Actions
open FSharpPlus

exception ErrorInActionException of message: string

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

let findElementByXPath (xpath: string) (root: XElement) =
    let ns = getNamespaces root
    let element = root.XPathSelectElement(xpath, ns)
    Option.ofObj element

let setAttributeByXPath (path: string) (name: string) (value: string) (root: XElement) =
    ()
    |> Result.protect (fun () ->
        let element = findElementByXPath path root
        element.Value.SetAttributeValue(!?name, value))

let saveXML (path: string) (element: XElement) =
    ()
    |> Result.protect (fun () -> using (XmlWriter.Create(path)) (fun w -> element.Save(w)))

let private processAction (root: XElement) action =
    match action with
    | SetAttribute { Path = path; Name = name; Value = value } -> setAttributeByXPath path name value root
    | AddSibling { Path = path; Element = element } ->
        match findElementByXPath path root with
        | Some elFound -> addTagAfter elFound element |> Ok
        | None -> ErrorInActionException "Tag not found" |> Error
    | AddLastChild { Path = path; Element = element } ->
        match findElementByXPath path root with
        | Some elFound -> addLastChild elFound element |> Ok
        | None -> ErrorInActionException "Tag not found" |> Error

let processActionsAndSave actions (root: XElement) path =
    resultExpr {
        let folder _ a =
            resultExpr { return! processAction root a }

        do! List.fold folder (Ok()) actions
        return! saveXML path root
    }
