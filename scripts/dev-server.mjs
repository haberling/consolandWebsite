// Zero-dependency static file server for local development. Serves src/ as
// the site root and content/ under /content/, mirroring the relative layout
// the C# build tool will eventually assemble into docs/.

import http from "node:http";
import { readFile, stat } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, "..");
const srcRoot = path.join(repoRoot, "src");
const contentRoot = path.join(repoRoot, "content");
const port = process.env.PORT ? Number(process.env.PORT) : 8080;

const mimeTypes = {
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".json": "application/json; charset=utf-8",
  ".md": "text/markdown; charset=utf-8",
  ".png": "image/png",
  ".jpg": "image/jpeg",
  ".jpeg": "image/jpeg",
  ".svg": "image/svg+xml",
  ".ico": "image/x-icon",
};

function contentTypeFor(filePath) {
  return mimeTypes[path.extname(filePath).toLowerCase()] ?? "application/octet-stream";
}

async function resolveFile(urlPath) {
  const isContent = urlPath.startsWith("/content/");
  const root = isContent ? contentRoot : srcRoot;
  const relative = isContent ? urlPath.slice("/content".length) : urlPath;
  const filePath = path.join(root, decodeURIComponent(relative));

  if (!filePath.startsWith(root)) return null; // guard against path escape

  try {
    const info = await stat(filePath);
    const finalPath = info.isDirectory() ? path.join(filePath, "index.html") : filePath;
    await stat(finalPath);
    return finalPath;
  } catch {
    return null;
  }
}

const server = http.createServer(async (req, res) => {
  const urlPath = decodeURI(new URL(req.url ?? "/", "http://localhost").pathname);
  const filePath = await resolveFile(urlPath === "/" ? "/index.html" : urlPath);

  if (!filePath) {
    res.writeHead(404, { "Content-Type": "text/plain" });
    res.end("Not found: " + urlPath);
    return;
  }

  try {
    const body = await readFile(filePath);
    res.writeHead(200, { "Content-Type": contentTypeFor(filePath) });
    res.end(body);
  } catch {
    res.writeHead(500, { "Content-Type": "text/plain" });
    res.end("Server error");
  }
});

server.listen(port, () => {
  console.log(`Dev server running at http://localhost:${port}/`);
  console.log(`  src/     -> /`);
  console.log(`  content/ -> /content/`);
});
