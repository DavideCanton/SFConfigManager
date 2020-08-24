module SFConfigManager.Core.Parsers.SolutionParser

open Microsoft.Build.Construction
open FSharpPlus
open SFConfigManager.Data.Parsers.ParserTypes

let private buildResult sfProjs = { SfProjList = sfProjs }

let parseSolution path =
    let absPathGetter (p: ProjectInSolution) = p.AbsolutePath
    try
        let sfProjs =
            SolutionFile.Parse(path).ProjectsInOrder
            |> Seq.map absPathGetter
            |> Seq.filter (String.endsWith "sfproj")
            |> Seq.toList

        buildResult sfProjs |> Ok

    with e -> Error e
