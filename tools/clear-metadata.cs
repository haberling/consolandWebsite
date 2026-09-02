// Toolchain tool -- strips a page's leading frontmatter block (bare
// "key: value" lines, optionally wrapped in a pair of lone "---" lines) so
// it never reaches the renderer. Canary itself doesn't parse frontmatter
// at all right now (see blog-list-generator.cs's own note on this) -- it
// just renders as a literal paragraph at the top of the page, which is
// the bug this fixes. Registered as "clear-metadata" in canary.jsonc's
// "tools" registry; applied per-post via that post's own .toolchain.json
// (no cascading -- every post folder that has frontmatter needs it added
// there individually).
//
// Contract (see docsite/content/guide/toolchain.md): read raw markdown on
// stdin, write the transformed markdown on stdout.
//
// Detection mirrors blog-list-generator.cs's AuthorDateFromMarkdown: an
// optional opening "---" line, then bare "key: value" lines, closed by
// another "---" line. If a heading shows up before a closing "---" is
// found (or EOF is reached first), there's no frontmatter block -- the
// file passes through completely unchanged rather than being partially
// mangled.

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

// Canary writes/reads this tool's stdio as UTF-8 (see ToolchainRunner) --
// match that here, since Console.In/Out otherwise default to the OS
// console codepage (commonly not UTF-8 on Windows), which corrupts any
// non-ASCII character (math symbols, smart quotes, accents) passing through.
Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);

var input = Console.In.ReadToEnd();
var lines = input.Replace("\r\n", "\n").Split('\n');

var bodyStart = FindFrontmatterEnd(lines);
if (bodyStart == -1)
{
    Console.Out.Write(input);
    return;
}

// Drop any blank lines right after the closing "---" too, so the body
// starts cleanly at its first real content line instead of leaving a gap.
while (bodyStart < lines.Length && lines[bodyStart].Trim().Length == 0)
{
    bodyStart++;
}

Console.Out.Write(string.Join('\n', lines.Skip(bodyStart)));
return;

// Index of the first body line (just past the closing "---"), or -1 if no
// frontmatter block is present.
static int FindFrontmatterEnd(string[] lines)
{
    var start = lines.Length > 0 && lines[0].Trim() == "---" ? 1 : 0;

    for (var i = start; i < lines.Length; i++)
    {
        var trimmed = lines[i].Trim();
        if (trimmed == "---") return i + 1;
        if (HeadingRegex().IsMatch(trimmed)) return -1;
    }
    return -1;
}

static Regex HeadingRegex() => new(@"^(#{1,6})\s+");
