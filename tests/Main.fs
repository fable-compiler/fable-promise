module Main

open type Scriptorium.Quill.Runner

[<EntryPoint>]
let main _ =
    runTests
        [
            PromiseTests.tests
            PromiseLikeTests.tests
            AsyncIterableTests.tests
        ]
