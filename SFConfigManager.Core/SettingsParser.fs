namespace SFConfigManager.Core

module SettingsParser =

    open System.IO
    open System.Xml.Linq
    open SFConfigManager.Core.Common

    type SettingsParser() = class
        let mutable sections = Map.empty

        let getItems ns (element: XElement) =
            element.Elements(XName.Get("Parameter", ns))
            |> Seq.map (fun q -> (element.Attribute(!> "Name").Value, (q.Attribute(!> "Name").Value, q.Attribute(!> "Value").Value)))

        let fill (doc: XDocument) =
            let ns = doc.Root.GetDefaultNamespace()
            sections <- doc
                .Descendants(XName.Get("Section", ns.NamespaceName))
                |> Seq.collect (getItems ns.NamespaceName)
                |> Map.ofSeq

        member this.Sections = sections

        member this.Parse path =
            let dir = Path.GetDirectoryName path
            let p = Path.Combine(dir, "PackageRoot", "Config", "Settings.xml")
            let doc = XDocument.Load(p)
            fill doc
    end