// Toolchain tool -- three independent insertions into a blog post:
//
//   1. A "< Return to Blog" / "< Return to Archive" link directly above the
//      post's title, linking back to that blog's own index page.
//   2. The post's title line and, if present, the italic tagline (a lone
//      "*...*" line) directly after it, replaced with a "blogheader"
//      widget block -- title, tagline, and (when the post's frontmatter has
//      both "author" and "authorDate" populated) a "Written by X on Y"
//      byline, all packed tight. See widgets/blogheader.html/.css. Anything
//      after the title/tagline (a header image, the rest of the post) is
//      left completely untouched.
//   3. The same return link again, appended at the very end of the post --
//      so a reader who scrolls all the way through doesn't have to scroll
//      back up to leave.
//
// Does nothing to any of these when run against the blog's own index page.
//
// Registered twice in canary.json under different names, each baking in a
// different CLI argument -- .toolchain.json can only reference a tool by
// name, it has no way to pass a per-invocation argument, so the argument
// has to live in the registered command string instead:
//
//   "format-blogpost-new": "dotnet run tools/format-blogpost.cs -- new",
//   "format-blogpost-archive": "dotnet run tools/format-blogpost.cs -- archive"
//
// Must run BEFORE clear-metadata in a post's .toolchain.json "tools" array
// -- for two compounding reasons now:
//   - The byline needs "author"/"authorDate" to still be there to read.
//   - This tool inserts its return-link directly above the first heading it
//     finds, assuming that heading is the post's real title. If frontmatter
//     still precedes it (clear-metadata hasn't run yet), the link lands
//     between the frontmatter and the title instead of above everything,
//     and a *later* clear-metadata pass over that already-modified markdown
//     no longer finds the frontmatter block at line 0 where its own
//     detection expects it.
// Tools run in declared array order, chained (see docsite/content/guide/
// toolchain.md) -- order in the array is what fixes both of these, not
// anything in either tool itself.
//
// Note: the page's own <title>/pageTitle is read from the pristine,
// pre-toolchain source file (Canary.Core.Build.PageBuilder.TitleFromMarkdown
// runs against the original "source", not the toolchain-transformed
// markdown this tool hands back) -- so replacing the literal "# Title" line
// in the rendered body with a widget here doesn't break that.
//
// Contract: read raw markdown on stdin, write the transformed markdown on
// stdout. Gets CANARY_ROUTE_PATH, compared directly against the target
// blog's own index route to detect "this invocation IS the index page" --
// no need to inspect the filesystem or manifest.json for that.

using System.Globalization;
using System.Text.RegularExpressions;

var blogKind = args.Length > 0 ? args[0] : null;
var (indexRoute, label) = blogKind switch
{
    "new" => ("blog", "Blog"),
    "archive" => ("blog/blog-archive", "Archive"),
    _ => throw new InvalidOperationException(
        $"[format-blogpost] expected a \"new\" or \"archive\" argument, got: {blogKind ?? "(none)"}"),
};

var routePath = Environment.GetEnvironmentVariable("CANARY_ROUTE_PATH") ?? "";
var input = Console.In.ReadToEnd();

if (routePath == indexRoute)
{
    Console.Out.Write(input);
    return;
}

var lines = input.Replace("\r\n", "\n").Split('\n');
var headingIndex = Array.FindIndex(lines, l => HeadingRegex().IsMatch(l.Trim()));
var insertReturnAt = headingIndex == -1 ? 0 : headingIndex;

var result = new List<string>(lines.Length + 6);
result.AddRange(lines[..insertReturnAt]);
result.Add($"[< Return to {label}](/{indexRoute})");
result.Add("");

if (headingIndex == -1)
{
    result.AddRange(lines[insertReturnAt..]);
}
else
{
    var title = HeadingRegex().Replace(lines[headingIndex].Trim(), "").Trim();
    var (tagline, tailStart) = TitleTaglineEnd(lines, headingIndex);
    var (author, date) = AuthorByline(input);

    result.Add("```blogheader");
    result.Add($"title: {YamlQuote(title)}");
    if (tagline is not null) result.Add($"tagline: {YamlQuote(tagline)}");
    if (author is not null) result.Add($"author: {YamlQuote(author)}");
    if (date is not null) result.Add($"date: {YamlQuote(date)}");
    result.Add("```");
    result.Add("");
    result.AddRange(lines[tailStart..]);
}

