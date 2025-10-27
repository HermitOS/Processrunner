/* This file is originally from the NUnit3TestAdapter project, and modified here */

namespace ProcessUtils.Tests;

using ProcessUtils;

public static class ProcessUtilsTests
{
    [Test]
    public static void EscapeProcessArguments_null()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments([null]), Is.EqualTo("\"\""));
    }

    [Test]
    public static async Task Run_PythonScript_Base64Roundtrip_Works()
    {
        // Arrange
        var testDirectory = Path.GetDirectoryName(typeof(ProcessUtilsTests).Assembly.Location)!;
        var samplePath = Path.Combine(testDirectory, "sample.bin");
        var encoderScript = Path.Combine(testDirectory, "encode_base64.py");

        // Create a small binary sample payload
        var original = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        await File.WriteAllBytesAsync(samplePath, original);

        var pythonCommand = OperatingSystem.IsWindows() ? "python" : "python3";

        // Act
        var result = await ProcessRunner.Run(
            workingDirectory: testDirectory,
            scriptPath: pythonCommand,
            arguments: new[] { encoderScript, samplePath }
        );

        // Assert
        Assert.That(result.ExitCode, Is.EqualTo(0), $"Python encoder failed: {result.StdErr}");
        var decoded = Convert.FromBase64String(result.StdOut.Trim());
        Assert.That(decoded.SequenceEqual(original), Is.True, "Decoded bytes did not match original");
    }

    [Test]
    public static void Run_Cancelled_ThrowsOperationCanceled()
    {
        // Arrange
        var testDirectory = Path.GetDirectoryName(typeof(ProcessUtilsTests).Assembly.Location);
        var pythonScript = Path.Combine(testDirectory!, "test_sleep.py");
        var pythonCommand = OperatingSystem.IsWindows() ? "python" : "python3";

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        // Act + Assert
    Assert.That(async () => await ProcessRunner.Run(
                workingDirectory: testDirectory!,
                scriptPath: pythonCommand,
                arguments: new[] { pythonScript, "5" },
        cancellationToken: cts.Token),
        Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public static async Task RunAsync_PythonScript_ReturnsCorrectOutput()
    {
        // Arrange
        var testDirectory = Path.GetDirectoryName(typeof(ProcessUtilsTests).Assembly.Location);
        var pythonScript = Path.Combine(testDirectory!, "test_add.py");

        var pythonCommand = OperatingSystem.IsWindows() ? "python" : "python3";

        // Act
        var result = await ProcessRunner.RunAsync(
            workingDirectory: testDirectory!,
            scriptPath: pythonCommand,
            arguments: new[] { pythonScript }
        );

        // Assert
        Assert.That(result.ExitCode, Is.EqualTo(0), $"Python script failed with error: {result.StdErr}");
        Assert.That(result.StdOut.Trim(), Is.EqualTo("42"));
    }

    [Test]
    public static void EscapeProcessArguments_null_alwaysQuote()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments([null], true), Is.EqualTo("\"\""));
    }

    [Test]
    public static void EscapeProcessArguments_empty()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments([string.Empty]), Is.EqualTo("\"\""));
    }

    [Test]
    public static void EscapeProcessArguments_empty_alwaysQuote()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments([string.Empty], true), Is.EqualTo("\"\""));
    }

    [Test]
    public static void EscapeProcessArguments_simple()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments(["123"]), Is.EqualTo("123"));
    }

    [Test]
    public static void EscapeProcessArguments_simple_alwaysQuote()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments(["123"], true), Is.EqualTo("\"123\""));
    }

    [Test]
    public static void EscapeProcessArguments_with_ending_backslash()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments(["123\\"]), Is.EqualTo("123\\"));
    }

    [Test]
    public static void EscapeProcessArguments_with_ending_backslash_alwaysQuote()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments(["123\\"], true), Is.EqualTo("\"123\\\\\""));
    }

    [Test]
    public static void EscapeProcessArguments_with_spaces_and_ending_backslash()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments([" 1 2 3 \\"]), Is.EqualTo("\" 1 2 3 \\\\\""));
    }

    [Test]
    public static void EscapeProcessArguments_with_spaces()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments([" 1 2 3 "]), Is.EqualTo("\" 1 2 3 \""));
    }

    [Test]
    public static void EscapeProcessArguments_with_quotes()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments(["\"1\"2\"3\""]), Is.EqualTo("\"\\\"1\\\"2\\\"3\\\"\""));
    }

    [Test]
    public static void EscapeProcessArguments_with_slashes()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments(["1\\2\\\\3\\\\\\"]), Is.EqualTo("1\\2\\\\3\\\\\\"));
    }

    [Test]
    public static void EscapeProcessArguments_with_slashes_alwaysQuote()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments(["1\\2\\\\3\\\\\\"], true), Is.EqualTo("\"1\\2\\\\3\\\\\\\\\\\\\""));
    }

    [Test]
    public static void EscapeProcessArguments_slashes_followed_by_quotes()
    {
        Assert.That(ProcessRunner.EscapeProcessArguments(["\\\\\""]), Is.EqualTo("\"\\\\\\\\\\\"\""));
    }

    [Test]
    public static async Task Run_PythonScript_ReturnsCorrectOutput()
    {
        // Arrange
        var testDirectory = Path.GetDirectoryName(typeof(ProcessUtilsTests).Assembly.Location);
        var pythonScript = Path.Combine(testDirectory!, "test_add.py");
        
        // Find Python executable
        var pythonCommand = OperatingSystem.IsWindows() ? "python" : "python3";
        
        // Act
        var result = await ProcessRunner.Run(
            workingDirectory: testDirectory!,
            scriptPath: pythonCommand,
            arguments: new[] { pythonScript }
        );
        
        // Assert
        Assert.That(result.ExitCode, Is.EqualTo(0), $"Python script failed with error: {result.StdErr}");
        Assert.That(result.StdOut.Trim(), Is.EqualTo("42"), "Expected output to be 42 (21 + 21)");
    }

    [Test]
    public static async Task Run_PythonScript_WithEmptyWorkingDirectory_UsesScriptDirectory()
    {
        // Arrange
        // This simulates an Azure environment where we don't know the working directory
        // We use empty string for workingDirectory, and the script path determines the directory
        var testDirectory = Path.GetDirectoryName(typeof(ProcessUtilsTests).Assembly.Location);
        var pythonScript = Path.Combine(testDirectory!, "test_add.py");
        
        // Find Python executable
        var pythonCommand = OperatingSystem.IsWindows() ? "python" : "python3";
        
        // Act
        // Using empty string for workingDirectory - it will use the script's directory
        var result = await ProcessRunner.Run(
            workingDirectory: string.Empty,
            scriptPath: pythonCommand,
            arguments: new[] { pythonScript }
        );
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.Zero, $"Python script failed with error: {result.StdErr}");
            Assert.That(result.StdOut.Trim(), Is.EqualTo("42"), "Expected output to be 42 (21 + 21)");
        }
    }

    [Test]
    public static async Task Run_PythonScript_WithRelativePath_FromCurrentDirectory()
    {
        // Arrange
        // This simulates running from the bin directory in Azure
        // We change to the test directory and use a relative path
        var testDirectory = Path.GetDirectoryName(typeof(ProcessUtilsTests).Assembly.Location);
        var originalDirectory = Directory.GetCurrentDirectory();
        
        try
        {
            // Change to the test output directory (simulating being in the bin folder)
            Directory.SetCurrentDirectory(testDirectory!);
            
            // Use relative path to the script (as if it's in the current folder)
            var relativePythonScript = "./test_add.py";
            
            // Find Python executable
            var pythonCommand = OperatingSystem.IsWindows() ? "python" : "python3";
            
            // Act
            var result = await ProcessRunner.Run(
                workingDirectory: string.Empty,
                scriptPath: pythonCommand,
                arguments: new[] { relativePythonScript }
            );
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ExitCode, Is.Zero, $"Python script failed with error: {result.StdErr}");
                Assert.That(result.StdOut.Trim(), Is.EqualTo("42"), "Expected output to be 42 (21 + 21)");
            }
        }
        finally
        {
            // Restore original directory
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}