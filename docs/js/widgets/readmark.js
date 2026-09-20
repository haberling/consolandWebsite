// Behavior for the readmark widget (see readmark.html). Event delegation
// on document, so instances spliced in by the router work without any init
// call. Shares bloglist's localStorage key ("bloglist-read", a JSON array
// of post paths like "/blog/blog-new/my-post/my-post"), so the two widgets
// always agree. Every storage access is guarded (blocked storage just means
// the mark doesn't stick).
//
// IIFE-wrapped because every widget script on a page shares one classic
// global scope (Canary's widget contract requires plain `<script defer>`).
(() => {
  const READ_KEY = "bloglist-read";

  function loadRead() {
    try {
      const parsed = JSON.parse(localStorage.getItem(READ_KEY) || "[]");
      return new Set(Array.isArray(parsed) ? parsed : []);
    } catch (e) {
      return new Set();
    }
  }

  function saveRead(set) {
    try { localStorage.setItem(READ_KEY, JSON.stringify(Array.from(set))); } catch (e) { /* storage blocked */ }
  }

  // The router uses pushState, so the live pathname is always the current
  // post. Normalized to bloglist's form: leading slash, no trailing slash.
  function currentUrl() {
    return "/" + window.location.pathname.replace(/^\/+|\/+$/g, "");
  }

  function refresh(btn) {
    const isRead = loadRead().has(currentUrl());
    btn.setAttribute("aria-pressed", String(isRead));
    btn.querySelector(".readmark-label").textContent = isRead ? "Read" : "Mark as read";
  }

  function refreshAll() {
    document.querySelectorAll("[data-readmark]").forEach(refresh);
  }

  document.addEventListener("click", (e) => {
    const btn = e.target.closest("[data-readmark]");
    if (!btn) return;
    const set = loadRead();
    const url = currentUrl();
    if (set.has(url)) set.delete(url);
    else set.add(url);
    saveRead(set);
    refreshAll();
  });

  window.addEventListener("storage", (e) => {
    if (e.key === READ_KEY || e.key === null) refreshAll();
  });

  // Instances arrive after load (hybrid mode's fragment swap); initialize
  // any that haven't been yet.
  new MutationObserver(() => {
    document.querySelectorAll("[data-readmark]:not([data-rm-ready])").forEach((btn) => {
      btn.dataset.rmReady = "1";
      refresh(btn);
    });
  }).observe(document.documentElement, { childList: true, subtree: true });

  const start = () => document.querySelectorAll("[data-readmark]").forEach((btn) => {
    btn.dataset.rmReady = "1";
    refresh(btn);
  });
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", start);
  else start();
})();
