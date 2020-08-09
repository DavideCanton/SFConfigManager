namespace SFConfigManager.Core

module SettingsParser =

    open System.IO
    open System.Xml.Linq
    open SFConfigManager.Core.Common

    type SettingsParseResult =
        { Sections: Map<string, string * string> }

    let private mapEntryBuilder element child =
        let eName = getAttrValue "Name" element
        let qName = getAttrValue "Name" child
        let qValue = getAttrValue "Value" child
        (eName, (qName, qValue))

    let private getItems ns (element: XElement) =
        xNameBuilder ns "Parameter"
        |> element.Elements
        |> Seq.map (mapEntryBuilder element)

    let private buildResult (doc: XDocument) =
        let ns = doc.Root.GetDefaultNamespace()
        let xNameBuilderP = xNameBuilder ns

        let sections =
            xNameBuilderP "Section"
            |> doc.Descendants
            |> Seq.collect (getItems ns)
            |> Map.ofSeq

        { Sections = sections }

    let parseSettings path =
        try
            Path.GetDirectoryName path
            |> fun d -> Path.Combine(d, "PackageRoot", "Config", "Settings.xml")
            |> XDocument.Load
            |> buildResult
            |> Ok
        with e -> Error e
