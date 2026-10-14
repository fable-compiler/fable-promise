module AsyncIterableBuilderTests

open System
open Fable.Core
open Scriptorium.Nib.Assertion
open type Scriptorium.Quill.Test

[<ImportMember("./AsyncIterable.js")>]
let asyncGenerator () : JS.AsyncIterable<int> = jsNative

[<ImportMember("./AsyncIterable.js")>]
let toArray (iterable: JS.AsyncIterable<'T>) : JS.Promise<'T array> = jsNative

[<ImportMember("./AsyncIterable.js")>]
let take (count: int) (iterable: JS.AsyncIterable<'T>) : JS.Promise<'T array> = jsNative

[<AttachMembers>]
type private Counter(start: int) =
    member _.Start = start

    member this.Values() =
        asyncIterable {
            yield this.Start
            do! Promise.sleep 1
            yield this.Start + 1
        }

let private expectValues (expected: 'T list) (iterable: JS.AsyncIterable<'T>) =
    iterable
    |> toArray
    |> Promise.tap (fun values -> assertThat (List.ofArray values) (isEqualTo expected))
    |> awaitPromise

let tests =
    testList (
        "AsyncIterable computation expression",
        [
            testAsync (
                "yield produces values in order",
                asyncIterable {
                    yield 1
                    yield 2
                    yield 3
                }
                |> expectValues
                    [
                        1
                        2
                        3
                    ]
            )

            testAsync (
                "Zero produces no values",
                asyncIterable {
                    if false then
                        yield 1
                }
                |> expectValues []
            )

            testAsync (
                "let! and do! await promises",
                asyncIterable {
                    let! a = Promise.lift 1
                    yield a
                    do! Promise.sleep 10
                    let! b = Promise.sleep 10 |> Promise.map (fun () -> 2)
                    yield b
                }
                |> expectValues
                    [
                        1
                        2
                    ]
            )

            testAsync (
                "and! awaits promises in parallel",
                async {
                    let mutable order = ""

                    let work ms letter =
                        promise {
                            do! Promise.sleep ms
                            order <- order + letter
                            return letter
                        }

                    do!
                        asyncIterable {
                            let! a = work 30 "a"
                            and! b = work 10 "b"
                            yield a + b

                            let! c = work 30 "c"
                            and! d = work 20 "d"
                            and! e = work 10 "e"
                            yield c + d + e
                        }
                        |> expectValues
                            [
                                "ab"
                                "cde"
                            ]

                    assertThat order (isEqualTo "baedc")
                }
            )

            testAsync (
                "for iterates over a seq",
                asyncIterable {
                    for i in 1..3 do
                        do! Promise.sleep 1
                        yield i * 10

                    for c in
                        [
                            'a'
                            'b'
                        ] do
                        yield int c
                }
                |> expectValues
                    [
                        10
                        20
                        30
                        97
                        98
                    ]
            )

            testAsync (
                "for iterates over an AsyncIterable",
                asyncIterable {
                    for i in asyncGenerator () do
                        yield i * 2
                }
                |> expectValues
                    [
                        0
                        2
                        4
                        6
                        8
                    ]
            )

            testAsync (
                "yield! inlines another AsyncIterable",
                asyncIterable {
                    yield 0

                    yield!
                        asyncIterable {
                            yield 1
                            yield 2
                        }

                    yield! asyncGenerator ()
                }
                |> expectValues
                    [
                        0
                        1
                        2
                        0
                        1
                        2
                        3
                        4
                    ]
            )

            testAsync (
                "yield! supports recursion",
                let rec countdown n =
                    asyncIterable {
                        if n > 0 then
                            yield n
                            yield! countdown (n - 1)
                    }

                countdown 3
                |> expectValues
                    [
                        3
                        2
                        1
                    ]
            )

            testAsync (
                "while loops until the guard is false",
                asyncIterable {
                    let mutable i = 0

                    while i < 3 do
                        do! Promise.sleep 1
                        yield i
                        i <- i + 1
                }
                |> expectValues
                    [
                        0
                        1
                        2
                    ]
            )

            testAsync (
                "try/with catches errors raised after values were yielded",
                asyncIterable {
                    try
                        yield 1
                        do! Promise.sleep 1
                        failwith "boom"
                        yield 2
                    with e ->
                        yield -1
                }
                |> expectValues
                    [
                        1
                        -1
                    ]
            )

            testAsync (
                "try/with catches rejected promises",
                asyncIterable {
                    try
                        let! _ = Promise.reject<int>(exn "boom")
                        yield 1
                    with e ->
                        yield e.Message.Length
                }
                |> expectValues [ 4 ]
            )

            testAsync (
                "Errors reject the consumer",
                asyncIterable {
                    yield 1
                    failwith "Oh, no!"
                }
                |> toArray
                |> Promise.either (fun _ -> "unexpected") (fun e -> e.Message)
                |> Promise.tap (fun message -> assertThat message (isEqualTo "Oh, no!"))
                |> awaitPromise
            )

            testAsync (
                "try/finally runs on completion",
                async {
                    let mutable finalized = false

                    do!
                        asyncIterable {
                            try
                                yield 1
                            finally
                                finalized <- true
                        }
                        |> expectValues [ 1 ]

                    assertThat finalized (isEqualTo true)
                }
            )

            testAsync (
                "try/finally runs when the consumer breaks",
                async {
                    let mutable finalized = false
                    let mutable resumed = false

                    do!
                        asyncIterable {
                            try
                                yield 1
                                yield 2
                                resumed <- true
                                yield 3
                            finally
                                finalized <- true
                        }
                        |> take 2
                        |> Promise.tap (fun values ->
                            assertThat
                                (List.ofArray values)
                                (isEqualTo
                                    [
                                        1
                                        2
                                    ])
                        )
                        |> awaitPromise

                    assertThat finalized (isEqualTo true)
                    assertThat resumed (isEqualTo false)
                }
            )

            testAsync (
                "use disposes the resource when the consumer breaks",
                async {
                    let mutable disposed = false

                    do!
                        asyncIterable {
                            use _ =
                                { new IDisposable with
                                    member _.Dispose() = disposed <- true
                                }

                            yield 1
                            yield 2
                        }
                        |> take 1
                        |> awaitPromise

                    assertThat disposed (isEqualTo true)
                }
            )

            testAsync (
                "for over an AsyncIterable closes the source when the consumer breaks",
                async {
                    let mutable i = 0
                    let mutable cancelled = false

                    let source =
                        AsyncIterable.createCancellable
                            (fun () -> cancelled <- true)
                            (fun () ->
                                i <- i + 1
                                Promise.lift (Some i)
                            )

                    do!
                        asyncIterable {
                            for value in source do
                                yield value
                        }
                        |> take 2
                        |> Promise.tap (fun values ->
                            assertThat
                                (List.ofArray values)
                                (isEqualTo
                                    [
                                        1
                                        2
                                    ])
                        )
                        |> awaitPromise

                    assertThat cancelled (isEqualTo true)
                }
            )

            testAsync (
                "for iterates over an array",
                asyncIterable {
                    for i in
                        [|
                            1
                            2
                            3
                        |] do
                        do! Promise.sleep 1
                        yield i * 10
                }
                |> expectValues
                    [
                        10
                        20
                        30
                    ]
            )

            testAsync (
                "for over a seq disposes its enumerator when the consumer breaks",
                async {
                    let mutable disposed = false

                    let source =
                        seq {
                            try
                                yield 1
                                yield 2
                                yield 3
                            finally
                                disposed <- true
                        }

                    do!
                        asyncIterable {
                            for value in source do
                                yield value
                        }
                        |> take 2
                        |> Promise.tap (fun values ->
                            assertThat
                                (List.ofArray values)
                                (isEqualTo
                                    [
                                        1
                                        2
                                    ])
                        )
                        |> awaitPromise

                    assertThat disposed (isEqualTo true)
                }
            )

            testAsync (
                "for over a seq disposes its enumerator when the seq completes",
                async {
                    let mutable disposed = false

                    let source =
                        seq {
                            try
                                yield 1
                            finally
                                disposed <- true
                        }

                    do!
                        asyncIterable {
                            for value in source do
                                yield value
                        }
                        |> toArray
                        |> awaitPromise

                    assertThat disposed (isEqualTo true)
                }
            )

            testAsync (
                "for over a seq reports an error from Dispose over the body's error",
                asyncIterable {
                    for value in
                        seq {
                            try
                                yield 1
                            finally
                                failwith "dispose failed"
                        } do
                        if value = 1 then
                            failwith "body failed"

                        yield value
                }
                |> toArray
                |> Promise.either (fun _ -> "unexpected") (fun e -> e.Message)
                |> Promise.tap (fun message -> assertThat message (isEqualTo "dispose failed"))
                |> awaitPromise
            )

            testAsync (
                "for over a seq disposes its enumerator when the body throws",
                async {
                    let mutable disposed = false

                    let source =
                        seq {
                            try
                                yield 1
                                yield 2
                            finally
                                disposed <- true
                        }

                    do!
                        asyncIterable {
                            for value in source do
                                if value = 2 then
                                    failwith "boom"

                                yield value
                        }
                        |> toArray
                        |> Promise.either (fun _ -> "unexpected") (fun e -> e.Message)
                        |> Promise.tap (fun message -> assertThat message (isEqualTo "boom"))
                        |> awaitPromise

                    assertThat disposed (isEqualTo true)
                }
            )

            testAsync (
                "Body does not run before iteration",
                async {
                    let mutable started = false

                    let iterable =
                        asyncIterable {
                            started <- true
                            yield 1
                        }

                    assertThat started (isEqualTo false)
                    do! iterable |> expectValues [ 1 ]
                    assertThat started (isEqualTo true)
                }
            )

            testAsync (
                "Each iteration restarts the body",
                async {
                    let mutable runs = 0

                    let iterable =
                        asyncIterable {
                            runs <- runs + 1

                            for i in 1..2 do
                                yield i
                        }

                    do!
                        iterable
                        |> expectValues
                            [
                                1
                                2
                            ]

                    do!
                        iterable
                        |> expectValues
                            [
                                1
                                2
                            ]

                    assertThat runs (isEqualTo 2)
                }
            )

            testAsync (
                "Can be used inside a class member",
                Counter(5).Values()
                |> expectValues
                    [
                        5
                        6
                    ]
            )

            testAsync (
                "Can be consumed with AsyncIterable.iter",
                async {
                    let mutable acc = 0

                    do!
                        asyncIterable {
                            yield 1
                            yield 2
                            yield 3
                        }
                        |> AsyncIterable.iter (fun _ i -> acc <- acc + i)
                        |> awaitPromise

                    assertThat acc (isEqualTo 6)
                }
            )
        ]
    )
