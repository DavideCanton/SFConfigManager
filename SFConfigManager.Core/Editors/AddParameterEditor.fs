module SFConfigManager.Core.Editors.AddParameterEditor

open SFConfigManager.Core.Editors.Actions
open SFConfigManager.Core.Editors.XMLEditor
open SFConfigManager.Extensions.ResultComputationExpression
open SFConfigManager.Core.Context
open SFConfigManager.Core.Common
open SFConfigManager.Data.Parsers.ParserTypes
open System.Xml.Linq
open System

let private innerBuildParameterElement name value isDefault ns =
    let element = XElement(XName.Get("Parameter", ns))

    let valueAttrName =
        if isDefault then "DefaultValue" else "Value"

    element.SetAttributeValue(!? "Name", name)
    element.SetAttributeValue(!?valueAttrName, value)
    element

let private buildParameterElement name value ns =
    innerBuildParameterElement name value false ns

let private buildParameterManifestElement name value ns =
    innerBuildParameterElement name value true ns

let private getParamNamePrefix (paramName: string) =
    let index = paramName.LastIndexOf("_")
    paramName.[index + 1..]

let private buildManifestParameterAction (manifest: ManifestParseResult) paramName value =
    let prefix = getParamNamePrefix paramName

    let xpath =
        String.Format(
            "/{0}:ApplicationManifest/{0}:Parameters/{0}:Parameter[starts-with(@Name, \"{1}\")]",
            DefaultNamespace,
            prefix
        )

    let target =
        findElementsByXPath xpath manifest.RootElement.XElement
        |> List.last

    let manager =
        getNamespaces manifest.RootElement.XElement

    let defaultNamespace = manager.DefaultNamespace

    let element =
        buildParameterManifestElement paramName value defaultNamespace

    AddSiblingElement { Target = target; Element = element }

let private buildManifestConfigAction (manifest: ManifestParseResult) (section: string) name paramName servicePkgName =
    let xpath =
        String.Format(
            "/{0}:ApplicationManifest/{0}:ServiceManifestImport/{0}:ServiceManifestRef[@ServiceManifestName=\"{1}\"]/following-sibling::{0}:ConfigOverrides/{0}:ConfigOverride/{0}:Settings/{0}:Section[@Name=\"{2}\"]",
            DefaultNamespace,
            servicePkgName,
            section
        )

    let manager =
        getNamespaces manifest.RootElement.XElement

    let defaultNamespace = manager.DefaultNamespace

    let element =
        buildParameterElement name (sprintf "[%s]" paramName) defaultNamespace

    AddLastChild { Path = xpath; Element = element }

let private updateManifest (manifest: ManifestParseResult) (section: string) name paramName value servicePkgName =
    let actions =
        [ buildManifestParameterAction manifest paramName value
          buildManifestConfigAction manifest section name paramName servicePkgName ]

    actions


let private updateSettings (settings: SettingsParseResult) (section: string) name value =
    let xpath =
        String.Format("/{0}:Settings/{0}:Section[@Name=\"{1}\"]", DefaultNamespace, section)

    let manager =
        getNamespaces settings.RootSettingsElement.XElement

    let defaultNamespace = manager.DefaultNamespace

    let element =
        buildParameterElement name value defaultNamespace

    let actions =
        [ AddLastChild { Path = xpath; Element = element } ]

    actions

let private updateParameters (parsedParameter: ParametersParseResult) paramName value =
    let prefix = getParamNamePrefix paramName

    let xpath =
        String.Format(
            "/{0}:Application/{0}:Parameters/{0}:Parameter[starts-with(@Name, \"{1}\")]",
            DefaultNamespace,
            prefix
        )

    let target =
        findElementsByXPath xpath parsedParameter.RootElement.XElement
        |> List.last

    let manager =
        getNamespaces parsedParameter.RootElement.XElement

    let defaultNamespace = manager.DefaultNamespace

    let element =
        buildParameterElement paramName value defaultNamespace

    let actions =
        [ AddSiblingElement { Target = target; Element = element } ]

    actions

let addParameterEditor (context: Context) (section: string) (name: string) (value: string) =
    resultExpr {
        let manifest = context.Manifest
        let settings = context.Settings.Value
        let parameters = context.Parameters
        let! paramName = normalizeParamNameWithService context section name

        let manifestActions =
            updateManifest manifest section name paramName value settings.RootServiceElement.Name

        let settingsActions =
            updateSettings settings section name value

        let tupleGenerator (parsedParameter: ParametersParseResult) =
            let parametersActions =
                updateParameters parsedParameter paramName value

            (parametersActions, parsedParameter.RootElement.XElement, parsedParameter.FilePath)

        let objs =
            [ (manifestActions, manifest.RootElement.XElement, manifest.ManifestPath)
              (settingsActions, settings.RootSettingsElement.XElement, settings.SettingsFilePath) ]
            @ List.map tupleGenerator parameters

        for (actions, element, filePath) in objs do
            yield! processActionsAndSave actions element filePath
    }
