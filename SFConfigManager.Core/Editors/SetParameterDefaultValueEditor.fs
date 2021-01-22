module SFConfigManager.Core.Editors.SetParameterDefaultValueEditor

open SFConfigManager.Core.Common
open SFConfigManager.Core.Editors.Actions
open SFConfigManager.Core.Context
open SFConfigManager.Core.Editors.XMLEditor
open System

let setParameterDefaultValueEditor context service section name value =
    let manifest = context.Manifest

    let (Ok paramName) =
        normalizeParamNameWithService context section name

    let xpath =
        String.Format
            ("/{0}:ApplicationManifest/{0}:Parameters/{0}:Parameter[@Name=\"{1}\"]", DefaultNamespace, paramName)

    let actions =
        [ SetAttribute
            { Path = xpath
              Name = "DefaultValue"
              Value = value } ]

    processActionsAndSave actions manifest.RootElement.XElement manifest.ManifestPath
