module EasyBuild.Workspace

open EasyBuild.FileSystemProvider

[<Literal>]
let ROOT = __SOURCE_DIRECTORY__ + "/../"

type Workspace = RelativeFileSystem<ROOT>
