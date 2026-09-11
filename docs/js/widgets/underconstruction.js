// Shared behavior for the underconstruction widget (see
// underconstruction.html for the template/data contract) -- referenced
// once per page via <script defer>, not duplicated per widget instance.
// Event delegation on `document` means a widget instance added later
// (hybrid mode's fragment-fetch swap) is handled automatically, no
// re-init call needed anywhere in the router code.
//
// The title-bar -/+ control and OK collapse the window to its title
// bar; clicking the title bar while collapsed expands it again. The
// button shows "-" when open and "+" when minimized.
//
// IIFE-wrapped because every widget script on a page shares one classic
// global scope (Canary's widget contract requires plain `<script defer>`,
// never `type="module"`).
(() => {
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
    toggle.textContent = collapsed ? "+" : "-";
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
