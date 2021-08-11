module SFConfigManager.Core.Test.TestCommons

open System.IO
open System

type TempFile() =
    let path = Path.GetTempFileName()
    member _.Path = path

    interface IDisposable with
        member _.Dispose() = File.Delete(path)
