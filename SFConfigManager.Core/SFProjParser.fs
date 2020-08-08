namespace SFConfigManager.Core

module SFProjParser =

    open System.IO
    open System.Xml.Linq

    let inline (!>) (x:^a) : ^b = ((^a or ^b) : (static member op_Implicit : ^a -> ^b) x)

    type SFProjParser() = class
        let mutable baseFolder = ""
        let mutable parameters = []
        let mutable manifestPath  = ""
        let mutable services  = []

        let joinToBase p =
            Path.Combine (baseFolder, p) |> Path.GetFullPath

        let getAttrValue (attributeName: string) (node: XElement) =
            node.Attribute(!> attributeName).Value

        let fill (document: XDocument) =
            let ns = document.Root.GetDefaultNamespace()
            parameters <- document
                .Descendants(XName.Get("None", ns.NamespaceName))
                |> Seq.filter (fun x -> (getAttrValue "Include" x).Contains("ApplicationParameters"))
                |> Seq.map (getAttrValue "Include")
                |> Seq.map joinToBase
                |> Seq.toList
        
            let node = document.Descendants(XName.Get("None", ns.NamespaceName))
                       |> Seq.find (fun x -> (getAttrValue "Include" x).Contains("ApplicationManifest"))

            manifestPath <- (getAttrValue "Include" node) |> joinToBase

            services <- document
                .Descendants(XName.Get("ProjectReference", ns.NamespaceName))
                |> Seq.map (getAttrValue "Include")
                |> Seq.map joinToBase
                |> Seq.toList
        
        member this.Parameters = parameters
        member this.ManifestPath = manifestPath
        member this.Services = services

        member this.parse path =
            baseFolder <- Path.GetDirectoryName(path)
            let document = XDocument.Load(path)
            fill document
    end