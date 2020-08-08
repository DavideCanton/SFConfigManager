namespace SFConfigManager.Core

module SolutionParser =

    open Microsoft.Build.Construction

    let getSfProjs path =
        let sl = SolutionFile.Parse(path)
        sl.ProjectsInOrder
        |> Seq.filter (fun p -> p.RelativePath.EndsWith("sfproj"))
        |> Seq.map (fun p -> p.AbsolutePath)
        |> Seq.toList