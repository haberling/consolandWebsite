// Scans content/ and writes content/manifest.json for the site nav.
//
// Usage (from anywhere): dotnet run tools/build-manifest.cs
//
// Rules:
//  - content/home.md, if present, always becomes a "Home" nav item pinned
//    first, ahead of the alphabetically-sorted rest.
//  - A loose top-level content/<name>.md becomes a flat nav item.
//  - A top-level content/<dir>/ becomes a nav item:
//      - If content/<dir>/<Dir>.md exists, that page is the landing page:
//        the nav item is clickable and links to it. The match is
//        case-insensitive and treats "-" and " " as interchangeable on both
//        sides, so dir "blog-archive" matches "blog-archive.md" or
//        "Blog Archive.md" equally.
//      - Every other .md file directly in <dir>/ becomes a dropdown child
//        (the landing page itself is never duplicated into its own dropdown).
//      - If there's no landing page, the nav item is a non-navigating
//        dropdown trigger over the same child list.
//      - A directory that ends up with no landing page and no children is
//        omitted entirely.
//  - Each item's title is its file's first "# Heading" line; falls back to
//    a title-cased version of the filename/directory name if none is found.
//  - content/<dir>/.nav.json can restrict which files become children:
//      { "allow": ["a.md", "b.md"] }   -- only these
//      { "deny": ["draft.md"] }        -- everything except these
//    Specifying both allow and deny in the same file is an error.
//
// This only recurses one level into content/ subdirectories; deeper nesting
// isn't supported yet.

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

var contentRoot = Path.Combine(RepoRoot(), "content");
Console.WriteLine($"Scanning {contentRoot}");

if (!Directory.Exists(contentRoot))
{
    Console.Error.WriteLine($"error: content/ not found at {contentRoot}");
    return 1;
}

