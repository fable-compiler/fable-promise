module EasyBuild.Main

open Spectre.Console.Cli
open EasyBuild.Commands.Test
open EasyBuild.Commands.Release
open EasyBuild.Tools.Husky

[<EntryPoint>]
let main args =

    if System.Environment.GetEnvironmentVariable("ACT") = null then
        Husky.install ()

    let app = CommandApp()

    app.Configure(fun config ->
        config.Settings.ApplicationName <- "./build.sh"

        config
            .AddCommand<TestCommand>("test")
            .WithDescription("Run the tests")
            .WithExample("test")
            .WithExample("test --watch")
        |> ignore

        config
            .AddCommand<ReleaseCommand>("release")
            .WithDescription("Pack and push the NuGet package")
        |> ignore
    )

    app.Run(args)
