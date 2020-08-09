namespace SFConfigManager.Core

module SolutionParser =

    open Microsoft.Build.Construction

    type SolutionParseResult = { SfProjList: string list }

    let parseSolution path =
        try
            SolutionFile.Parse(path).ProjectsInOrder
            |> Seq.filter (fun p -> p.RelativePath.EndsWith("sfproj"))
            |> Seq.map (fun p -> p.AbsolutePath)
            |> Seq.toList
            |> fun s -> { SfProjList = s }
            |> Ok
        with e -> Error e