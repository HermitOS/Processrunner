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

### Cancellation and timeouts

You can cancel a running process using a `CancellationToken`, for example to enforce a timeout. When cancelled, the process is terminated and the task will be cancelled (you may observe `TaskCanceledException`, which derives from `OperationCanceledException`).

```csharp
using ProcessUtils;
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

try
{
    var result = await ProcessRunner.Run(
        workingDirectory: string.Empty,
        scriptPath: OperatingSystem.IsWindows() ? "python" : "python3",
        arguments: new[] { "./long_running_script.py" },
        cancellationToken: cts.Token
    );
    Console.WriteLine($"Output: {result.StdOut}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Process timed out and was cancelled.");
}
```

Or via Package Manager Console:

```powershell
Install-Package ProcessUtils
```

## Usage

```csharp
using ProcessUtils;

// Run a process and capture output
var result = await ProcessRunner.Run(
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
var result = await ProcessRunner.Run(
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

### Working with binary data (PDF via Base64)

When a child process needs to return binary data (like a PDF) over stdout, a common pattern is to base64-encode the bytes in the script and decode them in .NET.

Python (pdf_to_base64.py):

```python
#!/usr/bin/env python3
import sys, base64

pdf_path = sys.argv[1] if len(sys.argv) > 1 else "./document.pdf"
with open(pdf_path, "rb") as f:
    data = f.read()

# Print base64 to stdout (no extra text)
print(base64.b64encode(data).decode("ascii"))
```

.NET usage:

```csharp
// Invoke the Python script to get a base64-encoded PDF on stdout
var result = await ProcessRunner.Run(
    workingDirectory: string.Empty,
    scriptPath: OperatingSystem.IsWindows() ? "python" : "python3",
    arguments: new[] { "./pdf_to_base64.py", "./document.pdf" }
);

if (result.ExitCode != 0)
    throw new InvalidOperationException($"Process failed: {result.StdErr}");

// Convert base64 back to bytes
// Note: Convert.FromBase64String ignores whitespace (including newlines)
string base64 = result.StdOut;
byte[] pdfBytes = Convert.FromBase64String(base64);

// Persist or stream as needed
await File.WriteAllBytesAsync("./output.pdf", pdfBytes);
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
- `StdOut`: Captured standard output
- `StdErr`: Captured standard error

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
