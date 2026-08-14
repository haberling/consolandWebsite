# Consoland Website — Markdown Wiki + C# Build/Deploy Tool

## Context

The user wants a personal site to publish CLI utilities/games (mostly served as downloadable MSI installers) as a lightweight, hand-rolled wiki: drop a markdown file in a folder, it becomes a page. Key decisions already locked in:

- **Browser-side runtime**: hand-written TypeScript (compiled to JS) doing routing + markdown→HTML conversion — no framework, no markdown library.
- **Tooling**: a C# console app does everything else — scanning content, compiling the TS, assembling the output folder, and running `git add/commit/push` itself.
- **Hosting**: GitHub Pages, serving a `docs/` folder on `main`. The GitHub repo doesn't exist yet.
- Directory is currently empty — from-scratch scaffold.

Plenty of specifics (exact markdown feature set, the download-widget's exact syntax, manifest.json shape, styling) are deliberately left open — those get decided while building each piece, not locked in up front.

## Broad-stroke steps

1. **GitHub repo setup** — `git init`, create the GitHub repo via `gh` CLI (public, needed for free Pages hosting), initial commit, push, and configure Pages to serve `main:/docs`.

2. **Scaffold content** — create `content/` with `home.md` plus one example page each under a couple of sections (e.g. `games/`, `utilities/`), so there's something real to render while building the runtime.

3. **Build the TS runtime** — a hand-written markdown→HTML parser, a client-side router (page load → fetch the right `.md` → render into the page), and the installer-downloads widget. Lives in `src/`, compiles to `docs/js`.

4. **Build the C# tool** — a console app with a `build` command (generate nav data from `content/`, compile the TS, copy content/images/installers into `docs/`) and a `deploy` command (`build`, then commit + push).

5. **Wire it together and verify locally** — run the tool's `build`, serve `docs/` locally, click through home → section → page, confirm images and the download widget work.

6. **First deploy** — run `deploy`, confirm the live GitHub Pages URL matches local.

## Verification
- Local: serve `docs/` with any static file server and click through nav → page → image → download widget.
- Remote: after deploy, repeat the same click-through against the live GitHub Pages URL.
