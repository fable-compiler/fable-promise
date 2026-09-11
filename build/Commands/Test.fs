module EasyBuild.Commands.Test

open Spectre.Console.Cli
open SimpleExec
open BlackFox.CommandLine
open EasyBuild.Workspace

type TestSettings() =
    inherit CommandSettings()

    [<CommandOption("-w|--watch")>]
    member val IsWatch = false with get, set

type TestCommand() =
    inherit Command<TestSettings>()
    interface ICommandLimiter<TestSettings>

    override __.Execute(_, settings, _) =
        let args =
            CmdLine.empty
            |> CmdLine.appendRaw "fable"
            |> CmdLine.appendIf settings.IsWatch "--watch"
            |> CmdLine.appendRaw "--runScript"
            |> CmdLine.toString

        Command.Run("dotnet", args, workingDirectory = Workspace.tests.``.``)

        0
