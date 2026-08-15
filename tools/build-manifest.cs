// Scans a site's content directory and writes <that dir>/manifest.json for
// the site nav.
//
// Usage (from anywhere): dotnet run tools/build-manifest.cs [path]
//
// Finding the content directory (the "site root"):
//  - The content directory is located at runtime, not assumed relative to
//    this script, so the whole project can be moved around freely as long
//    as the site root marker (below) travels with it.
//  - A directory is the site root if it has a ".nav.json" containing
//    "siteRoot": true.
//  - The search starts from [path] if given, otherwise the current
//    directory. It looks for the marker there, then in each of that
//    directory's immediate subdirectories (one level down, never deeper --
//    this intentionally does not walk the whole drive). The first match
//    found is used as the content root.
//  - If no site root is found, this exits with an error explaining how to
//    create one: run `dotnet run tools/build-manifest.cs --init-site-root
//    [path]` (path defaults to the current directory) to write a
//    ".nav.json" with "siteRoot": true at that location.
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
//  - content/<dir>/.nav.json can also set "priority" (an int, default 0) to
//    control that top-level nav item's position: lower sorts earlier,
//    negatives allowed. Items with equal priority (including the default 0
//    among themselves and against loose top-level pages, which always use 0)
//    fall back to alphabetical-by-title. "Home" is always pinned first
//    regardless of priority.
//  - content/<dir>/.nav.json can also set "nonav" (bool, default false) to
//    exclude that directory from the nav entirely, e.g. for pages that
//    should be reachable by direct link only.
//  - Any top-level content/<dir>/ containing at least one .md file is kept
//    self-documenting: if it has no .nav.json, one is created with the
//    default fields ("priority": 0, "nonav": false); if it has one but
//    "nonav" is missing, that key is backfilled onto it (defaulted to
//    false) in place. Both cases write back to content/<dir>/.nav.json.
//
// This only recurses one level into content/ subdirectories; deeper nesting
// isn't supported yet.

using System.Text.Json;
using System.Text.Json.Serialization;

if (args.Length > 0 && args[0] == "--init-site-root")
{
    var targetDir = Path.GetFullPath(args.Length > 1 ? args[1] : Directory.GetCurrentDirectory());
    InitSiteRoot(targetDir);
    return 0;
}

var searchFrom = Path.GetFullPath(args.Length > 0 ? args[0] : Directory.GetCurrentDirectory());
if (!Directory.Exists(searchFrom))
{
    Console.Error.WriteLine($"error: path not found: {searchFrom}");
    return 1;
}

var contentRoot = FindSiteRoot(searchFrom);
if (contentRoot == null)
{
    Console.Error.WriteLine("error: no site root found.");
    Console.Error.WriteLine();
    Console.Error.WriteLine($"Looked for a \".nav.json\" with \"siteRoot\": true in:");
    Console.Error.WriteLine($"  {searchFrom}");
    Console.Error.WriteLine($"  {searchFrom}{Path.DirectorySeparatorChar}* (immediate subdirectories)");
    Console.Error.WriteLine();
    Console.Error.WriteLine("To mark a folder as the site root, run:");
    Console.Error.WriteLine("  dotnet run tools/build-manifest.cs --init-site-root [path]");
    Console.Error.WriteLine("(path defaults to the current directory if omitted)");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Or point this run at the right place instead of searching from here:");
    Console.Error.WriteLine("  dotnet run tools/build-manifest.cs [path]");
    return 1;
}
Console.WriteLine($"Scanning {contentRoot}");

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

static string? FindSiteRoot(string searchFrom)
{
    if (IsSiteRoot(searchFrom)) return searchFrom;

    foreach (var child in Directory.GetDirectories(searchFrom))
    {
        if (IsSiteRoot(child)) return child;
    }

    return null;
}

static bool IsSiteRoot(string dir)
{
    var markerPath = Path.Combine(dir, ".nav.json");
    if (!File.Exists(markerPath)) return false;

    var config = JsonSerializer.Deserialize(File.ReadAllText(markerPath), JsonContext.Default.NavOverride);
    return config?.SiteRoot == true;
}

