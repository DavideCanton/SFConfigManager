module SFConfigManager.Core.Editors.SetParameterDefaultValueEditor

open SFConfigManager.Core.Common
open SFConfigManager.Core.Editors.Actions
open SFConfigManager.Core.Context
open SFConfigManager.Core.Editors.XMLEditor

let setParameterDefaultValueEditor context service section name value =
    let manifest = context.Manifest

    let paramName =
        normalizeParamNameWithService service section name

    let xpath =
        sprintf "/empty:ApplicationManifest/empty:Parameters/empty:Parameter[@Name=\"%s\"]" paramName

    let actions =
        [ SetAttribute
            { Path = xpath
              Name = "DefaultValue"
              Value = value } ]

    processActionsAndSave actions manifest.RootElement.XElement manifest.ManifestPath
