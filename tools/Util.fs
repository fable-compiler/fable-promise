module Util

open System
open System.Text.RegularExpressions
open Fable.Core.JsInterop
open Fable.Import
module Node = Fable.Import.Node.Exports
module NodeGlobals = Fable.Import.Node.Globals

module private Helpers =
    let readline: obj = importAll "readline"

    let inline (!>) x = ignore x
    let inline (~%) xs = createObj xs |> unbox

    type SingleObservable<'T>(?onDispose: unit->unit) =
        let mutable disposed = false
        let mutable listener: IObserver<'T> option = None
        member __.IsDisposed = disposed
        member __.Dispose() =
            if not disposed then
                onDispose |> Option.iter (fun d -> d())
                listener |> Option.iter (fun l -> l.OnCompleted())
                disposed <- true
                listener <- None
        member __.Trigger v =
            listener |> Option.iter (fun l -> l.OnNext v)
        interface IObservable<'T> with
            member this.Subscribe w =
                if disposed then failwith "Disposed"
                if Option.isSome listener then failwith "Busy"
                listener <- Some w
                { new IDisposable with
                    member __.Dispose() = this.Dispose() }

    let awaitWhileTrue (f: 'T->bool) (s: IObservable<'T>) =
        Async.FromContinuations <| fun (success,_,_) ->
            let mutable finished = false
            let mutable disp = Unchecked.defaultof<IDisposable>
            let observer =
                { new IObserver<'T> with
                    member __.OnNext v =
                        if not finished then
                            if not(f v) then
                                finished <- true
                                disp.Dispose()
                                success()
                    member __.OnError e = ()
                    member x.OnCompleted() =
                        success() }
            disp <- s.Subscribe(observer)

open Helpers

let (</>) p1 p2 = Node.path.join(p1, p2)

let args = NodeGlobals.``process``.argv |> Seq.skip 2 |> Seq.toList

let fullPath (path: string) =
  Node.path.resolve(path)

let dirname (path: string) =
  let parent = Node.path.dirname(path)
  if parent = path then null else parent

let dirFiles (path: string) =
    Node.fs.readdirSync(!^path).ToArray()

let isDirectory (path: string) =
    Node.fs.lstatSync(!^path).isDirectory()

let pathExists (path: string) =
    Node.fs.existsSync(!^path)

let filename (path: string) =
  Node.path.basename(path)

let filenameWithoutExtension (path: string) =
    let name = filename path
    let i = name.LastIndexOf(".")
    if i > -1 then name.Substring(0, i) else name

let rec removeDirRecursive (path: string) =
    if Node.fs.existsSync(!^path) then
        for file in Node.fs.readdirSync(!^path) do
            let curPath = path </> file
            if Node.fs.lstatSync(!^curPath).isDirectory() then
                removeDirRecursive curPath
            else
                Node.fs.unlinkSync(!^curPath)
        Node.fs.rmdirSync(!^path)

let writeFile (filePath: string) (txt: string) =
    Node.fs.writeFileSync(filePath, txt)

let readFile (filePath: string) =
    Node.fs.readFileSync(filePath).toString()

let readAllLines (filePath: string) =
    Node.fs.readFileSync(filePath).toString().Split('\n')

let readLines (filePath: string): IObservable<string> =
    let rl = readline?createInterface %[
        "input" ==> Node.fs.createReadStream(filePath)
        // Note: we use the crlfDelay option to recognize all instances of CR LF
        // ('\r\n') in input.txt as a single line break.
        "crlfDelay" ==> System.Double.PositiveInfinity
    ]
    let obs = SingleObservable(fun () -> rl?close())
    rl?on("line", fun line ->
        obs.Trigger(line))
    rl?on("close", fun line ->
        obs.Dispose())
    obs :> _

let takeLines (numLines: int) (filePath: string) = async {
    let mutable i = -1
    let lines = ResizeArray()
    do! readLines filePath
        |> awaitWhileTrue (fun line ->
            i <- i + 1
            if i < numLines then lines.Add(line); true
            else false)
    return lines.ToArray()
}

let takeLinesWhile (predicate: string->bool) (filePath: string) = async {
    let lines = ResizeArray()
    do! readLines filePath
        |> awaitWhileTrue (fun line ->
            if predicate line then lines.Add(line); true
            else false)
    return lines.ToArray()
}
let run cmd =
    printfn "> %s" cmd
    Node.childProcess.execSync(cmd, %[
        "stdio" ==> "inherit"
    ]) |> ignore

