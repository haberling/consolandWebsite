// Publishes docs/ (Canary's build output) to GitHub Pages.
//
// This is consolandWebsite's canary.json "publish" command -- `canary
// publish` already ran a fresh `canary build` before invoking this, so this
// script's only job is the git side: stage the output dir, refuse to do
// anything if the build produced no changes, commit with a message built
// from the actual file names that changed, and push.
//
// Usage: dotnet run tools/publish.cs
// (invoked by `canary publish` via canary.json's "publish" field, but safe
// to run directly too)

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

var repoRoot = RepoRoot();

var outputDir = Environment.GetEnvironmentVariable("CANARY_OUTPUT_DIR");
if (string.IsNullOrWhiteSpace(outputDir))
{
    outputDir = Path.Combine(repoRoot, "docs");
    Console.WriteLine($"note: CANARY_OUTPUT_DIR not set (not run via `canary publish`?), defaulting to {outputDir}");
}

var relativeOutputDir = Path.GetRelativePath(repoRoot, outputDir).Replace('\\', '/');
if (relativeOutputDir.StartsWith(".."))
{
    Console.Error.WriteLine($"error: output dir {outputDir} is not inside the git repo at {repoRoot}");
    return 1;
}

try
{
    RunOrThrow(repoRoot, "git", ["add", "--", relativeOutputDir]);

    var diff = RunCapture(repoRoot, "git", ["diff", "--cached", "--name-status", "--", relativeOutputDir]);
    var changes = ParseNameStatus(diff);

    if (changes.Count == 0)
    {
        Console.Error.WriteLine($"error: nothing to publish -- {relativeOutputDir} has no changes since the last commit.");
        return 1;
    }

    var message = BuildCommitMessage(changes);
    Console.WriteLine();
    Console.WriteLine("Commit message:");
    Console.WriteLine(message);
    Console.WriteLine();

    RunOrThrow(repoRoot, "git", ["commit", "-m", message, "--", relativeOutputDir]);
    RunOrThrow(repoRoot, "git", ["push"]);

    Console.WriteLine();
    Console.WriteLine("Published.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    return 1;
}

static string RepoRoot()
{
    var scriptPath = ThisFilePath();
    return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(scriptPath)!, ".."));
}

static string ThisFilePath([CallerFilePath] string path = "") => path;

static List<Change> ParseNameStatus(string nameStatusOutput)
{
    var changes = new List<Change>();
    foreach (var line in nameStatusOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries))
    {
        var parts = line.TrimEnd('\r').Split('\t');
        if (parts.Length < 2) continue;

        var status = parts[0][0];
        var display = status is 'R' or 'C' && parts.Length >= 3
            ? $"{parts[1]} -> {parts[2]}"
            : parts[1];
        changes.Add(new Change(status, display));
    }
    return changes;
}

static string BuildCommitMessage(List<Change> changes)
{
    var added = changes.Where(c => c.Status == 'A').Select(c => c.Path).ToList();
    var modified = changes.Where(c => c.Status == 'M').Select(c => c.Path).ToList();
    var deleted = changes.Where(c => c.Status == 'D').Select(c => c.Path).ToList();
    var renamed = changes.Where(c => c.Status is 'R' or 'C').Select(c => c.Path).ToList();

    var subject = changes.Count <= 3
        ? "Publish: " + string.Join(", ", changes.Select(c => c.Path))
        : $"Publish: {added.Count} added, {modified.Count} updated, {deleted.Count} deleted, {renamed.Count} renamed";

    var body = new StringBuilder();
    AppendSection(body, "Added", added);
    AppendSection(body, "Updated", modified);
    AppendSection(body, "Deleted", deleted);
    AppendSection(body, "Renamed", renamed);

    return body.Length == 0 ? subject : $"{subject}\n\n{body}".TrimEnd();
}

static void AppendSection(StringBuilder body, string label, List<string> paths)
{
    if (paths.Count == 0) return;
    if (body.Length > 0) body.Append('\n');
    body.Append(label).Append(":\n");
    foreach (var path in paths)
    {
        body.Append("- ").Append(path).Append('\n');
    }
}

// Collapses a multi-line argument (the commit message) down to something
// that reads as one line in the "$ git commit -m ..." echo, instead of
// blowing the whole message out across the console mid-command.
static string DisplayArg(string arg) =>
    arg.Contains('\n') ? $"\"{arg[..arg.IndexOf('\n')]}...\"" : arg;

static void RunOrThrow(string workingDirectory, string fileName, string[] arguments)
{
    Console.WriteLine($"$ {fileName} {string.Join(' ', arguments.Select(DisplayArg))}");

    var psi = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
    };
    foreach (var arg in arguments) psi.ArgumentList.Add(arg);

    using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {fileName}");
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException($"{fileName} {string.Join(' ', arguments.Select(DisplayArg))} exited with code {process.ExitCode}");
    }
}

static string RunCapture(string workingDirectory, string fileName, string[] arguments)
{
    var psi = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
    };
    foreach (var arg in arguments) psi.ArgumentList.Add(arg);

    using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {fileName}");
    var output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException($"{fileName} {string.Join(' ', arguments.Select(DisplayArg))} exited with code {process.ExitCode}");
    }
    return output;
}

// One entry from `git diff --name-status`: a status code (A/M/D/R100/...)
// plus the path(s) it applies to (two paths for a rename/copy).
readonly record struct Change(char Status, string Path);
