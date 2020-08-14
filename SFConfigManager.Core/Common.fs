module SFConfigManager.Core.Common

open System.Xml.Linq
    
let inline (!>) (x: ^a): ^b =
    ((^a or ^b): (static member op_Implicit: ^a -> ^b) x)

let getAttrValue (attributeName: string) (node: XElement) = node.Attribute(!>attributeName).Value

let contains (query: string) (value: string) = value.Contains query

let xNameBuilder (ns: XNamespace) name = XName.Get(name, ns.NamespaceName)
 