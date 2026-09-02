---
author: Marcus Haberling
authorDate: 2026-09-01
---

# Canary Development: Toolchain
*Keeping the source markdown clean*


## Origin
So when I started Canary, I had a few goals in mind. One of those was that the site should be able to use modern site features. See the slideshows and download widgets as [examples](https://canary.consoland.net/guide/widgets). But the site's source files are markdown documents, and I wanted to keep those mostly clean. Like, you could print out the source and it would be a basically readable document, not polluted with all sorts of strange formatting syntax.

Obviously adding features and keeping clean markdown fight against each other. There has to be 'something' on the page to define behavior. Toolchain allows those elements to be coded in, or expanded at the last possible moment. This way the original source stays clean.

## How Toolchain Works
Toolchain is a dead simple system. The tools themselves are command line calls, registered by name in the project's canary.jsonc file. Each subdirectory in the website content folder (usually called content) has a .toolchain.json that lists out which of those names should be run on the files in that directory. When canary build is going to push a file through the HTML renderer, it first runs that file through each of the listed commands, in order. Each command reads markdown on stdin and writes markdown on stdout — the next tool gets whatever the last one wrote, until the last tool's output is passed to the renderer. One may say we are chaining a list of tools together, get it? Bueller?

## Example (This Post)
OOOH META! That's right, since this page is run through a toolchain, we can use it as an example of how it can clean things up. Blog posts on Consoland use a couple of tools to prep them for rendering. In the source .md for this page, there are a few things missing that show up on the site.
- Blogheader: a widget that makes a title block with title, tagline, author, and date
- Chirp: a comments section widget
- "Return to Blog" links at the top and bottom of the page
The tool **format-blogpost-new** allows these elements to be added before rendering. Then, **clear-metadata** takes the metadata block off the top of the file before the renderer can see it.

### Blogheader
If I was going to inline a Canary widget, the "Title Block" of the page would have to look like this:
```code
lang: yaml
lines:
  - text: "```blogheader"
  - text: 'title: "Canary Development: Toolchain"'
  - text: 'tagline: "Keeping the source markdown clean"'
  - text: 'author: "Marcus Haberling"'
  - text: 'date: "September 1, 2026"'
  - text: "```"
```
The format-blogpost-new tool collects the author and date from metadata, then finds the title and tagline and replaces both of them with the fenced YAML shown above.

### Chirp and Return Links
Same deal for the comments widget and the pair of "back to the blog" links: none of it is in the source. If it were, the end of this page would look like this:
```code
lang: markdown
lines:
  - text: "```chirp"
  - text: 'page: "blog|canary-development-toolchain"'
  - text: "```"
  - text: ""
  - text: '[< Return to Blog](/blog)'
```
**format-blogpost-new** appends the Chirp block and writes that return link above the title and again at the very end. The Chirp "page" field is "blog|" + this file's name, so the comments stay tied to the post instead of its URL.

## Toolchain's Weakness
One issue with this (very slick much cool) Toolchain system is that it slows down build times in two ways.
First, to maintain flexibility in tools, Canary has to build every page on every build. Some tools may look at *other* pages in source to determine how to edit the page that it's on, so just because a page hasn't changed doesn't mean that rebuilding isn't necessary. I have *partially* addressed this in Canary v0.2.0. After the first pass, **serve** only rebuilds what changed, which can be wrong, and that's acceptable while you are editing.

Second, the startup times for a tool may end up compounding into a decent chunk of time as tools are started over and over again on different pages. In my case, the tools were originally written in C# and invoked with *dotnet run*. This page is the example: **format-blogpost-new** and **clear-metadata** each start once per post. The load time for a *dotnet run* call is trivial in most cases but does stack up when call after call occurs. I have addressed this by adding an AOT compilation feature for C# files to v0.2.0 of Canary. That way the tools are compiled and open much quicker. One, of course, could do this manually for any other language where this crops up.

## Conclusion
What I think (hope? pray?) this post does is show how a software goal can transition into a feature and then how the nature of that feature causes problems that require further evolution of the code base. Clean markdown (goal) -> Toolchain (feature) -> slow builds (problem) -> serve and AOT compilation (evolution).

I plan to explore other parts of Canary and how they were developed in future blog posts. Thanks for reading, and feel free to leave a comment/question below.