// Toolchain tool -- expands a `[!BLOG LIST|<List Title>|<relative-path>]`
// tag into a single `bloglist` widget block (a file-explorer style list/tile
// view, see widgets/bloglist.html) with one item per post found under
// <relative-path>. No heading is emitted; <List Title> becomes the widget's
// accessible label. Registered as "blog-list-generator" in canary.jsonc's
// "tools" registry; applied via content/blog/.toolchain.json.
//
// Contract (see docsite/content/guide/toolchain.md): read a page's raw
// markdown on stdin, write the transformed markdown on stdout. Runs with
// cwd = site root, so "content/..." paths resolve directly.
//
// A post's frontmatter is a block of bare "key: value" lines at the top of
// its file, optionally wrapped in a pair of lone "---" lines (Canary
// itself doesn't parse this block at all yet -- it's read directly here).
//
// <relative-path> is resolved against the *current page's own directory*
// (content/ + CANARY_ROUTE_PATH), matching how the existing tag in
// content/blog/index.md already writes "blog-new" to mean
// content/blog/blog-new, not content/blog-new. This only resolves
// correctly when the tag lives in an index.md (a folder's own landing
// page) -- the only place this tag is meant to be used.
//
// A post's URL (the "Root" field below) is computed with the exact same
// rule Canary.Core.Build.ContentScanner uses to turn a content file into a
// route: an index.md maps to its containing directory; anything else maps
// to its own content-root-relative path with ".md" stripped. That's what
// makes `!url "<Root>"` in the generated postlink block resolve to a real,
// working page -- it's not just "the file's location," it's the file's
// actual route.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

// Canary writes/reads this tool's stdio as UTF-8 (see ToolchainRunner) --
// match that here, since Console.In/Out otherwise default to the OS
// console codepage (commonly not UTF-8 on Windows), which corrupts any
// non-ASCII character (math symbols, smart quotes, accents) passing through.
Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);

var routePath = Environment.GetEnvironmentVariable("CANARY_ROUTE_PATH") ?? "";
var pageDir = routePath.Length == 0
    ? "content"
    : Path.Combine("content", routePath.Replace('/', Path.DirectorySeparatorChar));

var input = Console.In.ReadToEnd();
var lines = input.Replace("\r\n", "\n").Split('\n');
var output = new StringBuilder();

for (var i = 0; i < lines.Length; i++)
{
    var line = lines[i];

    var tagMatch = BlogListTagRegex().Match(line.Trim());
    if (!tagMatch.Success)
    {
        output.Append(line);
        if (i < lines.Length - 1) output.Append('\n');
        continue;
    }

    var listTitle = tagMatch.Groups[1].Value.Trim();
    var relativePath = tagMatch.Groups[2].Value.Trim();
    var posts = CollectPosts(Path.Combine(pageDir, relativePath), routePath);

    var folderRoute = RouteFromContentPath(Path.Combine(pageDir, relativePath, "index.md"));
    output.Append(RenderBlogList(listTitle, posts, folderRoute));
    if (i < lines.Length - 1) output.Append('\n');
}

Console.Out.Write(output.ToString());
return;

// ownRoutePath excludes the page the tag itself lives on from its own
// generated list -- needed now that a list can scan its own directory
// (relativePath "."), e.g. content/blog/blog-archive/index.md listing its
// sibling posts: without this, the index would show up as one of its own
// "posts" (title "Blog Archive", no date, since index.md has neither a
// frontmatter block nor any reason to).
static List<BlogPostEntry> CollectPosts(string folder, string ownRoutePath)
{
    if (!Directory.Exists(folder))
    {
        throw new InvalidOperationException($"[blog-list-generator] folder not found: {folder}");
    }

    var posts = new List<BlogPostEntry>();
    foreach (var file in Directory.GetFiles(folder, "*.md", SearchOption.AllDirectories))
    {
        var root = RouteFromContentPath(file);
        if (root == ownRoutePath) continue;

        var raw = File.ReadAllText(file);
        var title = TitleFromMarkdown(raw, Path.GetFileNameWithoutExtension(file));
        var authorDate = AuthorDateFromMarkdown(raw);
        var (bytes, image) = MeasurePost(file, raw, folder);
        posts.Add(new BlogPostEntry(title, authorDate, root, bytes, image));
    }

    return posts
        .OrderByDescending(p => p.AuthorDate ?? DateTime.MinValue)
        .ToList();
}

