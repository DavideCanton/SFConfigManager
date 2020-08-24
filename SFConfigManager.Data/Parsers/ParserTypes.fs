module SFConfigManager.Data.Parsers.ParserTypes

open SFConfigManager.Data

type ParameterResultEntry =
    { ServiceName: string
      ParamName: string
      ParamValue: string }

type ManifestParseResult =
    { Parameters: ParameterResultEntry list
      ManifestPath: string
      RootElement: FabricTypes.ApplicationManifest }

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
      Sections: Map<string, (string * string) list>
      RootServiceElement: FabricTypes.ServiceManifest
      RootSettingsElement: FabricTypes.Settings2 }


type SFProjParseResult =
    { FilePath: string
      Parameters: string list
      ManifestPath: string
      Services: string list }
