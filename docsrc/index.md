---
title:
layout: nacara-navbar-only
---

<!-- Disable the copy-button on all the elements contained inside the container (all this page) -->
<div class="container mt-5" date-disable-copy-button="true">
    <!--
        Selling points of Fable
        For the selling points of Fable we use CSS grid instead of Bulma columns
        because we want all the box to have the same height.
        This is not something possible to do dynamically using Flexbox / Bulma columns system
    -->
    <section class="section">
        <h2 class="title is-2 has-text-primary has-text-centered">
            Fable.Promise
        </h2>
        <p class="content is-size-5 has-text-centered">
            Fable.Promise is a library making it easy to work with Promise. It provides
        </p>
        <div class="columns is-vcentered mt-5">
            <div class="column is-4">
                <h4 class="title has-text-primary">
                    Pipeline API
                </h4>
                <p class="content is-size-5">
                    Pipeline allows you to write close to JavaScript code.
                </p>
            </div>
            <div class="column is-6 is-offset-1 is-7-tablet">
                <div class="content has-code-block is-normal">

<!-- The indentation/format used has been chosen so the code is displayed
without scrollbar on almost any screen size -->
```fsharp
fetch "https://jsonplaceholder.typicode.com/users"
|> Promise.map (fun user ->
    fetch "https://jsonplaceholder.typicode.com/posts"
)
|> Promise.map (fun posts ->
    fetch "https://jsonplaceholder.typicode.com/comments"
)
|> Promise.map (fun comments ->
    // Done
)
```
</div> <!-- Markdown is sensible to indentation -->
            </div>
        </div>
        <div class="columns is-vcentered mt-5">
            <div class="column is-4">
                <h4 class="title has-text-primary">
                    Computation expressions
                </h4>
                <p class="content is-size-5">
                    Computation expressions make it easy to chain operations
                </p>
            </div>
            <div class="column is-6 is-offset-1 is-7-tablet">
                <div class="content has-code-block is-normal">

<!-- The indentation/format used has been chosen so the code is displayed
without scrollbar on almost any screen size -->
```fsharp
promise {
    let! users = fetch "https://jsonplaceholder.typicode.com/users"
    let! posts = fetch "https://jsonplaceholder.typicode.com/posts"
    let! comments = fetch "https://jsonplaceholder.typicode.com/comments"

    // Done success
    return ()
}
```
</div> <!-- Markdown is sensible to indentation -->
            </div>
        </div>
    </section>
</div>
