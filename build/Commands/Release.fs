module EasyBuild.Commands.Release

open Spectre.Console.Cli
open EasyBuild.Workspace
open EasyBuild.Tools.DotNet

type ReleaseSettings() =
    inherit CommandSettings()

type ReleaseCommand() =
    inherit Command<ReleaseSettings>()
    interface ICommandLimiter<ReleaseSettings>

    override __.Execute(_, _, _) =
        let nupkgPath = DotNet.pack (workingDirectory = Workspace.src.``.``)

        DotNet.nugetPush (nupkgPath, skipDuplicate = true)

        0
