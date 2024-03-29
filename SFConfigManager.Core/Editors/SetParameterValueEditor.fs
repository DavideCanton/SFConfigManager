﻿module SFConfigManager.Core.Editors.SetParameterValueEditor

open FSharpPlus
open SFConfigManager.Data.Parsers.ParserTypes
open SFConfigManager.Core.Common
open SFConfigManager.Core.Editors.Actions
open SFConfigManager.Core.Editors.XMLEditor
open SFConfigManager.Core.Context
open System
open SFConfigManager.Extensions.ResultComputationExpression

let private filterEnvironments (argEnvironments: Set<string>) (envs: ParametersParseResult list) =
    if argEnvironments.IsEmpty then
        envs
    else
        List.filter (fun e -> argEnvironments.Contains e.FileName) envs

let setParamValueEditor (context: Context) section name value environments =
    resultExpr {
        let! paramName = normalizeParamNameWithService context section name

        let xpath =
            String.Format("/{0}:Application/{0}:Parameters/{0}:Parameter[@Name=\"{1}\"]", DefaultNamespace, paramName)

        let processEnvironment env =
            let actions =
                [ SetAttribute
                      { Path = xpath
                        Name = "Value"
                        Value = value } ]

            processActionsAndSave actions env.RootElement.XElement env.FilePath

        return!
            context.Parameters
            |> filterEnvironments environments
            |> List.map processEnvironment
            |> Result.partition
            |> fun (_, errors) ->
                if List.isEmpty errors then
                    Ok()
                else
                    Error(SetArgumentsFailedException errors)
    }
