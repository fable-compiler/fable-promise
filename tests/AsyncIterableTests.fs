module AsyncIterableTests

open Fable.Core
open Scriptorium.Nib.Assertion
open type Scriptorium.Quill.Test

[<ImportMember("./AsyncIterable.js")>]
let asyncGenerator () : JS.AsyncIterable<int> = jsNative

[<ImportMember("./AsyncIterable.js")>]
let handleAsyncIterable (iterable: JS.AsyncIterable<char>) : JS.Promise<string> = jsNative

[<ImportMember("./AsyncIterable.js")>]
let handleAsyncIterableWithBreak (iterable: JS.AsyncIterable<char>) : JS.Promise<string> = jsNative

let tests =
    testList (
        "AsyncIterable",
        [
            testAsync (
                "Can iterate AsyncIterable",
                async {
                    let mutable acc = 0

                    do!
                        asyncGenerator ()
                        |> AsyncIterable.iter (fun _ i -> acc <- acc + i)
                        |> Promise.tap (fun () -> assertThat acc (isEqualTo 10))
                        |> awaitPromise
                }
            )

            testAsync (
                "Can cancel AsyncIterable",
                async {
                    let mutable acc = 0

                    do!
                        asyncGenerator ()
                        |> AsyncIterable.iter (fun token i ->
                            if i = 3 then
                                token.Cancel()

                            acc <- acc + i
                        )
                        |> Promise.tap (fun () -> assertThat acc (isEqualTo 3))
                        |> awaitPromise
                }
            )

            testAsync (
                "Can error AsyncIterable",
                async {
                    do!
                        asyncGenerator ()
                        |> AsyncIterable.iter (fun _ i ->
                            if i = 3 then
                                failwith "Oh, no!"
                        )
                        |> Promise.either (fun _ -> "unexpected") (fun e -> e.Message)
                        |> Promise.tap (fun message -> assertThat message (isEqualTo "Oh, no!"))
                        |> awaitPromise
                }
            )

            testAsync (
                "Can create AsyncIterable",
                async {
                    let mutable i = -1
                    let chars = "abcd".ToCharArray()

                    do!
                        AsyncIterable.create (fun () ->
                            i <- i + 1
                            Array.tryItem i chars |> Promise.lift
                        )
                        |> handleAsyncIterable
                        |> Promise.tap (fun value -> assertThat value (isEqualTo "dcba"))
                        |> awaitPromise
                }
            )

            testAsync (
                "Created AsyncIterable can do cleaning on interruption",
                async {
                    let mutable i = -1
                    let chars = "abcd".ToCharArray()
                    let mutable result = "dirty"

                    do!
                        AsyncIterable.createCancellable
                            (fun () -> result <- "clean")
                            (fun () ->
                                i <- i + 1
                                Array.tryItem i chars |> Promise.lift
                            )
                        |> handleAsyncIterableWithBreak
                        |> Promise.tap (fun value ->
                            assertThat result (isEqualTo "clean")
                            assertThat value (isEqualTo "ba")
                        )
                        |> awaitPromise
                }
            )
        ]
    )
