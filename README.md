# Fable.Promise

Fable bindings for JS promise.

## How to test locally ?

`./build.sh test`

Add `--watch` to re-run the tests on each change.

## How to format the code ?

`dotnet fantomas .`

The pre-commit hook checks the staged F# files, and CI checks the whole repository.

## How to publish a new version of the package ?

Releases are automated with [EasyBuild.ShipIt](https://github.com/easybuild-org/EasyBuild.ShipIt).

Commits landing on `main` must follow the [Conventional Commits](https://www.conventionalcommits.org/) specification. ShipIt opens a release pull request with the new version and changelog entry. Merging that pull request publishes the package to NuGet.

## How to work on the documentation ?

1. `npm install`
2. `npm run docs:watch`
3. Go to [http://localhost:8080/](http://localhost:8080/)

## How to update the documentation ?

Deployment should be done automatically when pushing to `dev` branch.

If the CI is broken, you can manually deploy it by running `npm run docs:deploy`.
