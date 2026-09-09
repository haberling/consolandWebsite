# llote

A terminal-native note-taking utility with semantic search.

`v1.0.0` out for Windows.

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

1. Single-line notes can be added directly from the command line with `llote "note contents"`. You can also specify a file.
2. `llote ask` searches by meaning, not exact wording, using an embedding model. It returns your note verbatim. AI is used only for search.
3. `llote browse` is a pageable TUI for adding, searching, and reading the file.
4. Notes stay on disk as plain text; the embedding model downloads once (~100MB) and then works offline.
5. By default, notes go to a local `log.llote`, or to a global default if a local file doesn't exist.
6. As plain text, `.llote` files that play nicely with Git and AI assistants.

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
    url: "https://github.com/haberling/llote-docs/releases/download/llote-v1.0.0/llote-setup.exe"
  - label: "User's Guide"
    url: "https://llote.consoland.net/"
```

The Store build gives you less fine-grained control over uninstall. The installer here does.

llote is free to use. Copyright 2026 Habersoft.

## Comments

```chirp
page: "utilities|llote"
```
