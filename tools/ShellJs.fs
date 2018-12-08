// ts2fable 0.6.1
module rec ShellJs

open System
open Fable.Core
open Fable.Import.JS
open Fable.Import.Node

type [<AllowNullLiteral>] IExports =
    /// <summary>Changes to directory dir for the duration of the script. Changes to home directory if no argument is supplied.</summary>
    /// <param name="dir">Directory to change in.</param>
    abstract cd: ?dir: string -> unit
    /// Returns the current directory.
    abstract pwd: unit -> ShellString
    /// Returns array of files in the given path, or in current directory if no path provided.
    abstract ls: [<ParamArray>] paths: obj[] -> ShellArray
    /// <summary>Returns array of files in the given path, or in current directory if no path provided.</summary>
    /// <param name="options">Available options:  -R: recursive -A: all files (include files beginning with ., except for . and ..) -L: follow symlinks -d: list directories themselves, not their contents -l: list objects representing each file, each with fields containing ls -l output fields. See fs.Stats for more info</param>
    abstract ls: options: string * [<ParamArray>] paths: obj[] -> ShellArray
    /// Returns array of all files (however deep) in the given paths.
    abstract find: [<ParamArray>] path: obj[] -> ShellArray
    /// <summary>Copies files. The wildcard * is accepted.</summary>
    /// <param name="source">The source.</param>
    /// <param name="dest">The destination.</param>
    abstract cp: source: obj * dest: string -> unit
    /// <summary>Copies files. The wildcard * is accepted.</summary>
    /// <param name="options">Available options: -f: force (default behavior) -n: no-clobber -u: only copy if source is newer than dest -r, -R: recursive -L: follow symlinks -P: don't follow symlinks</param>
    /// <param name="source">The source.</param>
    /// <param name="dest">The destination.</param>
    abstract cp: options: string * source: obj * dest: string -> unit
    /// Removes files. The wildcard * is accepted.
    abstract rm: [<ParamArray>] files: obj[] -> unit
    /// <summary>Removes files. The wildcard * is accepted.</summary>
    /// <param name="options">Available options: -f (force), -r, -R (recursive)</param>
    abstract rm: options: string * [<ParamArray>] files: obj[] -> unit
    /// <summary>Moves files. The wildcard * is accepted.</summary>
    /// <param name="source">The source.</param>
    /// <param name="dest">The destination.</param>
    abstract mv: source: obj * dest: string -> unit
    /// <summary>Moves files. The wildcard * is accepted.</summary>
    /// <param name="options">Available options: -f: force (default behavior) -n: no-clobber</param>
    /// <param name="source">The source.</param>
    /// <param name="dest">The destination.</param>
    abstract mv: options: string * source: obj * dest: string -> unit
    /// Creates directories.
    abstract mkdir: [<ParamArray>] dir: obj[] -> unit
    /// <summary>Creates directories.</summary>
    /// <param name="options">Available options: p (full paths, will create intermediate dirs if necessary)</param>
    abstract mkdir: options: string * [<ParamArray>] dir: obj[] -> unit
    /// <summary>Evaluates expression using the available primaries and returns corresponding value.</summary>
    /// <param name="option">'-b': true if path is a block device; '-c': true if path is a character device; '-d': true if path is a directory; '-e': true if path exists; '-f': true if path is a regular file; '-L': true if path is a symboilc link; '-p': true if path is a pipe (FIFO); '-S': true if path is a socket</param>
    /// <param name="path">The path.</param>
    abstract test: option: TestOptions * path: string -> bool
    /// Returns a string containing the given file, or a concatenated string containing the files if more than one file is given (a new line character is introduced between each file). Wildcard * accepted.
    abstract cat: [<ParamArray>] files: obj[] -> ShellString
    /// <summary>Reads an input string from file and performs a JavaScript replace() on the input using the given search regex and replacement string or function. Returns the new string after replacement.</summary>
    /// <param name="searchRegex">The regular expression to use for search.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="file">The file to process.</param>
    abstract sed: searchRegex: obj * replacement: string * file: string -> ShellString
    /// <summary>Reads an input string from file and performs a JavaScript replace() on the input using the given search regex and replacement string or function. Returns the new string after replacement.</summary>
    /// <param name="options">Available options: -i (Replace contents of 'file' in-place. Note that no backups will be created!)</param>
    /// <param name="searchRegex">The regular expression to use for search.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="file">The file to process.</param>
    abstract sed: options: string * searchRegex: obj * replacement: string * file: string -> ShellString
    /// <summary>Reads input string from given files and returns a string containing all lines of the file that match the given regex_filter. Wildcard * accepted.</summary>
    /// <param name="regex_filter">The regular expression to use.</param>
    abstract grep: regex_filter: obj * [<ParamArray>] files: obj[] -> ShellString
    /// <summary>Reads input string from given files and returns a string containing all lines of the file that match the given regex_filter. Wildcard * accepted.</summary>
    /// <param name="options">Available options: -v (Inverse the sense of the regex and print the lines not matching the criteria.) -l: Print only filenames of matching files</param>
    /// <param name="regex_filter">The regular expression to use.</param>
    abstract grep: options: string * regex_filter: obj * [<ParamArray>] files: obj[] -> ShellString
    /// <summary>Searches for command in the system's PATH. On Windows looks for .exe, .cmd, and .bat extensions.</summary>
    /// <param name="command">The command to search for.</param>
    abstract which: command: string -> ShellString
    /// Prints string to stdout, and returns string with additional utility methods like .to().
    abstract echo: [<ParamArray>] text: string[] -> ShellString
    // /// <summary>Prints string to stdout, and returns string with additional utility methods like .to().</summary>
    // /// <param name="options">Available options: -e: interpret backslash escapes (default) -n: remove trailing newline from output</param>
    // abstract echo: options: string * [<ParamArray>] text: string[] -> ShellString
    /// <summary>Save the current directory on the top of the directory stack and then cd to dir. With no arguments, pushd exchanges the top two directories. Returns an array of paths in the stack.</summary>
    /// <param name="dir">Makes the current working directory be the top of the stack, and then executes the equivalent of cd dir.</param>
    abstract pushd: dir: string -> ShellArray
    /// <summary>Save the current directory on the top of the directory stack and then cd to dir. With no arguments, pushd exchanges the top two directories. Returns an array of paths in the stack.</summary>
    /// <param name="options">Available options: -n (Suppresses the normal change of directory when adding directories to the stack, so that only the stack is manipulated)</param>
    /// <param name="dir">Makes the current working directory be the top of the stack, and then executes the equivalent of cd dir.</param>
    abstract pushd: options: string * dir: string -> ShellArray
    /// When no arguments are given, popd removes the top directory from the stack and performs a cd to the new top directory. The elements are numbered from 0 starting at the first directory listed with dirs; i.e., popd is equivalent to popd +0. Returns an array of paths in the stack.
    abstract popd: unit -> ShellArray
    /// <summary>When no arguments are given, popd removes the top directory from the stack and performs a cd to the new top directory. The elements are numbered from 0 starting at the first directory listed with dirs; i.e., popd is equivalent to popd +0. Returns an array of paths in the stack.</summary>
    /// <param name="dir">You can only use -N and +N.</param>
    abstract popd: dir: string -> ShellArray
    /// <summary>When no arguments are given, popd removes the top directory from the stack and performs a cd to the new top directory. The elements are numbered from 0 starting at the first directory listed with dirs; i.e., popd is equivalent to popd +0. Returns an array of paths in the stack.</summary>
    /// <param name="options">Available options: -n (Suppresses the normal change of directory when removing directories from the stack, so that only the stack is manipulated) -q: Supresses output to the console.</param>
    /// <param name="dir">You can only use -N and +N.</param>
    abstract popd: options: string * dir: string -> ShellArray
    /// <summary>Display the list of currently remembered directories. Returns an array of paths in the stack, or a single path if +N or -N was specified.</summary>
    /// <param name="options">Available options: -c, -N, +N. You can only use those.</param>
    abstract dirs: options: string -> obj option
    /// <summary>Links source to dest. Use -f to force the link, should dest already exist.</summary>
    /// <param name="source">The source.</param>
    /// <param name="dest">The destination.</param>
    abstract ln: source: string * dest: string -> unit
    /// <summary>Links source to dest. Use -f to force the link, should dest already exist.</summary>
    /// <param name="options">Available options: s (symlink), f (force)</param>
    /// <param name="source">The source.</param>
    /// <param name="dest">The destination.</param>
    abstract ln: options: string * source: string * dest: string -> unit
    /// <summary>Exits the current process with the given exit code.</summary>
    /// <param name="code">The exit code.</param>
    abstract exit: code: float -> unit
    abstract env: obj
    /// <summary>Executes the given command synchronously.</summary>
    /// <param name="command">The command to execute.</param>
    abstract exec: command: string -> ExecOutputReturnValue
    /// <summary>Executes the given command synchronously.</summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="options">Silence and synchronous options.</param>
    abstract exec: command: string * options: ExecOptions -> ExecOutputReturnValue
    /// <summary>Executes the given command synchronously.</summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="options">Silence and synchronous options.</param>
    /// <param name="callback">Receives code and output asynchronously.</param>
    abstract exec: command: string * options: ExecOptions * callback: ExecCallback -> ChildProcess.ChildProcess
    /// <summary>Executes the given command synchronously.</summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="callback">Receives code and output asynchronously.</param>
    abstract exec: command: string * callback: ExecCallback -> ChildProcess.ChildProcess
    /// <summary>Alters the permissions of a file or directory by either specifying the absolute permissions in octal form or expressing the changes in symbols. This command tries to mimic the POSIX behavior as much as possible. Notable exceptions:
    /// - In symbolic modes, 'a-r' and '-r' are identical. No consideration is given to the umask.
    /// - There is no "quiet" option since default behavior is to run silent.</summary>
    /// <param name="octalMode">The access mode. Octal.</param>
    /// <param name="file">The file to use.</param>
    abstract chmod: octalMode: float * file: string -> unit
    /// <summary>Alters the permissions of a file or directory by either specifying the absolute permissions in octal form or expressing the changes in symbols. This command tries to mimic the POSIX behavior as much as possible. Notable exceptions:
    /// - In symbolic modes, 'a-r' and '-r' are identical. No consideration is given to the umask.
    /// - There is no "quiet" option since default behavior is to run silent.</summary>
    /// <param name="options">Available options: -v (output a diagnostic for every file processed), -c (like -v but report only when a change is made), -R (change files and directories recursively)</param>
    /// <param name="octalMode">The access mode. Octal.</param>
    /// <param name="file">The file to use.</param>
    abstract chmod: options: string * octalMode: float * file: string -> unit
    /// <summary>Alters the permissions of a file or directory by either specifying the absolute permissions in octal form or expressing the changes in symbols. This command tries to mimic the POSIX behavior as much as possible. Notable exceptions:
    /// - In symbolic modes, 'a-r' and '-r' are identical. No consideration is given to the umask.
    /// - There is no "quiet" option since default behavior is to run silent.</summary>
    /// <param name="mode">The access mode. Can be an octal string or a symbolic mode string.</param>
    /// <param name="file">The file to use.</param>
    abstract chmod: mode: string * file: string -> unit
    /// <summary>Alters the permissions of a file or directory by either specifying the absolute permissions in octal form or expressing the changes in symbols. This command tries to mimic the POSIX behavior as much as possible. Notable exceptions:
    /// - In symbolic modes, 'a-r' and '-r' are identical. No consideration is given to the umask.
    /// - There is no "quiet" option since default behavior is to run silent.</summary>
    /// <param name="options">Available options: -v (output a diagnostic for every file processed), -c (like -v but report only when a change is made), -R (change files and directories recursively)</param>
    /// <param name="mode">The access mode. Can be an octal string or a symbolic mode string.</param>
    /// <param name="file">The file to use.</param>
    abstract chmod: options: string * mode: string * file: string -> unit
    /// Searches and returns string containing a writeable, platform-dependent temporary directory. Follows Python's tempfile algorithm.
    abstract tempdir: unit -> ShellString
    /// Tests if error occurred in the last command.
    abstract error: unit -> ShellString
    abstract touch: [<ParamArray>] files: string[] -> unit
    abstract touch: options: TouchOptionsLiteral * [<ParamArray>] files: obj[] -> unit
    abstract touch: options: TouchOptionsArray * [<ParamArray>] files: obj[] -> unit
    /// Read the start of a file.
    abstract head: [<ParamArray>] files: obj[] -> ShellString
    /// Read the start of a file.
    abstract head: options: HeadOptions * [<ParamArray>] files: obj[] -> ShellString
    /// Return the contents of the files, sorted line-by-line. Sorting multiple files mixes their content (just as unix sort does).
    abstract sort: [<ParamArray>] files: obj[] -> ShellString
    /// <summary>Return the contents of the files, sorted line-by-line. Sorting multiple files mixes their content (just as unix sort does).</summary>
    /// <param name="options">Available options: -r: Reverse the results -n: Compare according to numerical value</param>
    abstract sort: options: string * [<ParamArray>] files: obj[] -> ShellString
    /// Read the end of a file.
    abstract tail: [<ParamArray>] files: obj[] -> ShellString
    /// Read the end of a file.
    abstract tail: options: TailOptions * [<ParamArray>] files: obj[] -> ShellString
    /// Filter adjacent matching lines from input.
    abstract uniq: input: string * ?output: string -> ShellString
    /// <summary>Filter adjacent matching lines from input.</summary>
    /// <param name="options">Available options: -i: Ignore case while comparing -c: Prefix lines by the number of occurrences -d: Only print duplicate lines, one for each group of identical lines</param>
    abstract uniq: options: string * input: string * ?output: string -> ShellString
    /// <summary>Sets global configuration variables</summary>
    /// <param name="options">Available options: `+/-e`: exit upon error (`config.fatal`), `+/-v`: verbose: show all commands (`config.verbose`), `+/-f`: disable filename expansion (globbing)</param>
    abstract set: options: string -> unit
    abstract config: ShellConfig