let runList cmdParts =
    String.concat " " cmdParts |> run

let environVarOrNone (varName: string): string option =
    NodeGlobals.``process``.env?(varName)
    |> Option.ofObj

open System.Text.RegularExpressions

let (|IgnoreCase|_|) (pattern: string) (input: string) =
    if String.Equals(input, pattern, StringComparison.OrdinalIgnoreCase) then
        Some IgnoreCase
    else None

let (|Regex|_|) (pattern: string) (input: string) =
    let m = Regex.Match(input, pattern)
    if m.Success then
        let mutable groups = []
        for i = m.Groups.Count - 1 downto 0 do
            groups <- m.Groups.[i].Value::groups
        Some groups
    else None

let replaceRegex (pattern: string) (replacement: string list) (input: string) =
    Regex.Replace(input, pattern, String.concat "" replacement)

module private Publish =
    let NUGET_VERSION = @"(<Version>)(.*?)(<\/Version>)"
    let NUGET_PACKAGE_VERSION = @"(<PackageVersion>)(.*?)(<\/PackageVersion>)"
    let NPM_VERSION = @"""version"":\s*""(.*?)"""
    let VERSION = @"\d+\.\d+\.\d+\S*"

    let splitPrerelease (version: string) =
        let i = version.IndexOf("-")
        if i > 0
        then version.Substring(0, i), Some(version.Substring(i + 1))
        else version, None

    let rec findFileUpwards fileName dir =
        let fullPath = dir </> fileName
        if pathExists fullPath
        then fullPath
        else
            let parent = dirname dir
            if isNull parent then
                failwithf "Couldn't find %s directory" fileName
            findFileUpwards fileName parent

    let loadReleaseVersion projFile =
        let projDir = if isDirectory projFile then projFile else dirname projFile
        let releaseNotes = findFileUpwards "RELEASE_NOTES.md" projDir
        match readFile releaseNotes with
        | Regex VERSION [version] -> version
        | _ -> failwithf "Couldn't find version in %s" releaseNotes

    let needsPublishing (checkPkgVersion: string->string option) (releaseVersion: string) projFile =
        let print msg =
            let projName =
                let projName = filename projFile
                if projName = "package.json"
                then dirname projFile |> filename
                else projName
            printfn "%s > %s" projName msg
        match readFile projFile |> checkPkgVersion with
        | None -> failwithf "Couldn't find package version in %s" projFile
        | Some version ->
            let sameVersion = version = releaseVersion
            if sameVersion then
                sprintf "Already version %s, no need to publish" releaseVersion |> print
            not sameVersion

    let pushNuget (projFile: string) =
        let checkPkgVersion = function
            | Regex NUGET_PACKAGE_VERSION [_;_;pkgVersion;_] -> Some pkgVersion
            | _ -> None
        let releaseVersion = loadReleaseVersion projFile
        if needsPublishing checkPkgVersion releaseVersion projFile then
            let projDir = dirname projFile
            let nugetKey =
                match environVarOrNone "NUGET_KEY" with
                | Some nugetKey -> nugetKey
                | None -> failwith "The Nuget API key must be set in a NUGET_KEY environmental variable"
            // Restore dependencies here so they're updated to latest project versions
            runList ["dotnet restore"; projDir]
            // Update the project file
            readFile projFile
            |> replaceRegex NUGET_VERSION ["$1"; splitPrerelease releaseVersion |> fst; "$3"]
            |> replaceRegex NUGET_PACKAGE_VERSION ["$1"; releaseVersion; "$3"]
            |> writeFile projFile
            try
                let tempDir = projDir </> "temp"
                removeDirRecursive tempDir
                runList ["dotnet pack"; projDir; "-c Release -o temp"]
                let pkgName = filenameWithoutExtension projFile
                let nupkg =
                    dirFiles tempDir
                    |> Seq.tryPick (fun path ->
                        if path.Contains(pkgName) then Some(tempDir </> path) else None)
                    |> function
                        | Some x -> x
                        | None -> failwithf "Cannot find .nupgk with name %s" pkgName
                runList ["dotnet nuget push"; nupkg; "-s nuget.org -k"; nugetKey]
                removeDirRecursive tempDir
            with _ ->
                filenameWithoutExtension projFile
                |> printfn "There's been an error when pushing project: %s"
                printfn "Please revert the version change in .fsproj"
                reraise()

let pushNuget projFile =
    Publish.pushNuget projFile
