module Main

#r "node_modules/fable-compiler-js/dist/metadata2/Fable.Core.dll"
#load "node_modules/fable-publish-utils/PublishUtils.fs"

open PublishUtils

run "npm test"
match args with
| IgnoreCase "publish"::_ ->
    pushNuget "src/Fable.Promise.fsproj"
| _ -> ()
