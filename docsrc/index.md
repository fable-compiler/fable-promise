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
            Fable.Promise is a library making it easy to work with Promise.
            <br/><br/>
            It provides two styles of APIs
        </p>
        <div class="columns is-vcentered mt-5">
            <div class="column is-4 is-offset-1-desktop">
                <h4 class="title has-text-primary">
                    Pipeline API
                </h4>
                <p class="content is-size-5">
                    Pipeline allows you to write similar to JavaScript code.
                </p>
                <a href="/fable-promise/documentation/pipeline.html">
                    Learn more →
                </a>
            </div>
            <div class="column is-5 is-offset-1-desktop is-7-tablet-only">
                <div class="content has-code-block is-normal">

<!-- The indentation/format used has been chosen so the code is displayed
without scrollbar on almost any screen size -->
```fsharp
fetch "https://my-api.com/users"
|> Promise.map (fun user ->
    fetch "https://my-api.com/posts"
)
|> Promise.map (fun posts ->
    fetch "https://my-api.com/comments"
)
|> Promise.map (fun comments ->
    // Done
)
```
</div> <!-- Markdown is sensible to indentation -->
            </div>
        </div>
        <div class="columns is-vcentered mt-5">
            <div class="column is-4 is-offset-1-desktop">
                <h4 class="title has-text-primary">
                    Computation expressions
                </h4>
                <p class="content is-size-5">
                    Computation expressions make it easy to chain operations
                </p>
                <a href="/fable-promise/documentation/computation-expression.html">
                    Learn more →
                </a>
            </div>
            <div class="column is-5 is-offset-1-desktop is-7-tablet-only">
                <div class="content has-code-block is-normal">

<!-- The indentation/format used has been chosen so the code is displayed
without scrollbar on almost any screen size -->
```fsharp
promise {
    let! users = fetch "https://my-api.com/users"
    let! posts = fetch "https://my-api.com/posts"
    let! comments = fetch "https://my-api.com/comments"

    // Done success
    return ()
}
```
</div> <!-- Markdown is sensible to indentation -->
            </div>
        </div>
    </section>
</div>
