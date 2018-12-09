module Main

open Util

run "npm test"
match args with
| IgnoreCase "publish"::_ ->
    pushNuget "src/Fable.Promise.fsproj"
| _ -> ()