type [<StringEnum>] [<RequireQualifiedAccess>] TestOptions =
    | [<CompiledName "-b">] B
    | [<CompiledName "-c">] C
    | [<CompiledName "-d">] D
    | [<CompiledName "-e">] E
    | [<CompiledName "-f">] F
    | [<CompiledName "-L">] L
    | [<CompiledName "-p">] P
    | [<CompiledName "-S">] S

type [<AllowNullLiteral>] ExecCallback =
    [<Emit "$0($1...)">] abstract Invoke: code: float * stdout: string * stderr: string -> obj option

type [<AllowNullLiteral>] ExecOptions =
    inherit ChildProcess.ExecOptions
    /// Do not echo program output to console (default: false).
    abstract silent: bool option with get, set
    /// Asynchronous execution. If a callback is provided, it will be set to true, regardless of the passed value (default: false).
    abstract async: bool option with get, set
    /// Character encoding to use. Affects the values returned to stdout and stderr, and what is written to stdout and stderr when not in silent mode (default: 'utf8').
    abstract encoding: string option with get, set

type [<AllowNullLiteral>] ExecOutputReturnValue =
    abstract code: float with get, set
    abstract stdout: string with get, set
    abstract stderr: string with get, set

type [<AllowNullLiteral>] ShellReturnValue =
    inherit ExecOutputReturnValue
    /// <summary>Analogous to the redirection operator > in Unix, but works with JavaScript strings (such as those returned by cat, grep, etc). Like Unix redirections, to() will overwrite any existing file!</summary>
    /// <param name="file">The file to use.</param>
    abstract ``to``: file: string -> unit
    /// <summary>Analogous to the redirect-and-append operator >> in Unix, but works with JavaScript strings (such as those returned by cat, grep, etc).</summary>
    /// <param name="file">The file to append to.</param>
    abstract toEnd: file: string -> unit
    abstract cat: [<ParamArray>] files: string[] -> ShellString
    abstract exec: callback: ExecCallback -> ChildProcess.ChildProcess
    abstract exec: unit -> ExecOutputReturnValue
    abstract grep: [<ParamArray>] files: obj[] -> ShellString
    abstract sed: replacement: string * file: string -> ShellString

