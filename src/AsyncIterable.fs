[<RequireQualifiedAccess>]
module AsyncIterable

open System
open Fable.Core
open Fable.Core.JsInterop

[<Emit("""{
    [Symbol.asyncIterator]() {
        return {
            next: $0,
            return: $1
        }
    }
}""")>]
let private createAsyncIterator
    (onNext: unit -> JS.Promise<obj>)
    (onCancel: unit -> obj)
    : JS.AsyncIterable<'T>
    =
    jsNative

/// Creates AsyncIterable. See https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/for-await...of#specifications
let create (onNext: unit -> JS.Promise<'T option>) : JS.AsyncIterable<'T> =
    createAsyncIterator
        (fun () ->
            onNext ()
            |> Promise.map (
                function
                | Some value ->
                    createObj
                        [
                            "value" ==> value
                            "done" ==> false
                        ]
                | None -> createObj [ "done" ==> true ]
            )
        )
        (fun () -> createObj [ "done" ==> true ])

/// Creates AsyncIterable with a cleaning function for cancellation (JS caller invokes `break` or `return` during iteration)
let createCancellable
    (onCancel: unit -> unit)
    (onNext: unit -> JS.Promise<'T option>)
    : JS.AsyncIterable<'T>
    =
    createAsyncIterator
        (fun () ->
            onNext ()
            |> Promise.map (
                function
                | Some value ->
                    createObj
                        [
                            "value" ==> value
                            "done" ==> false
                        ]
                | None -> createObj [ "done" ==> true ]
            )
        )
        (fun () ->
            onCancel ()
            createObj [ "done" ==> true ]
        )

type CancellationToken() =
    member this.Cancel() : unit = raise !!this

/// Iterates AsyncIterable. See https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/for-await...of
let iter
    (action: CancellationToken -> 'T -> unit)
    (iterable: JS.AsyncIterable<'T>)
    : JS.Promise<unit>
    =
    let token = CancellationToken()

    emitJsExpr
        (action, iterable, token)
        """(async () => {
    for await (const value of $1) {
        try {
            $0($2, value)
        } catch (err) {
            if (err instanceof $2.constructor) {
                break;
            }
            throw(err);
        }
    }
})()"""

/// Intermediate value of the <c>asyncIterable</c> computation expression.
type Computation<'T> = interface end

[<Literal>]
let private YieldComputation =
    "if (Array.isArray(computation)) { for (let i = 0; i < computation.length; i++) { yield computation[i]; } } else { yield* computation; }"

[<Literal>]
let private RunTemplate =
    "async function* (generator) { const computation = generator(); "
    + YieldComputation
    + " }"

[<Literal>]
let private BindTemplate =
    "async function* (p, binder) { const computation = binder(await p); "
    + YieldComputation
    + " }"

[<Literal>]
let private CombineTemplate =
    "async function* (first, second) { { const computation = first; "
    + YieldComputation
    + " } { const computation = second(); "
    + YieldComputation
    + " } }"

[<Literal>]
let private WhileTemplate =
    "async function* (guard, body) { while (guard()) { const computation = body(); "
    + YieldComputation
    + " } }"

[<Literal>]
let private ForTemplate =
    "async function* (source, body) { for await (const item of source) { const computation = body(item); "
    + YieldComputation
    + " } }"

[<Literal>]
let private ArrayForTemplate =
    "async function* (source, body) { for (let index = 0; index < source.length; index++) { const computation = body(source[index]); "
    + YieldComputation
    + " } }"

// Template arguments:
// - $0: enumerate
// - $1: moveNext
// - $2: current
// - $3: dispose
//
// for...of drops an error thrown by return() when the body has thrown; F# lets Dispose's error win.
[<Literal>]
let private SeqForTemplate =
    "async function* (source, body) { const enumerator = $0(source); try { while ($1(enumerator)) { const computation = body($2(enumerator)); "
    + YieldComputation
    + " } } finally { $3(enumerator); } }"

[<Literal>]
let private TryWithTemplate =
    "async function* (body, handler) { try { const computation = body(); "
    + YieldComputation
    + " } catch (error) { const computation = handler(error); "
    + YieldComputation
    + " } }"

[<Literal>]
let private TryFinallyTemplate =
    "async function* (body, compensation) { try { const computation = body(); "
    + YieldComputation
    + " } finally { compensation(); } }"

let private runGenerator: obj = emitJsExpr () RunTemplate
let private bindGenerator: obj = emitJsExpr () BindTemplate
let private combineGenerator: obj = emitJsExpr () CombineTemplate
let private whileGenerator: obj = emitJsExpr () WhileTemplate
let private forGenerator: obj = emitJsExpr () ForTemplate
let private tryWithGenerator: obj = emitJsExpr () TryWithTemplate
let private tryFinallyGenerator: obj = emitJsExpr () TryFinallyTemplate
let private arrayForGenerator: obj = emitJsExpr () ArrayForTemplate

let private enumerate (source: seq<obj>) = source.GetEnumerator()
let private moveNext (enumerator: Collections.Generic.IEnumerator<obj>) = enumerator.MoveNext()
let private current (enumerator: Collections.Generic.IEnumerator<obj>) = enumerator.Current
let private dispose (enumerator: Collections.Generic.IEnumerator<obj>) = enumerator.Dispose()

let private seqForGenerator: obj =
    emitJsExpr (enumerate, moveNext, current, dispose) SeqForTemplate

/// JS arrays and typed arrays, which Fable uses for F# arrays and <c>ResizeArray</c>.
let private isNativeArray (source: obj) : bool =
    emitJsExpr source "Array.isArray($0) || ArrayBuffer.isView($0)"

/// <summary>
/// A builder to provide AsyncIterable Computation Expression support.
///
/// The CE is available via <c>asyncIterable { ... }</c>
/// <example>
/// <code lang="fsharp">
/// let numbers (count : int) =
///     asyncIterable {
///         for i in 1 .. count do
///             do! Promise.sleep 100
///             yield i
///     }
/// </code>
/// </example>
/// </summary>
type AsyncIterableBuilder() =
    [<Emit("[]")>]
    member _.Zero() : Computation<'T> = jsNative

    [<Emit("[$1]")>]
    member _.Yield(value: 'T) : Computation<'T> = jsNative

    [<Emit("$1")>]
    member _.YieldFrom(source: JS.AsyncIterable<'T>) : Computation<'T> = jsNative

    [<Emit("$1")>]
    member _.Delay(generator: unit -> Computation<'T>) : unit -> Computation<'T> = jsNative

    member _.Run(generator: unit -> Computation<'T>) : JS.AsyncIterable<'T> =
        emitJsExpr (runGenerator, generator) "({ [Symbol.asyncIterator]: () => $0($1) })"

    member _.Bind(p: JS.Promise<'T>, binder: 'T -> Computation<'U>) : Computation<'U> =
        emitJsExpr (bindGenerator, p, binder) "$0($1, $2)"

    [<Emit("Promise.all([$1, $2])")>]
    member _.MergeSources(a: JS.Promise<'T1>, b: JS.Promise<'T2>) : JS.Promise<'T1 * 'T2> = jsNative

    [<Emit("Promise.all([$1, $2, $3])")>]
    member _.MergeSources3
        (a: JS.Promise<'T1>, b: JS.Promise<'T2>, c: JS.Promise<'T3>)
        : JS.Promise<'T1 * 'T2 * 'T3>
        =
        jsNative

    [<Emit("Promise.all([$1, $2, $3, $4])")>]
    member _.MergeSources4
        (a: JS.Promise<'T1>, b: JS.Promise<'T2>, c: JS.Promise<'T3>, d: JS.Promise<'T4>)
        : JS.Promise<'T1 * 'T2 * 'T3 * 'T4>
        =
        jsNative

    [<Emit("Promise.all([$1, $2, $3, $4, $5])")>]
    member _.MergeSources5
        (
            a: JS.Promise<'T1>,
            b: JS.Promise<'T2>,
            c: JS.Promise<'T3>,
            d: JS.Promise<'T4>,
            e: JS.Promise<'T5>
        )
        : JS.Promise<'T1 * 'T2 * 'T3 * 'T4 * 'T5>
        =
        jsNative

    [<Emit("Promise.all([$1, $2, $3, $4, $5, $6])")>]
    member _.MergeSources6
        (
            a: JS.Promise<'T1>,
            b: JS.Promise<'T2>,
            c: JS.Promise<'T3>,
            d: JS.Promise<'T4>,
            e: JS.Promise<'T5>,
            f: JS.Promise<'T6>
        )
        : JS.Promise<'T1 * 'T2 * 'T3 * 'T4 * 'T5 * 'T6>
        =
        jsNative

    member _.Combine(first: Computation<'T>, second: unit -> Computation<'T>) : Computation<'T> =
        emitJsExpr (combineGenerator, first, second) "$0($1, $2)"

    member _.While(guard: unit -> bool, body: unit -> Computation<'T>) : Computation<'T> =
        emitJsExpr (whileGenerator, guard, body) "$0($1, $2)"

    member _.For(source: JS.AsyncIterable<'T>, body: 'T -> Computation<'U>) : Computation<'U> =
        emitJsExpr (forGenerator, source, body) "$0($1, $2)"

    member _.For(source: seq<'T>, body: 'T -> Computation<'U>) : Computation<'U> =
        if isNativeArray source then
            emitJsExpr (arrayForGenerator, source, body) "$0($1, $2)"
        else
            emitJsExpr (seqForGenerator, source, body) "$0($1, $2)"

    member _.TryWith
        (body: unit -> Computation<'T>, handler: exn -> Computation<'T>)
        : Computation<'T>
        =
        emitJsExpr (tryWithGenerator, body, handler) "$0($1, $2)"

    member _.TryFinally
        (body: unit -> Computation<'T>, compensation: unit -> unit)
        : Computation<'T>
        =
        emitJsExpr (tryFinallyGenerator, body, compensation) "$0($1, $2)"

    member this.Using<'T, 'U when 'T :> IDisposable>
        (resource: 'T, binder: 'T -> Computation<'U>)
        : Computation<'U>
        =
        this.TryFinally(
            (fun () -> binder resource),
            fun () ->
                if not (isNull (box resource)) then
                    resource.Dispose()
        )
