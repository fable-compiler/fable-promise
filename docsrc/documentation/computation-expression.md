---
title: Computation expression
layout: nacara-standard
---

## Introduction

The `promise` computation expression makes it really easy to create and compose promise using F#.

<div class="columns" date-disable-copy-button="true">
    <div class="column is-half-desktop">

<div class="has-text-centered mb-2 has-text-weight-semibold">Pipeline API</div>

```fsharp
fetch "https://x.x/users"
|> Promise.map (fun response ->
    fetch "https://x.x/posts"
)
|> Promise.map (fun response ->
    // Done, do something with the result
)
```

</div>
    <div class="column is-half-desktop">

<div class="has-text-centered mb-2 has-text-weight-semibold">Computation expression</div>

```fsharp
promise {
    let! users = fetch "https://x.x/users"
    let! posts = fetch "https://x.x/posts"

    // Done, do something with the result
    return //...
}
```

</div>
</div>

## Guides

Here is a quick guides of what you can do with `promise` computations.

You can read more about computation expression in F# [here](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions).

### Create a promise

Creating a promise is as simple as writing `promise { }`.

```fsharp
let double (value : int) =
    promise {
        return value * 2
    }
```

### Chaining promises

If you need the result of a promise before calling another one, use the `let!` keyword

```fsharp
promise {
    let! user = fetchUsers session
    let! permission = fetchPermission user

    return permission
}
```

You can also directly return the result of a promise avoiding to use `let!` and `return`

`return!` will evaluate the promise and return the result value when completed

```fsharp
promise {
    let! user = fetchUsers session

    return! fetchPermission user
}
```

### Nesting promises

You can nest `promise` computation as needed.

```fsharp
promise {
    // Nested promise which returns a value
    let! isValid =
        promise {
            // Do something
            return aValue
        }

    // Nested promise which return unit
    do! promise {
        // Do something
        return ()
    }
}
```

### Parallel

If you have independent promise, you can use `let!` and `and!` to run them in parallel.

```fsharp
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

promise {
    let! a = p1
    and! b = p2
    and! c = p3

    return a + b + c // 1 + 2 + 3 = 6
}
```
