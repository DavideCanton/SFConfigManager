namespace SFConfigManager.Core

open System.Xml.Linq

module Common =
    let inline (!>) (x: ^a): ^b =
        ((^a or ^b): (static member op_Implicit: ^a -> ^b) x)

    let getAttrValue (attributeName: string) (node: XElement) = node.Attribute(!>attributeName).Value

    let contains (query: string) (value: string) = query |> value.Contains

    let xNameBuilder (ns: XNamespace) name = XName.Get(name, ns.NamespaceName)
