module SFConfigManager.Data.Parsers.ParserTypes

open SFConfigManager.Data

type ParameterResultEntry =
    { ParamName: string
      ParamValue: string }

type ManifestSectionKey =
    { ServicePkgName: string 
      Section: string
      ParamName: string }

type ManifestParseResult =
    { Parameters: ParameterResultEntry list
      ManifestPath: string
      RootElement: FabricTypes.ApplicationManifest
      Sections: Map<ManifestSectionKey, string> }

type SolutionParseResult = { SfProjList: string list }


type ParametersParseResult =
    { Params: ParameterResultEntry list
      FileName: string
      FilePath: string
      RootElement: FabricTypes.Application }


type SettingsParseResult =
    { Service: string
      ServiceFilePath: string
      SettingsFilePath: string
      ServicePkgName: string
      Sections: Map<string, (string * string) list>
      RootServiceElement: FabricTypes.ServiceManifest
      RootSettingsElement: FabricTypes.Settings2 }


type SFProjParseResult =
    { FilePath: string
      Parameters: string list
      ManifestPath: string
      Services: string list }
