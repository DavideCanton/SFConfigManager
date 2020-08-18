namespace SFConfigManager.Data

open FSharp.Data

type FabricTypes = XmlProvider<Schema="Schema/ServiceFabricServiceModel.xsd", EmbeddedResource="SFConfigManager.Data, SFConfigManager.Data.Schema.ServiceFabricServiceModel.xsd">
