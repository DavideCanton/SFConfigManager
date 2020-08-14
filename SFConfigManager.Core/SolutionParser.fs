module SFConfigManager.Core.SolutionParser

open Microsoft.Build.Construction
open FSharpPlus

type SolutionParseResult = { SfProjList: string list }

let private buildResult projectList = { SfProjList = projectList }

let parseSolution path =
    let absPathGetter (p: ProjectInSolution) = p.AbsolutePath
    try
        SolutionFile.Parse(path).ProjectsInOrder
        |> Seq.map absPathGetter
        |> Seq.filter (String.endsWith "sfproj")
        |> Seq.toList
        |> buildResult
        |> Ok
    with e -> Error e
