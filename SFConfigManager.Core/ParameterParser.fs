namespace SFConfigManager.Core

module ParameterParser =

    open System.Xml.Linq
    open SFConfigManager.Core.Common
    open FSharpPlus

    type ParameterResultEntry =
        { ServiceName: string
          ParamName: string
          ParamValue: string }

    type ParametersParseResult = { Params: ParameterResultEntry list }

    let private splitTwo (sep: string) (value: string) =
        let list = String.split [sep] value |> Seq.toList
        match list with
        | [] -> None
        | (sn::pn) -> Some (sn, String.concat "_" pn)

    let private extract (p: string) =
        splitTwo "_" p

    let private getItems ns element =
        let value = getAttrValue "Value" element
        getAttrValue "Name" element
        |> extract
        |> Option.map (fun (sn, pn) ->
            { ServiceName = sn
              ParamName = pn
              ParamValue = value })

    let private buildResult (doc: XDocument) =
        let ns = doc.Root.GetDefaultNamespace()

        let parameters =
            xNameBuilder ns "Parameter"
            |> doc.Descendants
            |> Seq.map (getItems ns)
            |> Seq.choose id
            |> Seq.toList

        { Params = parameters }

    let parseParameters (path: string) =
        try
            path |> XDocument.Load |> buildResult |> Ok
        with e -> Error e
