module SFConfigManager.Core.Parsers.SolutionParser

open Microsoft.Build.Construction
open FSharpPlus
open SFConfigManager.Data.Parsers.ParserTypes
open System.IO

let private buildResult sfProjs = { SfProjList = sfProjs }

let parseSolution path =
    let absPathGetter (p: ProjectInSolution) = p.AbsolutePath
    let absPath = Path.GetFullPath(path)

    try
        let sfProjs =
            SolutionFile.Parse(absPath).ProjectsInOrder
            |> Seq.map absPathGetter
            |> Seq.filter (String.endsWith "sfproj")
            |> Seq.toList

        buildResult sfProjs |> Ok

    with
    | e -> Error e