// A post's "file size" is what a reader actually downloads for it: the
// markdown source plus every distinct local image it references. Returns
// that total and the site-root-relative path of the first local image
// (null if the post has none). Image paths in posts are inconsistent
// (site-root-relative, post-relative, or stale folder names), so each is
// tried as-is, then relative to the post, then by bare filename anywhere
// under the list folder.
static (long Bytes, string? FirstImage) MeasurePost(string file, string raw, string listFolder)
{
    var total = new FileInfo(file).Length;
    string? first = null;
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (Match m in ImageRegex().Matches(raw))
    {
        var reference = m.Groups.Cast<Group>().Skip(1).First(g => g.Success).Value.Trim();
        if (reference.Length == 0 || reference.Contains("://") || reference.StartsWith("data:")) continue;

        var resolved = ResolveImage(reference, file, listFolder);
        if (resolved is null || !seen.Add(resolved)) continue;

        total += new FileInfo(resolved).Length;
        first ??= Path.GetRelativePath(".", resolved).Replace('\\', '/');
    }
    return (total, first);
}

static string? ResolveImage(string reference, string postFile, string listFolder)
{
    var clean = reference.Split('#')[0].Split('?')[0].TrimStart('/');
    var candidates = new[]
    {
        clean,
        Path.Combine(Path.GetDirectoryName(postFile)!, clean),
    };
    foreach (var candidate in candidates)
    {
        if (File.Exists(candidate)) return candidate;
    }

    var name = Path.GetFileName(clean);
    return name.Length == 0
        ? null
        : Directory.EnumerateFiles(listFolder, name, SearchOption.AllDirectories).FirstOrDefault();
}

// Mirrors Canary.Core.Build.PageBuilder.TitleFromMarkdown -- first "# "
// line wins, else a title-cased filename, so a listed post's title always
// matches what the post's own page actually shows in its <title>/<h1>.
static string TitleFromMarkdown(string source, string fallbackSlug)
{
    foreach (var line in source.Split('\n'))
    {
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith("# ")) return trimmed[2..].TrimEnd('\r').Trim();
    }
    return TitleCase(fallbackSlug);
}

