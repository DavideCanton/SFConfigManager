module SFConfigManager.Core.Test.ManifestParserTests

open NUnit.Framework
open FsUnit
open FSharpPlus
open System.IO
open SFConfigManager.Core.Common
open SFConfigManager.Core.Parsers
open SFConfigManager.Extensions.ResultExtensions
open SFConfigManager.Data.Parsers.ParserTypes

let testFolder =
    Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "ManifestSamples")

let assertParamIs (parameter: ParameterResultEntry) paramName paramValue =
    parameter.ParamName |> should equal paramName
    parameter.ParamValue |> should equal paramValue

[<TestFixture>]
type XMLEditorTests() =
    [<SetUp>]
    member _.Setup() = ()

    [<Test>]
    [<Category("ApplicationManifest Parsing tests")>]
    member _.ManifestParserTestOk() =
        let path = Path.Combine(testFolder, "Sample1.xml")

        let result = ManifestParser.parseManifest path

        Result.isOk result |> should be True

        let parsedManifest = Result.get result

        parsedManifest.ManifestPath |> should equal path
        let parameters = parsedManifest.Parameters 

        parameters |> should haveLength 6
        
        assertParamIs parameters.[0] "Backend_InstanceCount" "-1"
        assertParamIs parameters.[1] "ProvaSFWebAPI_ASPNETCORE_ENVIRONMENT" ""
        assertParamIs parameters.[2] "ProvaSFWebAPI_InstanceCount" "-1"
        assertParamIs parameters.[3] "ProvaSFWebAPI_MyConfigSection_MyParameter" "Default"
        assertParamIs parameters.[4] "ProvaSFFrontend_ASPNETCORE_ENVIRONMENT" ""
        assertParamIs parameters.[5] "ProvaSFFrontend_InstanceCount" "-1"

    [<Test>]
    [<Category("ApplicationManifest Parsing tests")>]
    member _.ManifestParserTestFail() =
        let path = Path.Combine(testFolder, "InvalidSample1.xml")

        let result = ManifestParser.parseManifest path

        Result.isError result |> should be True
        let error = Result.getError result
        
        error |> should be instanceOfType<InvalidFileException>

    [<Test>]
    [<Category("ApplicationManifest Parsing tests")>]
    member _.ManifestParserTestNotFound() =
        let path = Path.Combine(testFolder, "NonExistantFile.xml")

        let result = ManifestParser.parseManifest path

        Result.isError result |> should be True
        let error = Result.getError result
        
        error |> should be instanceOfType<FileNotFoundException>