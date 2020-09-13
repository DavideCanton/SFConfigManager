module SFConfigManager.Core.Editors.Actions

open System.Xml.Linq

exception SetArgumentsFailedException of errors: exn list

type SetAttributeArgs =
    { Path: string
      Name: string
      Value: string }

type AddActionArgs = { Path: string; Element: XElement }

type AddWithElementArgs = { Target: XElement; Element: XElement }

type XMLAction =
    | SetAttribute of SetAttributeArgs
    | AddSiblingElement of AddWithElementArgs
    | AddSibling of AddActionArgs
    | AddLastChild of AddActionArgs