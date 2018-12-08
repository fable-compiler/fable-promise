module Main

open Fable.Core.JsInterop

let shell: ShellJs.IExports = importAll "shelljs"

let (!>) x = ignore x

[<EntryPoint>]
let main args =
    !> shell.echo("foo", "bar")
    printfn "Hello %s!" args.[0]
    1

