// Entry point for renderMode "hybrid" (see PLAN.md's Render modes section).
// Every route already has a prerendered file: cold loads get it straight
// from the server, no JS required. Once this script has loaded, in-app
// navigation (via router.ts's pushState-based click interception) fetches
// the TARGET route's own prerendered HTML file, extracting just the #app
// fragment and splicing it in -- no re-render, no raw markdown ever reaches
// the client, and cold vs. warm navigation show byte-identical content
// because it's the same file either way.
import { registerRouteHandler, start } from "./router.js";
import { loadNav, updateActiveNav } from "./nav.js";
function prerenderedUrlFor(routePath) {
    return routePath === "" ? "/" : `/${routePath}/`;
}
// Widget <script> tags (chirp.js, downloads.js, slideshow.js, ...) are
// emitted outside #app -- see SiteBuilder.BuildWidgetScriptsHtml and
// shell.html's {{widgetScripts}} placeholder, which sits after </main> --
// so the #app-only innerHTML splice below never carries them over. Without
// this, navigating in-app from a page that doesn't use a given widget to
// one that does silently strands that widget: its markup lands in the DOM
// but the script that would enhance it never loads, every single time (a
// full reload works because that's a genuine cold load of the complete
// document, trailing scripts included). Reconciles by resolved `src` --
// already-loaded scripts (from the current page, or appended by an earlier
// navigation) are left alone -- and appends real elements for the rest,
// since scripts inside an innerHTML-assigned string never execute.
function loadMissingWidgetScripts(fetchedDoc) {
    const alreadyLoaded = new Set(Array.from(document.scripts, (s) => s.src).filter(Boolean));
    for (const script of Array.from(fetchedDoc.scripts)) {
        if (!script.src || alreadyLoaded.has(script.src))
            continue;
        const clone = document.createElement("script");
        for (const attr of Array.from(script.attributes)) {
            clone.setAttribute(attr.name, attr.value);
        }
        document.body.appendChild(clone);
    }
}
let initialLoad = true;
registerRouteHandler(async (route) => {
    const app = document.getElementById("app");
    if (!app)
        return;
    // The very first route dispatch is always the cold-load case (start()
    // calls the handler synchronously, before any click can happen) -- #app
    // already contains this exact page's prerendered content, so skip the
    // redundant fetch-and-replace.
    if (initialLoad) {
        initialLoad = false;
        return;
    }
    updateActiveNav();
    app.innerHTML = "<p>Loading&hellip;</p>";
    const url = prerenderedUrlFor(route.path);
    try {
        const res = await fetch(url);
        if (!res.ok) {
            app.innerHTML = `
        <h1>Not found</h1>
        <p>No page at <code>${url}</code>.</p>
        <p><a href="/">&larr; back home</a></p>
      `;
            return;
        }
        const html = await res.text();
        const fetchedDoc = new DOMParser().parseFromString(html, "text/html");
        const fragment = fetchedDoc.getElementById("app");
        app.innerHTML = fragment ? fragment.innerHTML : "<h1>Error</h1><p>Could not parse this page.</p>";
        if (fragment)
            loadMissingWidgetScripts(fetchedDoc);
    }
    catch {
        app.innerHTML = "<h1>Error</h1><p>Could not load this page.</p>";
    }
});
loadNav();
start();
