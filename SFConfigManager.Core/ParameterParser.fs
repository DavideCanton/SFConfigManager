namespace SFConfigManager.Core

module ParameterParser =

    open System.Xml.Linq
    open SFConfigManager.Core.Common

    type ParameterResultEntry =
        { ServiceName: string
          SectionName: string option
          ParamValue: string }

    type ParametersParseResult =
        { Params: Map<string, ParameterResultEntry> }

    let private extract (p: string) =
        let parts = (p.Split '_') |> Array.toList
        match parts with
        | [ sn; pn ] -> Some(sn, None, pn)
        | [ sn; snn; pn ] -> Some(sn, Some snn, pn)
        | _ -> None

    let private getItems ns element =
        let value = getAttrValue "Value" element
        getAttrValue "Name" element
        |> extract
        |> Option.map (fun (sn, snn, pn) ->
            (pn,
             { ServiceName = sn
               SectionName = snn
               ParamValue = value }))

    let private buildResult (doc: XDocument) =
        let ns = doc.Root.GetDefaultNamespace()

        let parameters =
            xNameBuilder ns "Parameter"
            |> doc.Descendants
            |> Seq.map (getItems ns)
            |> Seq.choose id
            |> Map.ofSeq

        { Params = parameters }

    let parseParameters (path: string) =
        try
            path |> XDocument.Load |> buildResult |> Ok
        with e -> Error e
