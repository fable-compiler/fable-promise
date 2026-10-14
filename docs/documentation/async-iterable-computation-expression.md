---
title: AsyncIterable computation expression
layout: standard
toc:
    to: 3
---

## Introduction

The `asyncIterable` computation expression creates a `JS.AsyncIterable<'T>`.

It is the F# equivalent of a JavaScript `async function*`.

<div class="columns" date-disable-copy-button="true">
    <div class="column is-half-desktop">

<div class="has-text-centered mb-2 has-text-weight-semibold">JavaScript</div>

```js
async function* numbers(count) {
    for (let i = 1; i <= count; i++) {
        await sleep(100);
        yield i;
    }
}
```

</div>
    <div class="column is-half-desktop">

<div class="has-text-centered mb-2 has-text-weight-semibold">F#</div>

```fsharp
let numbers (count: int) =
    asyncIterable {
        for i in 1 .. count do
            do! Promise.sleep 100
            yield i
    }
```

</div>
</div>

The generated code uses async generators. Your Fable target must be ES2018 or later.

## Guides

You can read more about computation expression in F# [here](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions).

### Yield values

Use `yield` to produce a value.

```fsharp
asyncIterable {
    yield 1
    yield 2
}
```

### Await a promise

Use `let!` to wait for a promise and get its result.

Use `do!` to wait for a promise that returns `unit`.

```fsharp
asyncIterable {
    let! user = fetchUser id
    yield user.Name

    do! Promise.sleep 100
    yield "done"
}
```

### Await promises in parallel

Use `let!` with `and!` to wait for several promises at the same time.

```fsharp
asyncIterable {
    let! user = fetchUser id
    and! permissions = fetchPermissions id

    yield user, permissions
}
```

### Loop over a sequence

Use `for ... in ... do` to loop over a list, an array or any `seq`.

```fsharp
asyncIterable {
    for id in [ 1; 2; 3 ] do
        let! user = fetchUser id
        yield user
}
```

### Loop over an AsyncIterable

Use `for ... in ... do` to loop over another `JS.AsyncIterable`. It is the equivalent of `for await` in JavaScript.

```fsharp
asyncIterable {
    for line in readLines file do
        yield line.ToUpper()
}
```

### Yield another AsyncIterable

Use `yield!` to produce all the values of another `JS.AsyncIterable`.

```fsharp
let rec countdown (n: int) =
    asyncIterable {
        if n > 0 then
            yield n
            yield! countdown (n - 1)
    }
```

### Loop while a condition is true

Use `while ... do` to repeat the body until the condition is `false`.

```fsharp
asyncIterable {
    let mutable page = 0
    let mutable hasMore = true

    while hasMore do
        let! result = fetchPage page

        for item in result.Items do
            yield item

        hasMore <- result.HasMore
        page <- page + 1
}
```

### Handle errors

Use `try ... with` to handle an exception or a rejected promise.

```fsharp
asyncIterable {
    try
        let! user = fetchUser id
        yield Ok user
    with error ->
        yield Error error.Message
}
```

An unhandled error rejects the consumer.

### Clean up resources

Use `try ... finally` or `use` to run code when the iteration ends.

The cleanup runs when the iteration completes, when an error occurs, or when the consumer stops early with `break`.

```fsharp
asyncIterable {
    use connection = openConnection ()

    for row in connection.Rows do
        yield row
}
```

### Evaluation

The body runs when the consumer starts the iteration.

Each new iteration runs the body again from the start.

### Consume an AsyncIterable

Use `AsyncIterable.iter` to consume the values from F#.

```fsharp
numbers 3
|> AsyncIterable.iter (fun _ value -> printfn "%d" value)
```

You can also pass the `JS.AsyncIterable` to JavaScript code that uses `for await`.
