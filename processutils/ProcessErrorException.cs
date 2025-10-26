/* This file is originally from the NUnit3TestAdapter project, and modified here */
using System;
using System.Text;

namespace ProcessUtils;

public sealed class ProcessErrorException(ProcessRunResult result) : Exception(BuildMessage(result))
{
    public ProcessRunResult Result { get; } = result;

    private static string BuildMessage(ProcessRunResult result)
    {
        var builder = new StringBuilder();
        builder.Append("Process ‘").Append(result.ProcessName);
        builder.Append("’ exited with code ").Append(result.ExitCode).Append('.');
        builder.AppendLine().Append("Executable: ").Append(result.FileName);

        if (!string.IsNullOrWhiteSpace(result.Arguments))
        {
            builder.AppendLine().Append("Arguments: ").Append(result.Arguments);
        }

        var hasStdErr = !string.IsNullOrWhiteSpace(result.StdErr);

        if (hasStdErr || !string.IsNullOrWhiteSpace(result.StdOut))
        {
            builder.AppendLine().Append(hasStdErr ? "Stderr:" : "Stdout:");
            builder.AppendLine().Append(hasStdErr ? result.StdErr : result.StdOut);
        }

        return builder.ToString();
    }
}