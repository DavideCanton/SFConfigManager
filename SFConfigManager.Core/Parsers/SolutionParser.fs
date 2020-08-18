module SFConfigManager.Core.Parsers.SolutionParser

open Microsoft.Build.Construction
open FSharpPlus

type SolutionParseResult = { 
    SfProjList: string list
    CsProjList: string list 
}

let private buildResult sfProjs csProjs = { 
    SfProjList = sfProjs
    CsProjList = csProjs 
}

let parseSolution path =
    let absPathGetter (p: ProjectInSolution) = p.AbsolutePath
    try
        let sfProjs = SolutionFile.Parse(path).ProjectsInOrder
                      |> Seq.map absPathGetter
                      |> Seq.filter (String.endsWith "sfproj")
                      |> Seq.toList
        let csProjs = SolutionFile.Parse(path).ProjectsInOrder
                      |> Seq.map absPathGetter
                      |> Seq.filter (String.endsWith "sfproj")
                      |> Seq.toList

        buildResult sfProjs csProjs |> Ok

    with e -> Error e
