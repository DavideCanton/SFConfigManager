module SFConfigManager.Extensions.OptionExtensions

module Option =
    let peek fn opt =
        match opt with
        | Some v -> fn v
        | None -> ()
        opt
