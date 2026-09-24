# llote

A terminal-native note-taking utility with semantic search.

`v1.1.0` out for Windows on the Microsoft Store.

```slideshow
title: Screenshots
slides:
  - src: !url "content/utilities/images/llote-browse.jpg"
    caption: "Browse mode: interactive terminal interface"
  - src: !url "content/utilities/images/llote-splash.png"
    caption: "Splash screen"
  - src: !url "content/utilities/images/llote-cli.png"
    caption: "Add notes, search, and open browse mode from the command line"
  - src: !url "content/utilities/images/llote-docs-cover.jpg"
    caption: "First-class documentation"
  - src: !url "content/utilities/images/llote-docs-commands.jpg"
    caption: "Become an expert llote user in ~10 minutes"
  - src: !url "content/utilities/images/llote-start-menu.png"
    caption: "Lives in Windows like a normal app"
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

## Why Microsoft Store only?

My original plan was to release llote both here and on the Microsoft Store. What changed is that, like a lot of software projects, once the first version was out I came up with all sorts of improvements and refinements I wanted to add. llote is a side project, and I decided to spend the time I have for it on new features instead of testing and maintaining installers. The Microsoft Store won out because, hopefully, more people will find llote there than in this small corner of the internet.

```downloads
title: Get llote
items:
  - label: "Microsoft Store"
    url: "https://apps.microsoft.com/detail/9N8BHTQZR4L4"
  - label: "User's Guide"
    url: "https://llote.consoland.net/"
```

llote is free to use. Copyright 2026 Habersoft.

## Comments

```chirp
page: "utilities|llote"
```
