// Shared behavior for the underconstruction widget (see
// underconstruction.html for the template/data contract) -- referenced
// once per page via <script defer>, not duplicated per widget instance.
// Event delegation on `document` means a widget instance added later
// (hybrid mode's fragment-fetch swap) is handled automatically, no
// re-init call needed anywhere in the router code.
//
// The title-bar minimize/maximize control and OK collapse the window to its title
// bar; clicking the title bar while collapsed expands it again. The
// button shows a minimize glyph when open and a maximize glyph when minimized.
//
// IIFE-wrapped because every widget script on a page shares one classic
// global scope (Canary's widget contract requires plain `<script defer>`,
// never `type="module"`).
(() => {
  // Win98 title-bar glyphs: minimize (low bar) when open, maximize (box with
  // thick top edge) when collapsed.
  const MINIMIZE_ICON = '<svg viewBox="0 0 10 10" width="10" height="10" fill="currentColor" shape-rendering="crispEdges" aria-hidden="true"><rect x="1" y="7" width="6" height="2"/></svg>';
  const MAXIMIZE_ICON = '<svg viewBox="0 0 10 10" width="10" height="10" fill="none" stroke="currentColor" shape-rendering="crispEdges" aria-hidden="true"><rect x="0.5" y="0.5" width="9" height="9"/><path d="M0 1.5h10" stroke-width="2"/></svg>';

  function rootFrom(el) {
    return el && el.closest ? el.closest('[data-widget="underconstruction"]') : null;
  }

  function setCollapsed(root, collapsed) {
    root.classList.toggle("is-collapsed", collapsed);
    const toggle = root.querySelector(".uc-btn-toggle");
    if (!toggle) return;
    toggle.setAttribute("aria-expanded", collapsed ? "false" : "true");
    toggle.setAttribute(
      "aria-label",
      collapsed
        ? "Maximize under construction notice"
        : "Minimize under construction notice"
    );
    toggle.innerHTML = collapsed ? MAXIMIZE_ICON : MINIMIZE_ICON;
  }

  document.addEventListener("click", (e) => {
    const toggle = e.target.closest("[data-uc-toggle]");
    if (toggle) {
      const root = rootFrom(toggle);
      if (!root) return;
      setCollapsed(root, !root.classList.contains("is-collapsed"));
      return;
    }

    const bar = e.target.closest("[data-uc-titlebar]");
    if (!bar) return;
    const root = rootFrom(bar);
    if (root && root.classList.contains("is-collapsed")) {
      setCollapsed(root, false);
    }
  });
})();
