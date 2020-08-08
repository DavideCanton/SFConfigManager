namespace SFConfigManager.Core

module SFProjParser =

    open System.IO
    open System.Xml.Linq
    open SFConfigManager.Core.Common

    type SFProjParseResult(parameters: string list, manifestPath: string, services: string list) =
        class
            member this.Parameters = parameters
            member this.ManifestPath = manifestPath
            member this.Services = services
        end

    type SFProjParser() =
        class
            let mutable baseFolder = ""

            let joinToBase p =
                Path.Combine(baseFolder, p) |> Path.GetFullPath

            let getAttrValue (attributeName: string) (node: XElement) = node.Attribute(!>attributeName).Value

            let attrContains (query: string) (value: string) =
                query |> value.Contains

            let buildResult (document: XDocument) =
                let ns = document.Root.GetDefaultNamespace()
                let xNameBuilder name = XName.Get(name, ns.NamespaceName)

                let parameters =
                    xNameBuilder "None"
                    |> document.Descendants
                    |> Seq.filter (getAttrValue "Include" >> attrContains "ApplicationParameters")
                    |> Seq.map (getAttrValue "Include" >> joinToBase)
                    |> Seq.toList

                let node =
                    xNameBuilder "None"
                    |> document.Descendants
                    |> Seq.find (getAttrValue "Include" >> attrContains "ApplicationManifest")

                let manifestPath =
                    (getAttrValue "Include" node) |> joinToBase

                let services =
                    xNameBuilder "ProjectReference"
                    |> document.Descendants
                    |> Seq.map (getAttrValue "Include" >> joinToBase)
                    |> Seq.toList

                SFProjParseResult(parameters, manifestPath, services)


            member this.Parse path =
                try
                    baseFolder <- Path.GetDirectoryName(path)
                    let document = XDocument.Load(path)
                    let result = buildResult document
                    Ok(result)
                with e -> Error(e)
        end
