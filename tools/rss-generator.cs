// Toolchain tool -- writes docs/rss.xml from the curated rss.md at the
// site root, and expands a `[!WHATS NEW]` tag into a single bloglist
// widget (one item per feed entry, no heading). Registered as "rss-generator" in
// canary.jsonc's "tools" registry; applied via content/.toolchain.json
// so it runs once per full build, on the home page. There is no cascading.
//
// rss.md is the source of truth, not a scan of blog-new or product pages.
// Add an item when a post ships or a piece of software becomes available.
// See rss.md itself for the item format.
//
// lastBuildDate is the newest item's date, not DateTime.UtcNow -- a
// rebuild with no feed edits must produce a byte-identical rss.xml
// (Canary's own write-only-if-different rule, applied here too) or git
// will see a noisy docs/ diff on every publish.
//
// Contract (see docsite/content/guide/toolchain.md): read a page's raw
// markdown on stdin, write the transformed markdown on stdout. Runs with
// cwd = site root.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);

var input = Console.In.ReadToEnd();

const string SourcePath = "rss.md";
const string OutputPath = "docs/rss.xml";
const string BaseUrl = "https://consoland.net";
const string FeedPath = "/rss.xml";

if (!File.Exists(SourcePath))
{
    throw new InvalidOperationException($"[rss-generator] feed document not found: {SourcePath}");
}

var items = ParseFeed(File.ReadAllText(SourcePath));
WriteFeedIfChanged(OutputPath, RenderRss(BaseUrl, FeedPath, items));

Console.Out.Write(ExpandWhatsNew(input, items));
return;

// Each "## Title" starts an item. Bare "key: value" lines immediately
// under it are fields (date, url, author). The first paragraph after
// those fields is the summary. Prose before the first "##" is ignored
// (the file's own instructions). Same-day items keep source order;
// OrderByDescending is stable.
static List<RssItem> ParseFeed(string source)
{
    var items = new List<RssItem>();
    var lines = source.Replace("\r\n", "\n").Split('\n');

    string? title = null;
    string? url = null;
    string? author = null;
    DateTime? date = null;
    string? summary = null;
    var readingFields = false;
    var readingSummary = false;

    void Flush()
    {
        if (title is null) return;
        if (string.IsNullOrWhiteSpace(url) || date is null)
        {
            throw new InvalidOperationException(
                $"[rss-generator] item \"{title}\" needs both date and url.");
        }
        items.Add(new RssItem(title, summary, author, date, url));
        title = url = author = summary = null;
        date = null;
        readingFields = readingSummary = false;
    }

    foreach (var raw in lines)
    {
        var trimmed = raw.TrimEnd('\r').Trim();

        if (trimmed.StartsWith("## "))
        {
            Flush();
            title = trimmed[3..].Trim();
            if (title.Length == 0)
            {
                throw new InvalidOperationException("[rss-generator] empty ## heading.");
            }
            readingFields = true;
            continue;
        }

        if (title is null) continue;

        if (readingFields)
        {
            if (trimmed.Length == 0) continue;

            var colon = trimmed.IndexOf(':');
            if (colon > 0 && IsFieldName(trimmed[..colon].Trim()))
            {
                var key = trimmed[..colon].Trim();
                var value = trimmed[(colon + 1)..].Trim().Trim('"', '\'');
                if (key.Equals("url", StringComparison.OrdinalIgnoreCase))
                {
                    url = value.Length > 0 ? value : null;
                }
                else if (key.Equals("author", StringComparison.OrdinalIgnoreCase))
                {
                    author = value.Length > 0 ? value : null;
                }
                else if (key.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    {
                        throw new InvalidOperationException(
                            $"[rss-generator] item \"{title}\" has an unreadable date: {value}");
                    }
                    date = DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
                }
                continue;
            }

            readingFields = false;
            readingSummary = true;
        }

        if (readingSummary)
        {
            if (trimmed.Length == 0)
            {
                if (summary is not null) readingSummary = false;
                continue;
            }
            summary = summary is null ? trimmed : $"{summary} {trimmed}";
        }
    }

    Flush();

    return items
        .OrderByDescending(i => i.Date ?? DateTime.MinValue)
        .ToList();
}

static bool IsFieldName(string key) =>
    key.Equals("date", StringComparison.OrdinalIgnoreCase)
    || key.Equals("url", StringComparison.OrdinalIgnoreCase)
    || key.Equals("author", StringComparison.OrdinalIgnoreCase);