try
{
    var nav = BuildNav(contentRoot);
    var manifest = new Manifest { Nav = nav };
    var json = JsonSerializer.Serialize(manifest, JsonContext.Default.Manifest);

    var outPath = Path.Combine(contentRoot, "manifest.json");
    File.WriteAllText(outPath, json + "\n");

    Console.WriteLine();
    Console.WriteLine($"Nav summary ({nav.Count} top-level item(s)):");
    foreach (var item in nav)
    {
        var kind = item.Path != null ? item.Path : "(no landing page)";
        Console.WriteLine($"  - {item.Title} -> {kind}");
        foreach (var child in item.Children ?? new List<NavItem>())
        {
            Console.WriteLine($"      - {child.Title} -> {child.Path}");
        }
    }

    Console.WriteLine();
    Console.WriteLine($"Wrote {outPath}");
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

static List<NavItem> BuildNav(string contentRoot)
{
    var items = new List<NavItem>();

    var homeFile = Path.Combine(contentRoot, "home.md");
    if (File.Exists(homeFile))
    {
        Console.WriteLine("Found home.md -> pinning \"Home\" first in the nav");
        items.Add(new NavItem { Title = "Home", Path = "home" });
    }
    else
    {
        Console.WriteLine("No home.md found -> no Home nav item");
    }

    var rest = new List<NavItem>();

    foreach (var file in Directory.GetFiles(contentRoot, "*.md", SearchOption.TopDirectoryOnly))
    {
        var slug = Path.GetFileNameWithoutExtension(file);
        if (string.Equals(slug, "home", StringComparison.OrdinalIgnoreCase)) continue;

        var title = TitleFromFile(file, slug);
        Console.WriteLine($"Found top-level page {Path.GetFileName(file)} -> \"{title}\"");
        rest.Add(new NavItem
        {
            Title = title,
            Path = ToContentPath(contentRoot, file),
        });
    }

    foreach (var dir in Directory.GetDirectories(contentRoot))
    {
        Console.WriteLine($"Scanning directory {Path.GetFileName(dir)}/");
        var item = BuildDirectoryNavItem(contentRoot, dir);
        if (item == null)
        {
            Console.WriteLine($"  -> no landing page and no children left after filtering; omitting from nav");
        }
        else
        {
            rest.Add(item);
        }
    }

    items.AddRange(rest.OrderBy(n => n.Title, StringComparer.OrdinalIgnoreCase));
    return items;
}

static NavItem? BuildDirectoryNavItem(string contentRoot, string dir)
{
    var dirName = Path.GetFileName(dir);
    var mdFiles = Directory.GetFiles(dir, "*.md", SearchOption.TopDirectoryOnly);
    Console.WriteLine($"  found {mdFiles.Length} .md file(s)");

    var landingFile = mdFiles.FirstOrDefault(f =>
        string.Equals(NormalizeForMatch(Path.GetFileNameWithoutExtension(f)), NormalizeForMatch(dirName), StringComparison.OrdinalIgnoreCase));

    Console.WriteLine(landingFile != null
        ? $"  landing page: {Path.GetFileName(landingFile)} -> nav item will be clickable"
        : $"  no landing page (no .md file matching \"{dirName}\", dashes/spaces interchangeable) -> nav item will be a dropdown-only trigger");

    var candidates = mdFiles.Where(f => f != landingFile).ToList();
    var allowed = ApplyNavOverride(dir, candidates);

    var children = allowed
        .Select(f => new NavItem { Title = TitleFromFile(f, Path.GetFileNameWithoutExtension(f)), Path = ToContentPath(contentRoot, f) })
        .OrderBy(n => n.Title, StringComparer.OrdinalIgnoreCase)
        .ToList();

    Console.WriteLine($"  {children.Count} child page(s) in dropdown: {(children.Count > 0 ? string.Join(", ", children.Select(c => c.Title)) : "(none)")}");

    string title;
    string? path = null;

    if (landingFile != null)
    {
        title = TitleFromFile(landingFile, dirName);
        path = ToContentPath(contentRoot, landingFile);
    }
    else
    {
        title = TitleCase(dirName);
    }

    if (landingFile == null && children.Count == 0) return null;

    return new NavItem
    {
        Title = title,
        Path = path,
        Children = children.Count > 0 ? children : null,
    };
}

static List<string> ApplyNavOverride(string dir, List<string> candidateFiles)
{
    var overridePath = Path.Combine(dir, ".nav.json");
    if (!File.Exists(overridePath)) return candidateFiles;

    Console.WriteLine("  applying .nav.json override");
    var config = JsonSerializer.Deserialize(File.ReadAllText(overridePath), JsonContext.Default.NavOverride);

    if (config == null) return candidateFiles;

    if (config.Allow != null && config.Deny != null)
        throw new InvalidOperationException($"{overridePath}: specify \"allow\" or \"deny\", not both.");

    if (config.Allow != null)
    {
        var allowSet = new HashSet<string>(config.Allow, StringComparer.OrdinalIgnoreCase);
        var result = candidateFiles.Where(f => allowSet.Contains(Path.GetFileName(f))).ToList();
        var excluded = candidateFiles.Except(result).Select(f => Path.GetFileName(f));
        Console.WriteLine($"    allow list keeps: {string.Join(", ", result.Select(Path.GetFileName))}");
        if (excluded.Any()) Console.WriteLine($"    excluded (not in allow list): {string.Join(", ", excluded)}");
        return result;
    }

    if (config.Deny != null)
    {
        var denySet = new HashSet<string>(config.Deny, StringComparer.OrdinalIgnoreCase);
        var result = candidateFiles.Where(f => !denySet.Contains(Path.GetFileName(f))).ToList();
        var excluded = candidateFiles.Except(result).Select(f => Path.GetFileName(f));
        if (excluded.Any()) Console.WriteLine($"    excluded (deny list): {string.Join(", ", excluded)}");
        return result;
    }

    return candidateFiles;
}

static string NormalizeForMatch(string s) => s.Replace('-', ' ');

static string TitleFromFile(string file, string fallbackSlug)
{
    foreach (var line in File.ReadLines(file))
    {
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith("# ")) return trimmed[2..].Trim();
    }
    return TitleCase(fallbackSlug);
}

static string TitleCase(string slug)
{
    var words = slug.Replace('-', ' ').Replace('_', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
    return string.Join(' ', words.Select(w => char.ToUpperInvariant(w[0]) + w[1..]));
}

static string ToContentPath(string contentRoot, string file)
{
    var rel = Path.GetRelativePath(contentRoot, file).Replace('\\', '/');
    return rel[..^3]; // strip ".md"
}

class Manifest
{
    [JsonPropertyName("nav")]
    public List<NavItem> Nav { get; set; } = new();
}

class NavItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("children")]
    public List<NavItem>? Children { get; set; }
}

class NavOverride
{
    [JsonPropertyName("allow")]
    public List<string>? Allow { get; set; }

    [JsonPropertyName("deny")]
    public List<string>? Deny { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Manifest))]
[JsonSerializable(typeof(NavOverride))]
partial class JsonContext : JsonSerializerContext
{
}
