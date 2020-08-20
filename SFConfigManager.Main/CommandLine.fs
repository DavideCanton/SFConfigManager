module SFConfigManager.Main.CommandLine

open Argu
open SFConfigManager.Main.Arguments
open SFConfigManager.Main.Utils
open SFConfigManager.Core.Context

let processCommand command arguments = command arguments

let private buildContextAndExecute path service fn =
    buildContext path service
    |> Result.bind fn
    |> Result.map ignore

let get (g: ParseResults<GetArgs>) (root: ParseResults<SfConfigArgs>) =
    let name = g.GetResult(Name, defaultValue = "")
    let section = g.GetResult(Section, defaultValue = "")
    let service = g.GetResult(Service, defaultValue = "")
    let path = getSolutionPath root

    let paramPrinter fileName parameters =
        let value =
            getParamValue name section service parameters

        match value with
        | Some v -> printfn "%s: %s" fileName v.ParamValue
        | None -> printfn "%s: not found" fileName

    let innerBody context =
        // write file try
        //using (File.Open("./prova.xml", FileMode.OpenOrCreate)) (fun stream ->
        //    let q = Array.last context.manifest.RootElement.Parameters.Value.Parameters
        //    let child = XElement(q.XElement)
        //    child.SetAttributeValue(XName.Get "Name", "GNIIII")
        //    child.SetAttributeValue(XName.Get "DefaultValue", "GNEEEEE")
        //    q.XElement.AddAfterSelf(
        //        XText("\r\n\t"),
        //        child,
        //        XText("\r\n\t")
        //    )
        //    context.manifest.RootElement.XElement.Save(stream)
        //)

        printfn "Value of [Service=%s; Section=%s; Name=%s]" service section name
        context.Parameters
        |> List.iter (fun x -> paramPrinter x.FileName x.Params)
        paramPrinter "Default Value" context.Manifest.Parameters
        Ok()

    buildContextAndExecute path service innerBody

let add (a: ParseResults<AddArgs>) (root: ParseResults<SfConfigArgs>) = Ok()
