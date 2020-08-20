module SFConfigManager.Core.Editors.XMLEditor

open System.Xml.Linq
open System

let private computeIndent indentNumber =
    if indentNumber > 0 then (String.replicate indentNumber "\t") else ""

let addTagAfter (element: XElement) (newTag: XElement) indentNumber =
    let indent = computeIndent indentNumber

    let textBefore =
        sprintf "%s%s" Environment.NewLine indent

    //let args: XNode[] = [| XText(textBefore); newTag; XText(Environment.NewLine) |]
    let args: XNode [] = [| newTag |]

    element.AddAfterSelf(args)

let addChildAtEnd (element: XElement) (newTag: XElement) indentNumber =
    let indent = computeIndent (indentNumber + 1)

    let textBefore =
        sprintf "%s%s" Environment.NewLine indent

    let args: XNode [] =
        [| XText(textBefore)
           newTag
           XText(Environment.NewLine) |]

    element.Add(args)