while (result.Count > 0 && result[^1].Trim().Length == 0) result.RemoveAt(result.Count - 1);
result.Add("");
result.Add($"[< Return to {label}](/{indexRoute})");

Console.Out.Write(string.Join('\n', result));
return;

// The italic tagline (a lone "*...*" line) after the title, if there is
// one -- and the index of the document's own next real (non-blank) line,
// where the widget's own blank-line spacing takes over from whatever blank
// line(s) originally sat between the title/tagline and whatever follows
// (an image, a paragraph). Without skipping those, the widget's own
// trailing blank stacks on top of the original one instead of replacing
// it -- same double-blank-line bug the byline blockquote version of this
// tool had before it was tracked separately. Deliberately does NOT also
// swallow a header image the way an earlier version of this tool did: the
// widget only owns title+tagline+byline, nothing else.
//
// A blank line is allowed to sit between the title and the tagline (the
// archive posts are written that way; the-canary-logo-pipeline.md isn't --
// both need to work). TaglineRegex itself rejects a line containing "["
// -- a tagline with a markdown link in it (legacy-code.md's does) can't be
// pulled into the widget's plain-text field without silently turning that
// link into dead text, since Mustache doesn't render markdown. Left alone,
// that line just falls through as an ordinary paragraph after the widget,
// still a real link.
static (string? Tagline, int TailStart) TitleTaglineEnd(string[] lines, int headingIndex)
{
    var i = headingIndex + 1;
    while (i < lines.Length && lines[i].Trim().Length == 0) i++;

    string? tagline = null;
    if (i < lines.Length)
    {
        var taglineMatch = TaglineRegex().Match(lines[i].Trim());
        if (taglineMatch.Success)
        {
            tagline = taglineMatch.Groups[1].Value;
            i++;
            while (i < lines.Length && lines[i].Trim().Length == 0) i++;
        }
    }
    return (tagline, i);
}

// (author, formatted date) read from frontmatter, or (null, null) unless
// BOTH fields are genuinely populated -- the widget's byline line only
// shows up when there's a full "Written by X on Y" to say, never a
// half-filled one.
static (string? Author, string? Date) AuthorByline(string source)
{
    var lines = source.Split('\n');
    var start = lines.Length > 0 && lines[0].Trim() == "---" ? 1 : 0;

    string? author = null;
    DateTime? authorDate = null;

    for (var i = start; i < lines.Length; i++)
    {
        var line = lines[i].TrimEnd('\r');
        var trimmed = line.Trim();
        if (trimmed == "---") break;
        if (HeadingRegex().IsMatch(trimmed)) break;

        var colon = line.IndexOf(':');
        if (colon <= 0) continue;

        var key = line[..colon].Trim();
        var value = line[(colon + 1)..].Trim().Trim('"', '\'');

        if (key.Equals("author", StringComparison.OrdinalIgnoreCase))
        {
            author = value.Length > 0 ? value : null;
        }
        else if (key.Equals("authorDate", StringComparison.OrdinalIgnoreCase))
        {
            authorDate = DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : null;
        }
    }

    return author is null || authorDate is null
        ? (null, null)
        : (author, authorDate.Value.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture));
}

// The widget YAML subset has no escape sequences (Canary.Core.Templating.
// YamlParser.ParseScalarText just strips the surrounding quote chars
// as-is) -- so instead of escaping, pick whichever quote character isn't
// present in the value. Falls back to double quotes if both appear, which
// is a known, accepted limitation for that rare case. Same approach as
// blog-list-generator.cs's own YamlQuote.
static string YamlQuote(string value)
{
    var quote = value.Contains('"') && !value.Contains('\'') ? '\'' : '"';
    return $"{quote}{value}{quote}";
}

static Regex HeadingRegex() => new(@"^(#{1,6})\s+");
static Regex TaglineRegex() => new(@"^\*([^*\[\]]+)\*$");