// A lone `[!WHATS NEW]` line becomes a bloglist
// widget (same widget the blog list uses, no heading). Anything else passes through.
static string ExpandWhatsNew(string markdown, List<RssItem> items)
{
    var lines = markdown.Replace("\r\n", "\n").Split('\n');
    var output = new StringBuilder();
    for (var i = 0; i < lines.Length; i++)
    {
        if (WhatsNewTagRegex().IsMatch(lines[i].Trim()))
        {
            output.Append(RenderWhatsNew(items));
        }
        else
        {
            output.Append(lines[i]);
        }
        if (i < lines.Length - 1) output.Append('\n');
    }
    return output.ToString();
}

// One `bloglist` widget (the same Explorer-style list/tile window the blog
// index uses, see widgets/bloglist.html) for the whole feed. The feed is
// curated by hand in rss.md, so the item set and order-by-date come from
// there; only size and first image are computed, from the page each item's
// url points at. An absolute url (a GitHub release, say) has no local page,
// so it gets no size and no image.
static string RenderWhatsNew(List<RssItem> items)
{
    var measured = items
        .Select(item => (Item: item, Page: MeasurePage(item.Url!)))
        .ToList();

    var block = new StringBuilder();
    block.Append("```bloglist").Append('\n');
    block.Append("label: ").Append(YamlQuote("What's New")).Append('\n');
    block.Append("title: ").Append(YamlQuote("C:\\whats-new")).Append('\n');
    block.Append("count: ").Append(YamlQuote(items.Count.ToString(CultureInfo.InvariantCulture))).Append('\n');
    block.Append("total: ").Append(YamlQuote(FormatSize(measured.Sum(m => m.Page.Bytes)))).Append('\n');
    block.Append("items:").Append('\n');
    foreach (var (item, page) in measured)
    {
        block.Append("  - title: ").Append(YamlQuote(item.Title)).Append('\n');
        block.Append("    url: ").Append(WidgetUrl(item.Url!)).Append('\n');
        if (item.Date is { } date)
        {
            block.Append("    date: ").Append(YamlQuote(date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture))).Append('\n');
            block.Append("    sortdate: ").Append(YamlQuote(date.ToString("yyyyMMdd", CultureInfo.InvariantCulture))).Append('\n');
        }
        if (page.Found)
        {
            block.Append("    bytes: ").Append(YamlQuote(page.Bytes.ToString(CultureInfo.InvariantCulture))).Append('\n');
            block.Append("    size: ").Append(YamlQuote(FormatSize(page.Bytes))).Append('\n');
        }
        if (page.Image is { } image)
        {
            block.Append("    image: !url ").Append(YamlQuote(image)).Append('\n');
        }
    }
    block.Append("```").Append('\n');

    return block.ToString().TrimEnd('\n');
}

// A page's "file size" and first image, computed the same way as
// blog-list-generator.cs (each tool is a standalone script, so the logic is
// duplicated -- keep the two in step): the markdown source plus every
// distinct local image it references, and the first such image. A route maps
// to content/<route>.md or content/<route>/index.md.
static (bool Found, long Bytes, string? Image) MeasurePage(string url)
{
    if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
        return (false, 0, null);
    }

    var route = url.Trim('/');
    var file = new[]
    {
        Path.Combine("content", route + ".md"),
        Path.Combine("content", route, "index.md"),
    }.FirstOrDefault(File.Exists);
    if (file is null) return (false, 0, null);

    var raw = File.ReadAllText(file);
    var total = new FileInfo(file).Length;
    string? first = null;
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (Match m in ImageRegex().Matches(raw))
    {
        var reference = m.Groups.Cast<Group>().Skip(1).First(g => g.Success).Value.Trim();
        if (reference.Length == 0 || reference.Contains("://") || reference.StartsWith("data:")) continue;

        var clean = reference.Split('#')[0].Split('?')[0].TrimStart('/');
        var resolved = new[] { clean, Path.Combine(Path.GetDirectoryName(file)!, clean) }.FirstOrDefault(File.Exists);
        if (resolved is null || !seen.Add(resolved)) continue;

        total += new FileInfo(resolved).Length;
        first ??= Path.GetRelativePath(".", resolved).Replace('\\', '/');
    }
    return (true, total, first);
}

