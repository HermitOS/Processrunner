# ProcessUtils

A lightweight .NET utility library for running external processes with proper argument escaping and output capture.

[![CI Build](https://github.com/hermitos/processutils/actions/workflows/ci-build.yml/badge.svg)](https://github.com/hermitos/processutils/actions/workflows/ci-build.yml)
[![NuGet](https://img.shields.io/nuget/v/ProcessUtils.svg)](https://www.nuget.org/packages/ProcessUtils/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ProcessUtils.svg)](https://www.nuget.org/packages/ProcessUtils/)

## Features

- ✅ Safe process execution with proper argument escaping
- ✅ Capture standard output and error streams
- ✅ Cross-platform support (.NET 8.0+)
- ✅ Simple, clean API
- ✅ Originally derived from the NUnit3TestAdapter project

## Installation

Install via NuGet:

```bash
dotnet add package ProcessUtils
```

Or via Package Manager Console:

```powershell
Install-Package ProcessUtils
```

## Usage

```csharp
using ProcessUtils;

// Run a process and capture output
var result = ProcessRunner.Run(
    workingDirectory: @"C:\MyApp",
    scriptPath: "myapp.exe",
    arguments: new[] { "--option", "value with spaces" }
);

// Check the results
if (result.ExitCode == 0)
{
    Console.WriteLine("Success!");
    Console.WriteLine($"Output: {result.StdOut}");
}
else
{
    Console.WriteLine($"Error: {result.StdErr}");
}
```

### Azure/Cloud scenarios

When you don't control the working directory (for example in Azure Functions, App Service, or containerized environments), you can pass an empty string for the working directory. ProcessUtils will use the current directory of the running process (typically the bin folder). You can then pass a relative script path like "./pythonscript.py".

```csharp
using ProcessUtils;

// Assume pythonscript.py is deployed next to your binaries (bin folder)
// We'll call Python and pass a relative path to the script
var result = ProcessRunner.Run(
    workingDirectory: string.Empty,     // Use current directory automatically
    scriptPath: OperatingSystem.IsWindows() ? "python" : "python3",
    arguments: new[] { "./pythonscript.py" }
);

if (result.ExitCode == 0)
{
    Console.WriteLine($"Script output: {result.StdOut}");
}
else
{
    Console.WriteLine($"Script error: {result.StdErr}");
}
```

### API Reference

#### `ProcessRunner.Run()`

Executes a process and returns the result.

**Parameters:**

- `workingDirectory` (string): The working directory for the process. Must be an absolute path.
- `scriptPath` (string): The path to the executable or script to run.
- `arguments` (IEnumerable<string>, optional): Command-line arguments to pass to the process.

**Returns:** `ProcessRunResult` containing:

- `ExitCode`: The exit code of the process
- `StandardOutput`: Captured standard output
- `StandardError`: Captured standard error

#### `ProcessRunner.EscapeProcessArguments()`

Escapes a collection of argument values for safe process execution.

**Parameters:**

- `literalValues` (IEnumerable<string>): The argument values to escape
- `alwaysQuote` (bool, optional): Whether to always quote arguments

**Returns:** A properly escaped command-line string

## Building from Source

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Create NuGet package
dotnet pack --configuration Release
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.

## Acknowledgments

This library is derived from the [NUnit3TestAdapter](https://github.com/nunit/nunit3-vs-adapter) project.

## Support

- 📖 [Documentation](https://github.com/hermitos/processutils)
- 🐛 [Issue Tracker](https://github.com/hermitos/processutils/issues)
- 💬 [Discussions](https://github.com/hermitos/processutils/discussions)
