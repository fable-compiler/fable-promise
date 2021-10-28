---
title: Pipeline
layout: standard
toc:
    to: 3
---

## Introduction

Pipeline style allows you use to chain your promise using the pipe operator `|>`.

Writing your code using the pipeline style makes your code looks similar to what you would write in JavaScript.

<div class="columns" date-disable-copy-button="true">
    <div class="column is-half-desktop">

<div class="has-text-centered mb-2 has-text-weight-semibold">JavaScript</div>

```js
fetch('https://my-api.com/users')
.then(function (response) {
  return fetch('https://my-api.com/posts')
})
.then(function (response) {
    // Done, do something with the result
})
.catch(function (req) {
    // An error ocurred
})
```

</div>
    <div class="column is-half-desktop">

<div class="has-text-centered mb-2 has-text-weight-semibold">F#</div>

```fsharp
fetch "https://my-api.com/users"
|> Promise.map (fun response ->
    fetch "https://my-api.com/posts"
)
|> Promise.map (fun response ->
    // Done, do something with the result
)
|> Promise.catch (fun error ->
    // An error ocurred
)
```
</div>
</div>

You can find all the available function with examples [here](http://localhost:8080/reference/Fable.Promise/global-promise.html)
