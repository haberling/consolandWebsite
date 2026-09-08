# llote

A Windows command-line note-taker. Write a line, get a timestamp. Ask later by meaning — `ask` only ever returns a line you already wrote, verbatim. Nothing leaves your machine.

`v1.0.0` out for windows.

```slideshow
title: Screenshots
slides:
  - src: !url "content/utilities/images/llote-splash.png"
    caption: "Startup splash"
  - src: !url "content/utilities/images/llote-cli.png"
    caption: "Ask by meaning, then keep writing"
  - src: !url "content/utilities/images/llote-browse.png"
    caption: "browse TUI"
  - src: !url "content/utilities/images/llote-docs.png"
    caption: "User's guide"
```

## Features

1. Single-line notes, timestamped the moment you write them
2. `llote ask` searches by meaning, not exact wording — and never generates text
3. `llote browse` is a pageable TUI for adding, searching, and reading the file
4. Notes stay on disk as plain text; the embedding model downloads once (~100MB) and then works offline
5. A local `log.llote` in the current folder, or a global notes file in app data
6. Append-only `.llote` files that play nicely with Git and AI assistants

## Quick start

```code
lang: text
lines:
  - text: 'llote "bought milk"'
  - text: "llote"
  - text: 'llote ask "what did I write about the dentist?"'
  - text: "llote browse"
```

The full user's guide is at [llote.consoland.net](https://llote.consoland.net/).

```downloads
title: Get llote
items:
  - label: "Microsoft Store"
    url: "https://apps.microsoft.com/detail/9N8BHTQZR4L4"
  - label: "Windows Installer"
    url: "https://github.com/haberling/consolandWebsite/releases/download/llote-v1.0.0/llote-setup.exe"
  - label: "User's Guide"
    url: "https://llote.consoland.net/"
```

The Store build gives you less fine-grained control over uninstall. The installer here does.

llote is free to use. Copyright 2026 Habersoft.

## Comments

```chirp
page: "utilities|llote"
```
