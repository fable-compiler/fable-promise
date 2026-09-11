module PromiseTests

open System
open Fable.Core
open Scriptorium.Nib.Assertion
open type Scriptorium.Quill.Test

type DisposableAction(f) =
    interface IDisposable with
        member __.Dispose() = f ()

let tests =
    testList (
        "Promise",
        [
            testAsync (
                "Simple promise translates without exception",
                async { do! promise { return () } |> awaitPromise }
            )

            testAsync (
                "Promise.map works",
                async {
                    do!
                        Promise.lift "Hello"
                        |> Promise.map (fun x -> assertThat x.Length (isEqualTo 5))
                        |> awaitPromise
                }
            )

            testAsync (
                "PromiseBuilder.Combine works",
                async {
                    let nums = [| 1; 2; 3; 4; 5 |]

                    do!
                        promise {
                            let mutable xs = []

                            for x in nums do
                                let x = x + 1

                                if x < 5 then
                                    xs <- x :: xs

                            return xs
                        }
                        |> Promise.map (fun xs -> assertThat xs (isEqualTo [ 4; 3; 2 ]))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise for binding works correctly",
                async {
                    let inputs = [| 1; 2; 3 |]
                    let mutable result = 0

                    do!
                        promise {
                            for inp in inputs do
                                result <- result + inp
                        }
                        |> Promise.map (fun () -> assertThat result (isEqualTo 6))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise while binding works correctly",
                async {
                    let mutable result = 0

                    do!
                        promise {
                            while result < 10 do
                                result <- result + 1
                        }
                        |> Promise.map (fun () -> assertThat result (isEqualTo 10))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise exceptions are handled correctly",
                async {
                    let mutable result = 0

                    let f shouldThrow =
                        promise {
                            try
                                if shouldThrow then
                                    failwith "boom!"
                                else
                                    result <- 12
                            with _ ->
                                result <- 10
                        }
                        |> Promise.map (fun () -> result)

                    do!
                        promise {
                            let! x = f true
                            let! y = f false
                            return x + y
                        }
                        |> Promise.map (fun total -> assertThat total (isEqualTo 22))
                        |> awaitPromise
                }
            )

            testAsync (
                "Simple promise is executed correctly",
                async {
                    let mutable result = false
                    let x = promise { return 99 }

                    do!
                        promise {
                            let! x = x
                            let y = 99
                            result <- x = y
                        }
                        |> Promise.map (fun () -> assertThat result isTrue)
                        |> awaitPromise
                }
            )

            testAsync (
                "promise use statements should dispose of resources when they go out of scope",
                async {
                    let mutable isDisposed = false
                    let mutable step1ok = false
                    let mutable step2ok = false

                    let resource =
                        promise { return new DisposableAction(fun () -> isDisposed <- true) }

                    do!
                        promise {
                            use! r = resource
                            step1ok <- not isDisposed
                        }
                        |> Promise.map (fun () ->
                            step2ok <- isDisposed
                            assertThat (step1ok && step2ok) isTrue
                        )
                        |> awaitPromise
                }
            )

            testAsync (
                "Try ... with ... expressions inside promise expressions work the same",
                async {
                    let mutable result = ""
                    let throw () : unit = raise (exn "Boo!")
                    let append x = result <- result + x

                    let innerPromise () =
                        promise {
                            append "b"

                            try
                                append "c"
                                throw ()
                                append "1"
                            with _ ->
                                append "d"

                            append "e"
                        }

                    do!
                        promise {
                            append "a"

                            try
                                do! innerPromise ()
                            with _ ->
                                append "2"

                            append "f"
                        }
                        |> Promise.map (fun () -> assertThat result (isEqualTo "abcdef"))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise try .. with returns correctly from 'with' branch",
                async {
                    let work =
                        promise {
                            try
                                failwith "testing"
                                return -1
                            with e ->
                                return 42
                        }

                    do! work |> Promise.map (fun value -> assertThat value (isEqualTo 42)) |> awaitPromise
                }
            )

            todo "Deep recursion with promise doesn't cause stack overflow"

            testAsync (
                "Nested failure propagates in promise expressions",
                async {
                    do!
                        promise {
                            let mutable data = ""

                            let f1 x =
                                promise {
                                    try
                                        failwith "1"
                                        return x
                                    with e ->
                                        return! failwith ("2 " + e.Message.Trim('"'))
                                }

                            let f2 x =
                                promise {
                                    try
                                        return! f1 x
                                    with e ->
                                        return! failwith ("3 " + e.Message.Trim('"'))
                                }

                            let f () =
                                promise {
                                    try
                                        let! y = f2 4
                                        return ()
                                    with e ->
                                        data <- e.Message.Trim('"')
                                }

                            do! f ()
                            do! Promise.sleep 100
                            assertThat data (isEqualTo "3 2 1")
                        }
                        |> awaitPromise
                }
            )

            testAsync (
                "Try .. finally expressions inside promise expressions work",
                async {
                    do!
                        promise {
                            let mutable data = ""

                            do!
                                promise {
                                    try
                                        data <- data + "1 "
                                    finally
                                        data <- data + "2 "
                                }

                            do!
                                promise {
                                    try
                                        try
                                            failwith "boom!"
                                        finally
                                            data <- data + "3"
                                    with _ ->
                                        ()
                                }

                            do! Promise.sleep 100
                            assertThat data (isEqualTo "1 2 3")
                        }
                        |> awaitPromise
                }
            )

            testAsync (
                "Final statement inside promise expressions can throw",
                async {
                    do!
                        promise {
                            let mutable data = ""

                            let f () =
                                promise {
                                    try
                                        data <- data + "1 "
                                    finally
                                        failwith "boom!"
                                }

                            do!
                                promise {
                                    try
                                        do! f ()
                                        return ()
                                    with e ->
                                        data <- data + e.Message.Trim('"')
                                }

                            do! Promise.sleep 100
                            assertThat data (isEqualTo "1 boom!")
                        }
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.Bind propagates exceptions",
                async {
                    do!
                        promise {
                            let task2 name =
                                promise {
                                    do! Promise.sleep 100

                                    if name = "fail" then
                                        failwith "Invalid access credentials"

                                    return "Ok"
                                }

                            let doWork name task =
                                promise {
                                    let! b = task "fail" |> Promise.catch (fun ex -> ex.Message)
                                    return b
                                }

                            let! res2 = doWork "task2" task2
                            assertThat res2 (isEqualTo "Invalid access credentials")
                        }
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.catchBind takes a Promise-returning function",
                async {
                    do!
                        promise {
                            let pr =
                                promise {
                                    failwith "Boo!"
                                    return "Ok"
                                }

                            let exHandler (e: exn) = promise { return e.Message }

                            let! res = pr |> Promise.catchBind exHandler
                            assertThat res (isEqualTo "Boo!")
                        }
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.either can take all combinations of value-returning and Promise-returning continuations",
                async {
                    do!
                        promise {
                            let failing = promise { failwith "Boo!" }
                            let successful = Promise.lift 42

                            let! r1 =
                                successful
                                |> Promise.either (fun x -> string x) (fun x -> failwith "Shouldn't get called")

                            let! r2 =
                                successful
                                |> Promise.eitherBind
                                    (fun n -> string n |> Promise.lift)
                                    (fun x -> failwith "Shouldn't get called")

                            let! r3 =
                                failing
                                |> Promise.either
                                    (fun x -> failwith "Shouldn't get called")
                                    (fun (ex: Exception) -> ex.Message)

                            let! r4 =
                                failing
                                |> Promise.eitherBind
                                    (fun x -> failwith "Shouldn't get called")
                                    (fun (ex: Exception) -> Promise.lift ex.Message)

                            assertThat r1 (isEqualTo "42")
                            assertThat r2 (isEqualTo "42")
                            assertThat r3 (isEqualTo "Boo!")
                            assertThat r4 (isEqualTo "Boo!")
                        }
                        |> awaitPromise
                }
            )

            test (
                "Promise.start works",
                fun _ ->
                    promise {
                        printfn "    Promise started"
                        return 5
                    }
                    |> Promise.start
            )

            testAsync (
                "Promise mapResultError works correctly",
                async {
                    do!
                        Result.Error "foo"
                        |> Promise.lift
                        |> Promise.mapResultError (fun (msg: string) -> 666)
                        |> Promise.map (fun exnRes ->
                            match exnRes with
                            | Ok _ -> failwith "Shouldn't get called"
                            | Error e -> assertThat e (isEqualTo 666)
                        )
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.bindResult works",
                async {
                    let multiplyBy2 (value: int) =
                        Promise.create (fun resolve reject -> resolve (value * 2))

                    do!
                        Promise.lift 42
                        |> Promise.result
                        |> Promise.bindResult (fun value -> multiplyBy2 value)
                        |> Promise.tap (fun result -> assertThat result (isEqualTo (Ok(42 * 2))))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.tap passes original value through to next transform",
                async {
                    do!
                        Promise.lift 5
                        |> Promise.tap (fun x -> assertThat x (isEqualTo 5))
                        |> Promise.map (fun x -> assertThat x (isEqualTo 5))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.Parallel works",
                async {
                    let p1 =
                        promise {
                            do! Promise.sleep 100
                            return 1
                        }

                    let p2 =
                        promise {
                            do! Promise.sleep 200
                            return 2
                        }

                    let p3 =
                        promise {
                            do! Promise.sleep 300
                            return 3
                        }

                    do!
                        Promise.Parallel [ p1; p2; p3 ]
                        |> Promise.map (fun res -> assertThat (List.ofArray res) (isEqualTo [ 1; 2; 3 ]))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.all works",
                async {
                    let p1 =
                        promise {
                            do! Promise.sleep 100
                            return 1
                        }

                    let p2 =
                        promise {
                            do! Promise.sleep 200
                            return 2
                        }

                    let p3 =
                        promise {
                            do! Promise.sleep 300
                            return 3
                        }

                    do!
                        Promise.all [ p1; p2; p3 ]
                        |> Promise.map (fun res -> assertThat (List.ofArray res) (isEqualTo [ 1; 2; 3 ]))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.allSettled works",
                async {
                    let success =
                        promise {
                            do! Promise.sleep 100
                            return 1
                        }

                    let rejection = Promise.reject (exn "I Failed")

                    do!
                        Promise.allSettled [ success; rejection ]
                        |> Promise.map (fun values ->
                            let success = values.[0]
                            let rejection = values.[1]
                            assertThat success.value.Value (isEqualTo 1)
                            assertThat rejection.reason.Value.Message (isEqualTo "I Failed")
                        )
                        |> awaitPromise
                }
            )

            testAsync (
                "SettledValue.toResult works",
                async {
                    let success =
                        promise {
                            do! Promise.sleep 100
                            return 1
                        }

                    let rejection = Promise.reject (exn "I Failed")

                    do!
                        Promise.allSettled [ success; rejection ]
                        |> Promise.map (fun results ->
                            let success = results.[0]
                            let rejection = results.[1]

                            match success |> SettledValue.toResult with
                            | Ok value -> assertThat value (isEqualTo 1)
                            | _ -> failwith "Unreachable result"

                            match rejection |> SettledValue.toResult with
                            | Error ex -> assertThat ex.Message (isEqualTo "I Failed")
                            | _ -> failwith "Unreachable result"
                        )
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.any works",
                async {
                    let success () =
                        promise {
                            do! Promise.sleep 100
                            return 1
                        }

                    let rejection () = Promise.reject (exn "I Failed")

                    let rejection2 () =
                        promise {
                            do! Promise.sleep 50
                            return! Promise.reject (exn "I Failed")
                        }

                    do!
                        Promise.any [ rejection (); rejection2 (); success () ]
                        |> Promise.map (fun result -> assertThat result (isEqualTo 1))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.race resolves first",
                async {
                    let success () =
                        promise {
                            do! Promise.sleep 5
                            return 1
                        }

                    let rejection () =
                        promise {
                            do! Promise.sleep 10
                            return! Promise.reject (exn "I Failed First")
                        }

                    let rejection2 () =
                        promise {
                            do! Promise.sleep 50
                            return! Promise.reject (exn "I Failed Last")
                        }

                    do!
                        Promise.race [ rejection2 (); rejection (); success () ]
                        |> Promise.map (fun result -> assertThat result (isEqualTo 1))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.race fails first",
                async {
                    let success () =
                        promise {
                            do! Promise.sleep 100
                            return 1
                        }

                    let rejection () =
                        promise {
                            do! Promise.sleep 10
                            return! Promise.reject (exn "I Failed First")
                        }

                    let rejection2 () =
                        promise {
                            do! Promise.sleep 50
                            return! Promise.reject (exn "I Failed Last")
                        }

                    do!
                        Promise.race [ rejection2 (); rejection (); success () ]
                        |> Promise.map (fun _ -> failwith "Unreachable result")
                        |> Promise.catch (fun result -> assertThat result.Message (isEqualTo "I Failed First"))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.result maps to Result.Ok in case of success",
                async {
                    do!
                        Promise.lift 42
                        |> Promise.result
                        |> Promise.tap (fun result -> assertThat result (isEqualTo (Ok 42)))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise.result maps to Result.Error in case of error",
                async {
                    do!
                        Promise.reject (exn "Invalid value")
                        |> Promise.result
                        |> Promise.tap (fun result ->
                            let msg =
                                match result with
                                | Ok _ -> ""
                                | Error e -> e.Message

                            assertThat msg (isEqualTo "Invalid value")
                        )
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise can be run in parallel with andFor extension",
                async {
                    let one = Promise.lift 1
                    let two = Promise.lift 2

                    do!
                        promise {
                            for a in one do
                                andFor b in two
                                return a + b
                        }
                        |> Promise.tap (fun result -> assertThat result (isEqualTo 3))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise can be run in parallel with and!",
                async {
                    let one = Promise.lift 1
                    let two = Promise.lift 2

                    do!
                        promise {
                            let! a = one
                            and! b = two
                            return a + b
                        }
                        |> Promise.tap (fun result -> assertThat result (isEqualTo 3))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise can run multiple tasks in parallel with and!",
                async {
                    let mutable s = ""

                    let doWork ms letter =
                        promise {
                            do! Promise.sleep ms
                            s <- s + letter
                            return letter
                        }

                    let one = doWork 1000 "a"
                    let two = doWork 500 "b"
                    let three = doWork 200 "c"

                    do!
                        promise {
                            let! a = one
                            and! b = two
                            and! c = three
                            return a + b + c
                        }
                        |> Promise.tap (fun result ->
                            assertThat result (isEqualTo "abc")
                            assertThat s (isEqualTo "cba")
                        )
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise can run multiple tasks in parallel with andFor extension",
                async {
                    let one = Promise.lift 1
                    let two = Promise.lift 2
                    let three = Promise.lift 3

                    do!
                        promise {
                            for a in one do
                                andFor b in two
                                andFor c in three
                                return a + b = c
                        }
                        |> Promise.tap (fun result -> assertThat result isTrue)
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise does not re-execute multiple times",
                async {
                    let mutable promiseExecutionCount = 0

                    let p =
                        promise {
                            promiseExecutionCount <- promiseExecutionCount + 1
                            return 1
                        }

                    p.``then`` (fun _ -> assertThat promiseExecutionCount (isEqualTo 1)) |> ignore
                    p.``then`` (fun _ -> assertThat promiseExecutionCount (isEqualTo 1)) |> ignore

                    do!
                        p.``then`` (fun _ -> assertThat promiseExecutionCount (isEqualTo 1))
                        |> awaitPromise
                }
            )

            testAsync (
                "Promise is hot",
                async {
                    let mutable promiseExecutionCount = 0

                    let _ =
                        promise {
                            promiseExecutionCount <- promiseExecutionCount + 1
                            return 1
                        }

                    let delayed =
                        Promise.create (fun ok er -> JS.setTimeout (fun () -> ok ()) 10 |> ignore)

                    do!
                        delayed.``then`` (fun _ -> assertThat promiseExecutionCount (isEqualTo 1))
                        |> awaitPromise
                }
            )
        ]
    )
