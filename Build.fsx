#load "node_modules/fable-publish-utils/PublishUtils.fs"
open PublishUtils

run "npm install && npm test"
match args with
| IgnoreCase "publish"::_ ->
    pushFableNuget "src/Fable.Promise.fsproj" [] doNothing
| _ -> ()
