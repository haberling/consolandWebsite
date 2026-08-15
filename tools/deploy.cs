// Builds the site into docs/ for GitHub Pages.
//
// Usage (from anywhere): dotnet run tools/deploy.cs
//
// Steps:
//  - Runs the local tsc binary (node_modules/.bin) to compile src/ts -> src/js.
//    Invoked directly rather than via `npm run build`: npm's own Windows
//    .cmd shim resolves its module path relative to the process's working
//    directory rather than its own install location when spawned this way
//    (as opposed to from an interactive shell), which breaks it entirely.
//  - Runs `dotnet run tools/build-manifest.cs` to regenerate content/manifest.json.
//  - Clears docs/ and copies src/ (html/css/js/img) into it as the site root.
//  - Copies every content/** file (markdown, manifest.json, images, etc.) into
//    docs/content/, preserving the relative layout. content/**/.nav.json is a
//    build-time-only config file (consumed by build-manifest.cs) and is not copied.
//  - Writes docs/CNAME with the site's custom domain, so every deploy
//    re-asserts it regardless of what's in docs/ before the run.
//
// GitHub Pages must be configured to serve from the main branch's /docs
// folder (Settings -> Pages -> Source) for this to take effect once pushed.

using System.Diagnostics;
using System.Runtime.CompilerServices;

const string CustomDomain = "consoland.net";

var repoRoot = RepoRoot();
var srcRoot = Path.Combine(repoRoot, "src");
var contentRoot = Path.Combine(repoRoot, "content");
var docsRoot = Path.Combine(repoRoot, "docs");

try
{
    var tscBin = Path.Combine(repoRoot, "node_modules", ".bin", OperatingSystem.IsWindows() ? "tsc.cmd" : "tsc");
    if (!File.Exists(tscBin))
        throw new InvalidOperationException($"tsc binary not found at {tscBin} -- run `npm install` first.");

    RunOrThrow(repoRoot, tscBin, "");
    RunOrThrow(repoRoot, "dotnet", "run tools/build-manifest.cs");

    Console.WriteLine();
    if (Directory.Exists(docsRoot))
    {
        Console.WriteLine($"Clearing {docsRoot}");
        Directory.Delete(docsRoot, recursive: true);
    }
    Directory.CreateDirectory(docsRoot);

    Console.WriteLine($"Copying {srcRoot} -> {docsRoot} (excluding ts/, the uncompiled source)");
    CopyDirectory(srcRoot, docsRoot, skipDirNames: new[] { "ts" });

    var docsContentRoot = Path.Combine(docsRoot, "content");
    Console.WriteLine($"Copying content/** (excluding .nav.json) -> {docsContentRoot}");
    CopyContent(contentRoot, docsContentRoot);

    var cnamePath = Path.Combine(docsRoot, "CNAME");
    File.WriteAllText(cnamePath, CustomDomain + "\n");
    Console.WriteLine($"Wrote {cnamePath} ({CustomDomain})");

    Console.WriteLine();
    Console.WriteLine($"Deploy build complete: {docsRoot}");
    Console.WriteLine("Review the changes, then commit and push docs/ to publish.");
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

static void RunOrThrow(string workingDirectory, string fileName, string arguments)
{
    Console.WriteLine($"$ {fileName} {arguments}");

    var psi = new ProcessStartInfo
    {
        FileName = fileName,
        Arguments = arguments,
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
    };

    using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {fileName}");
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException($"{fileName} {arguments} exited with code {process.ExitCode}");
    }
}

static void CopyDirectory(string sourceDir, string destDir, IReadOnlyCollection<string>? skipDirNames = null)
{
    Directory.CreateDirectory(destDir);
    foreach (var file in Directory.GetFiles(sourceDir))
    {
        File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
    }
    foreach (var dir in Directory.GetDirectories(sourceDir))
    {
        var dirName = Path.GetFileName(dir);
        if (skipDirNames != null && skipDirNames.Contains(dirName))
        {
            Console.WriteLine($"  skipping {dir}");
            continue;
        }
        CopyDirectory(dir, Path.Combine(destDir, dirName), skipDirNames);
    }
}

static void CopyContent(string contentRoot, string destRoot)
{
    Directory.CreateDirectory(destRoot);
    foreach (var file in Directory.GetFiles(contentRoot, "*", SearchOption.AllDirectories))
    {
        var name = Path.GetFileName(file);
        if (name == ".nav.json") continue;

        var relative = Path.GetRelativePath(contentRoot, file);
        var dest = Path.Combine(destRoot, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(file, dest, overwrite: true);
    }
}
