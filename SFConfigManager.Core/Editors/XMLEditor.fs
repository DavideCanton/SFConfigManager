module SFConfigManager.Core.Editors.XMLEditor

open System.Xml.Linq

let addTagAfter (element: XElement) (newTag: XElement) =
    element.AddAfterSelf(newTag)

let addChildAtEnd (element: XElement) (newTag: XElement) =    
    element.Add(newTag)