static string TitleCase(string slug)
{
    var words = slug.Replace('-', ' ').Replace('_', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
    return string.Join(' ', words.Select(w => char.ToUpperInvariant(w[0]) + w[1..]));
}

// Frontmatter here is a block of bare "key: value" lines at the very top
// of the file, optionally opened by a lone "---" line and closed by
// another. Scanning stops the moment a real heading shows up, so a post
// with no frontmatter at all (body starts straight at "# Title") never
// gets misread.
static DateTime? AuthorDateFromMarkdown(string source)
{
    var lines = source.Split('\n');
    var start = lines.Length > 0 && lines[0].TrimEnd('\r').Trim() == "---" ? 1 : 0;

    for (var i = start; i < lines.Length; i++)
    {
        var line = lines[i].TrimEnd('\r');
        var trimmed = line.Trim();
        if (trimmed == "---") return null; // frontmatter closed with no authorDate field
        if (HeadingRegex().IsMatch(trimmed)) return null; // no frontmatter block present

        var colon = line.IndexOf(':');
        if (colon <= 0) continue;

        var key = line[..colon].Trim();
        if (!key.Equals("authorDate", StringComparison.OrdinalIgnoreCase)) continue;

        var value = line[(colon + 1)..].Trim().Trim('"', '\'');
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }
    return null;
}

// Same rule as Canary.Core.Build.ContentScanner: an index.md's route is its
// containing directory; anything else is its own content-root-relative
// path with ".md" stripped. Assumes cwd is the site root and "content" is
// the content root, same assumption ContentScanner itself runs under.
static string RouteFromContentPath(string file)
{
    var relative = Path.GetRelativePath("content", file).Replace('\\', '/');
    return relative.EndsWith("/index.md", StringComparison.OrdinalIgnoreCase) || relative.Equals("index.md", StringComparison.OrdinalIgnoreCase)
        ? relative[..^"index.md".Length].TrimEnd('/')
        : relative[..^3];
}

static string RenderBlogList(string listTitle, List<BlogPostEntry> posts, string folderRoute)
{
    var block = new StringBuilder();

    // No heading is emitted -- the widget is its own titled window. The tag's list
    // title survives as the widget's accessible label; the title bar shows a path.
    var windowTitle = "C:\\" + folderRoute.Replace('/', '\\');

    block.Append("```bloglist").Append('\n');
    block.Append("label: ").Append(YamlQuote(listTitle)).Append('\n');
    block.Append("title: ").Append(YamlQuote(windowTitle)).Append('\n');
    block.Append("count: ").Append(YamlQuote(posts.Count.ToString(CultureInfo.InvariantCulture))).Append('\n');
    block.Append("total: ").Append(YamlQuote(FormatSize(posts.Sum(p => p.Bytes)))).Append('\n');
    block.Append("items:").Append('\n');
    foreach (var post in posts)
    {
        block.Append("  - title: ").Append(YamlQuote(post.Title)).Append('\n');
        block.Append("    url: !url ").Append(YamlQuote(post.Root)).Append('\n');
        if (post.AuthorDate is { } date)
        {
            block.Append("    date: ").Append(YamlQuote(date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture))).Append('\n');
            block.Append("    sortdate: ").Append(YamlQuote(date.ToString("yyyyMMdd", CultureInfo.InvariantCulture))).Append('\n');
        }
        block.Append("    bytes: ").Append(YamlQuote(post.Bytes.ToString(CultureInfo.InvariantCulture))).Append('\n');
        block.Append("    size: ").Append(YamlQuote(FormatSize(post.Bytes))).Append('\n');
        if (post.Image is { } image)
        {
            block.Append("    image: !url ").Append(YamlQuote(image)).Append('\n');
        }
    }
    block.Append("```").Append('\n');

    return block.ToString().TrimEnd('\n');
}

// Explorer-style: whole KB rounded up, MB with one decimal past 1 MB.
static string FormatSize(long bytes) => bytes >= 1024 * 1024
    ? $"{bytes / 1024.0 / 1024.0:0.0} MB"
    : $"{Math.Max(1, (bytes + 1023) / 1024):N0} KB";

// The widget YAML subset has no escape sequences (Canary.Core.Templating.
// YamlParser.ParseScalarText just strips the surrounding quote chars
// as-is) -- so instead of escaping, pick whichever quote character isn't
// present in the value. Falls back to double quotes if both appear, which
// is a known, accepted limitation for that rare case.
static string YamlQuote(string value)
{
    var quote = value.Contains('"') && !value.Contains('\'') ? '\'' : '"';
    return $"{quote}{value}{quote}";
}


static Regex HeadingRegex() => new(@"^(#{1,6})\s+");
// Markdown "![alt](path)", an HTML <img src="...">, or a widget "src:" line
// (slideshows: `- src: !url "content/..."`), in file order.
static Regex ImageRegex() => new(@"!\[[^\]]*\]\(\s*<?([^)\s>]+)[^)]*\)|<img[^>]+src=[""']([^""']+)[""']|^\s*-?\s*src:\s*(?:!url\s*)?[""']?([^""'\s]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
static Regex BlogListTagRegex() => new(@"^\[!BLOG LIST\|([^|]+)\|([^\]]+)\]$");

readonly record struct BlogPostEntry(string Title, DateTime? AuthorDate, string Root, long Bytes, string? Image);
