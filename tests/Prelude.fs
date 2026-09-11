[<AutoOpen>]
module Prelude

open Fable.Core

let awaitPromise (p: JS.Promise<'T>) : Async<unit> =
    p |> Promise.map ignore |> Async.AwaitPromise
