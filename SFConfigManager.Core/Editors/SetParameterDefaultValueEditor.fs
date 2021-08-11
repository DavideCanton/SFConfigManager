module SFConfigManager.Core.Editors.SetParameterDefaultValueEditor

open SFConfigManager.Core.Common
open SFConfigManager.Core.Editors.Actions
open SFConfigManager.Core.Context
open SFConfigManager.Core.Editors.XMLEditor
open System
open SFConfigManager.Extensions.ResultComputationExpression

let setParameterDefaultValueEditor context section name value =

    resultExpr {
        let manifest = context.Manifest

        let! paramName = normalizeParamNameWithService context section name

        let xpath =
            String.Format(
                "/{0}:ApplicationManifest/{0}:Parameters/{0}:Parameter[@Name=\"{1}\"]",
                DefaultNamespace,
                paramName
            )

        let actions =
            [ SetAttribute
                  { Path = xpath
                    Name = "DefaultValue"
                    Value = value } ]

        return! processActionsAndSave actions manifest.RootElement.XElement manifest.ManifestPath
    }
