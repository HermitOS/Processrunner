/* This file is originally from the NUnit3TestAdapter project, and modified here */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessUtils;

public static class ProcessRunner
{
    public static Task<ProcessRunResult> Run(
        string workingDirectory,
        string scriptPath,
        IEnumerable<string>? arguments = null,
        CancellationToken cancellationToken = default)
        => RunAsync(workingDirectory, scriptPath, arguments, cancellationToken);

    public static async Task<ProcessRunResult> RunAsync(
        string workingDirectory,
        string scriptPath,
        IEnumerable<string>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (workingDirectory == string.Empty)
        {
            var derived = Path.GetDirectoryName(scriptPath);
            workingDirectory = string.IsNullOrEmpty(derived)
                ? Directory.GetCurrentDirectory()
                : derived;
        }
        if (!Path.IsPathRooted(workingDirectory))
            throw new ArgumentException("Working directory must not be relative.", nameof(workingDirectory));

        if (string.IsNullOrWhiteSpace(scriptPath))
            throw new ArgumentException("Script file name must be specified.", nameof(scriptPath));

        var escapedArguments = arguments is null ? string.Empty : EscapeProcessArguments(arguments, alwaysQuote: false);

        using var process = new Process
        {
            StartInfo =
            {
                UseShellExecute = false,
                WorkingDirectory = workingDirectory,
                FileName = scriptPath,
                Arguments = escapedArguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        var outClosed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var errClosed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data is null)
            {
                outClosed.TrySetResult(true);
                return;
            }
            if (stdout.Length > 0) stdout.AppendLine();
            stdout.Append(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data is null)
            {
                errClosed.TrySetResult(true);
                return;
            }
            if (stderr.Length > 0) stderr.AppendLine();
            stderr.Append(e.Data);
        };

        using var _ = cancellationToken.Register(() =>
        {
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
            catch { /* ignore */ }
        });

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        await Task.WhenAll(outClosed.Task, errClosed.Task).ConfigureAwait(false);

        return new ProcessRunResult(
            scriptPath,
            escapedArguments,
            process.ExitCode,
            stdout.ToString(),
            stderr.ToString());
    }

    private static readonly char[] CharsThatRequireQuoting = { ' ', '"' };
    private static readonly char[] CharsThatRequireEscaping = { '\\', '"' };

    /// <summary>
    /// Escapes arbitrary values so that the process receives the exact string you intend and injection is impossible.
    /// Spec: https://msdn.microsoft.com/en-us/library/bb776391.aspx.
    /// </summary>
    public static string EscapeProcessArguments(IEnumerable<string> literalValues, bool alwaysQuote = false)
    {
        if (literalValues is null) throw new ArgumentNullException(nameof(literalValues));

        using var en = literalValues.GetEnumerator();
        if (!en.MoveNext()) return string.Empty;

        var builder = new StringBuilder();

        while (true)
        {
            EscapeProcessArgument(builder, en.Current, alwaysQuote);
            if (!en.MoveNext()) break;
            builder.Append(' ');
        }

        return builder.ToString();
    }

    private static void EscapeProcessArgument(StringBuilder builder, string literalValue, bool alwaysQuote)
    {
        if (string.IsNullOrEmpty(literalValue))
        {
            builder.Append("\"\"");
            return;
        }

        if (literalValue.IndexOfAny(CharsThatRequireQuoting) == -1) // Happy path
        {
            if (!alwaysQuote)
            {
                builder.Append(literalValue);
                return;
            }
            if (literalValue[literalValue.Length - 1] != '\\')
            {
                builder.Append('"').Append(literalValue).Append('"');
                return;
            }
        }

        builder.Append('"');

        var nextPosition = 0;
        while (true)
        {
            var nextEscapeChar = literalValue.IndexOfAny(CharsThatRequireEscaping, nextPosition);
            if (nextEscapeChar == -1) break;

            builder.Append(literalValue, nextPosition, nextEscapeChar - nextPosition);
            nextPosition = nextEscapeChar + 1;

            switch (literalValue[nextEscapeChar])
            {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    var numBackslashes = 1;
                    while (nextPosition < literalValue.Length && literalValue[nextPosition] == '\\')
                    {
                        numBackslashes++;
                        nextPosition++;
                    }
                    if (nextPosition == literalValue.Length || literalValue[nextPosition] == '"')
                        numBackslashes <<= 1;

                    for (; numBackslashes != 0; numBackslashes--)
                        builder.Append('\\');
                    break;
            }
        }

        builder.Append(literalValue, nextPosition, literalValue.Length - nextPosition);
        builder.Append('"');
    }
}