type [<AllowNullLiteral>] ShellString =
    interface end

type [<AllowNullLiteral>] ShellArray =
    interface end

type [<StringEnum>] [<RequireQualifiedAccess>] TouchOptionsLiteral =
    | [<CompiledName "-a">] A
    | [<CompiledName "-c">] C
    | [<CompiledName "-m">] M
    | [<CompiledName "-d">] D
    | [<CompiledName "-r">] R

/// Update the access and modification times of each FILE to the current time. A FILE argument that does not exist is created empty, unless -c is supplied
type [<AllowNullLiteral>] TouchOptionsArray =
    abstract ``-d``: string option with get, set
    abstract ``-r``: string option with get, set

type [<AllowNullLiteral>] HeadOptions =
    /// Show the first <num> lines of the files.
    abstract ``-n``: float with get, set

type [<AllowNullLiteral>] TailOptions =
    /// Show the last <num> lines of files.
    abstract ``-n``: float with get, set

type [<AllowNullLiteral>] ShellConfig =
    /// Suppresses all command output if true, except for echo() calls. Default is false.
    abstract silent: bool with get, set
    /// If true the script will die on errors. Default is false.
    abstract fatal: bool with get, set
    /// Will print each executed command to the screen. Default is true.
    abstract verbose: bool with get, set
    // /// Passed to glob.sync() instead of the default options ({}).
    // abstract globOptions: Glob.IOptions with get, set
    /// Absolute path of the Node binary. Default is null (inferred).
    abstract execPath: string option with get, set
    /// Reset shell.config to the defaults.
    abstract reset: unit -> unit