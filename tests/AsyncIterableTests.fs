module AsyncIterableTests

open Fable.Core

[<ImportMember("./AsyncIterable.js")>]
let asyncGenerator(): JS.AsyncIterable<int> = jsNative

[<ImportMember("./AsyncIterable.js")>]
let handleAsyncIterable(iterable: JS.AsyncIterable<char>): JS.Promise<string> = jsNative

[<ImportMember("./AsyncIterable.js")>]
let handleAsyncIterableWithBreak(iterable: JS.AsyncIterable<char>): JS.Promise<string> = jsNative

describe "AsyncIterable tests" <| fun _ ->

    it "Can iterate AsyncIterable" <| fun () ->
        let mutable acc = 0

        asyncGenerator()
        |> AsyncIterable.iter (fun _ i ->
            acc <- acc + i)
        |> Promise.tap (fun () ->
            equal 10 acc)

    it "Can cancel AsyncIterable" <| fun () ->
        let mutable acc = 0

        asyncGenerator()
        |> AsyncIterable.iter (fun token i ->
            if i = 3 then
                token.Cancel()
            acc <- acc + i)
        |> Promise.tap (fun () ->
            equal 3 acc)

    it "Can error AsyncIterable" <| fun () ->
        let mutable acc = 0

        asyncGenerator()
        |> AsyncIterable.iter (fun _ i ->
            if i = 3 then
                failwith "Oh, no!"
            acc <- acc + i)

        |> Promise.either
            (fun _ -> "unexpected")
            (fun e -> e.Message)

        |> Promise.tap (equal "Oh, no!")

    it "Can create AsyncIterable" <| fun () ->
        let mutable i = -1
        let chars = "abcd".ToCharArray()
        AsyncIterable.create (fun () ->
            i <- i + 1
            Array.tryItem i chars
            |> Promise.lift
        )
        |> handleAsyncIterable
        |> Promise.tap (equal "dcba")

    it "Created AsyncIterable can do cleaning on interruption" <| fun () ->
        let mutable i = -1
        let chars = "abcd".ToCharArray()
        let mutable result = "dirty"
        AsyncIterable.createCancellable
            (fun () -> result <- "clean")
            (fun () ->
                i <- i + 1
                Array.tryItem i chars
                |> Promise.lift
            )
        |> handleAsyncIterableWithBreak
        |> Promise.tap (fun value ->
            equal "clean" result
            equal "ba" value)