// Explorer-style: whole KB rounded up, MB with one decimal past 1 MB.
static string FormatSize(long bytes) => bytes >= 1024 * 1024
    ? $"{bytes / 1024.0 / 1024.0:0.0} MB"
    : $"{Math.Max(1, (bytes + 1023) / 1024):N0} KB";

// Markdown "![alt](path)", an HTML <img src="...">, or a widget's "src:"
// line (slideshows: `- src: !url "content/..."`), in file order.
static Regex ImageRegex() => new(@"!\[[^\]]*\]\(\s*<?([^)\s>]+)[^)]*\)|<img[^>]+src=[""']([^""']+)[""']|^\s*-?\s*src:\s*(?:!url\s*)?[""']?([^""'\s]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

// Site routes go through Canary's !url resolver so they match real page
// paths. Absolute URLs (a GitHub release, say) are left as a plain scalar.
static string WidgetUrl(string url)
{
    if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
        return YamlQuote(url);
    }
    return "!url " + YamlQuote(url.TrimStart('/'));
}

// Same quoting rule as blog-list-generator.cs -- the widget YAML subset
// has no escape sequences, so pick whichever quote character isn't in
// the value.
static string YamlQuote(string value)
{
    var quote = value.Contains('"') && !value.Contains('\'') ? '\'' : '"';
    return $"{quote}{value}{quote}";
}

static Regex WhatsNewTagRegex() => new(@"^\[!WHATS NEW\]$");

static string RenderRss(string baseUrl, string feedPath, List<RssItem> items)
{
    var trimmedBase = baseUrl.TrimEnd('/');
    var feedUrl = $"{trimmedBase}{feedPath}";
    var lastBuild = items.Select(i => i.Date).FirstOrDefault(d => d is not null);

    var xml = new StringBuilder();
    xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
    xml.Append("<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n");
    xml.Append("  <channel>\n");
    xml.Append("    <title>Consoland</title>\n");
    xml.Append("    <link>").Append(EscapeXml($"{trimmedBase}/")).Append("</link>\n");
    xml.Append("    <description>Software releases and development stories from Consoland.</description>\n");
    xml.Append("    <language>en-us</language>\n");
    xml.Append("    <atom:link href=\"").Append(EscapeXml(feedUrl)).Append("\" rel=\"self\" type=\"application/rss+xml\" />\n");
    if (lastBuild is { } buildDate)
    {
        xml.Append("    <lastBuildDate>").Append(Rfc822(buildDate)).Append("</lastBuildDate>\n");
    }

    foreach (var item in items)
    {
        var itemUrl = ResolveUrl(trimmedBase, item.Url!);
        xml.Append("    <item>\n");
        xml.Append("      <title>").Append(EscapeXml(item.Title)).Append("</title>\n");
        xml.Append("      <link>").Append(EscapeXml(itemUrl)).Append("</link>\n");
        xml.Append("      <guid isPermaLink=\"true\">").Append(EscapeXml(itemUrl)).Append("</guid>\n");
        if (item.Date is { } pubDate)
        {
            xml.Append("      <pubDate>").Append(Rfc822(pubDate)).Append("</pubDate>\n");
        }
        if (item.Author is not null)
        {
            xml.Append("      <dc:creator>").Append(EscapeXml(item.Author)).Append("</dc:creator>\n");
        }
        if (item.Summary is not null)
        {
            xml.Append("      <description>").Append(EscapeXml(item.Summary)).Append("</description>\n");
        }
        xml.Append("    </item>\n");
    }

    xml.Append("  </channel>\n");
    xml.Append("</rss>\n");
    return xml.ToString();
}

// Site routes get the same trailing-slash URLs as sitemap.xml. Anything
// already absolute (a GitHub release, say) is left alone.
static string ResolveUrl(string trimmedBase, string url)
{
    if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
        return url;
    }
    return $"{trimmedBase}/{url.TrimStart('/')}/";
}

static void WriteFeedIfChanged(string path, string contents)
{
    var fullPath = Path.GetFullPath(path);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    if (File.Exists(fullPath) && File.ReadAllText(fullPath) == contents) return;
    File.WriteAllText(fullPath, contents, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static string Rfc822(DateTime date) => date.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture);

static string EscapeXml(string value) =>
    value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");

readonly record struct RssItem(string Title, string? Summary, string? Author, DateTime? Date, string? Url);
