// Widget for a fenced ```downloads block in markdown. Block syntax (one
// link per line):
//
//   ```downloads:Optional Title
//   Label | https://example.com/file.msi
//   ```
//
// See markdown.ts's fence handling for how widget modules are dispatched.

interface DownloadLink {
  label: string;
  url: string;
}

function escapeHtml(s: string): string {
  return s
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}

function parseLinks(body: string): DownloadLink[] {
  return body
    .split("\n")
    .map((line) => line.trim())
    .filter(Boolean)
    .map((line) => {
      const [label, url] = line.split("|").map((part) => part.trim());
      return { label: label ?? "", url: url ?? "" };
    })
    .filter((link) => link.label && link.url);
}

export function render(title: string, body: string): string {
  const links = parseLinks(body);
  const items = links
    .map(
      (link) =>
        `<li><a class="download-button" href="${escapeHtml(link.url)}">${escapeHtml(link.label)}</a></li>`
    )
    .join("");
  return `<div class="downloads-widget"><h3>${escapeHtml(title || "Downloads")}</h3><ul>${items}</ul></div>`;
}