static void InitSiteRoot(string dir)
{
    Directory.CreateDirectory(dir);

    var markerPath = Path.Combine(dir, ".nav.json");
    var config = File.Exists(markerPath)
        ? JsonSerializer.Deserialize(File.ReadAllText(markerPath), JsonContext.Default.NavOverride) ?? new NavOverride()
        : new NavOverride();

    config.SiteRoot = true;
    var json = JsonSerializer.Serialize(config, JsonContext.Default.NavOverride);
    File.WriteAllText(markerPath, json + "\n");

    Console.WriteLine($"Marked {dir} as the site root (wrote {markerPath}).");
}

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

    items.AddRange(rest.OrderBy(n => n.Priority).ThenBy(n => n.Title, StringComparer.OrdinalIgnoreCase));
    return items;
}

static NavItem? BuildDirectoryNavItem(string contentRoot, string dir)
{
    var dirName = Path.GetFileName(dir);
    var mdFiles = Directory.GetFiles(dir, "*.md", SearchOption.TopDirectoryOnly);
    Console.WriteLine($"  found {mdFiles.Length} .md file(s)");

    var config = LoadOrCreateNavOverride(dir, mdFiles.Length > 0);

    if (config.NoNav)
    {
        Console.WriteLine("  nonav: true -> excluding this directory from the nav entirely");
        return null;
    }

    var landingFile = mdFiles.FirstOrDefault(f =>
        string.Equals(NormalizeForMatch(Path.GetFileNameWithoutExtension(f)), NormalizeForMatch(dirName), StringComparison.OrdinalIgnoreCase));

    Console.WriteLine(landingFile != null
        ? $"  landing page: {Path.GetFileName(landingFile)} -> nav item will be clickable"
        : $"  no landing page (no .md file matching \"{dirName}\", dashes/spaces interchangeable) -> nav item will be a dropdown-only trigger");

    var candidates = mdFiles.Where(f => f != landingFile).ToList();
    var allowed = ApplyNavOverride(dir, candidates, config);

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
        Priority = config.Priority,
    };
}

static NavOverride LoadOrCreateNavOverride(string dir, bool hasMdFiles)
{
    var overridePath = Path.Combine(dir, ".nav.json");

    if (!File.Exists(overridePath))
    {
        if (!hasMdFiles) return new NavOverride();

        Console.WriteLine("  no .nav.json found -> creating default (\"priority\": 0, \"nonav\": false)");
        var created = new NavOverride();
        WriteNavOverride(overridePath, created);
        return created;
    }

    Console.WriteLine("  applying .nav.json override");
    var raw = File.ReadAllText(overridePath);
    var config = JsonSerializer.Deserialize(raw, JsonContext.Default.NavOverride) ?? new NavOverride();

    if (!raw.Contains("\"nonav\""))
    {
        Console.WriteLine("  .nav.json missing \"nonav\" -> backfilling default (false)");
        WriteNavOverride(overridePath, config);
    }

    return config;
}

static void WriteNavOverride(string path, NavOverride config)
{
    var json = JsonSerializer.Serialize(config, JsonContext.Default.NavOverride);
    File.WriteAllText(path, json + "\n");
}

static List<string> ApplyNavOverride(string dir, List<string> candidateFiles, NavOverride? config)
{
    if (config == null) return candidateFiles;

    if (config.Allow != null && config.Deny != null)
        throw new InvalidOperationException($"{Path.Combine(dir, ".nav.json")}: specify \"allow\" or \"deny\", not both.");

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

    [JsonIgnore]
    public int Priority { get; set; }
}

class NavOverride
{
    [JsonPropertyName("allow")]
    public List<string>? Allow { get; set; }

    [JsonPropertyName("deny")]
    public List<string>? Deny { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("nonav")]
    public bool NoNav { get; set; }

    [JsonPropertyName("siteRoot")]
    public bool SiteRoot { get; set; }
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
