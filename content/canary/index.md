# Canary

*A web framework for those with enough web development skills to want to avoid it.*
![canary logo](content/canary/images/canary-logo.svg)
Canary is a framework for making websites out of a directory of markdown documents. It consists of a typescript client router, paired with a C# build and deploy tool for rendering your pages in HTML at build time. This makes a site with smooth SPA like page navigation while still playing nice with web crawlers, direct links, and preview scrapers.

## Why it exists

Canary comes out of Consoland development. There was always a plan of at least publishing a collection of tools. When it became clear that SEO wasn't going to be possible with the # navigation of the original site, I decided to start that process earlier than originally planned.

## What's under the hood

- Markdown -> HTML renderer.
- Widgets -> made using JavaScript and templated HTML (mustache) Downloads and Slideshow are examples on this site.
- Toolchain -> optionally transform your markdown files before render to keep the source markdown documents cleaner.
- Client Router -> Rebuilds using history API to give you a live editing experience.
- Command Line Interface 
```code 
lang: CLI
lines:
 - text: canary build | canary serve | canary publish | [and more]
```


## Status

Alpha release is out and functional (if this site is working at least). Source is on [GitHub](https://github.com/haberling/canary) and open under the MIT License. Would be happy to receive any suggestions or contributions there.

## Downloads
Try Canary out for yourself!

```downloads
title: Release 0.1.0
items:
  - label: "Windows Installer"
    url: "https://github.com/haberling/canary/releases/download/v0.1.0/CanaryInstaller.msi"
  - label: "Source Code (zip)"
    url: "https://github.com/haberling/canary/archive/refs/tags/v0.1.0.zip"
  - label: "Source Code (tar.gz)"
    url: "https://github.com/haberling/canary/archive/refs/tags/v0.1.0.tar.gz"

```
