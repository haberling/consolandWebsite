// Minimal hash router. Content-fetching/markdown-rendering plugs in later
// via registerRouteHandler; for now main.js supplies a placeholder handler.

const Router = (() => {
  let handler = null;

  function parseHash() {
    const hash = window.location.hash.replace(/^#/, "");
    const path = hash === "" ? "/" : hash;
    const segments = path.split("/").filter(Boolean);
    return { path, segments };
  }

  function registerRouteHandler(fn) {
    handler = fn;
  }

  function handleRouteChange() {
    if (!handler) return;
    handler(parseHash());
  }

  function start() {
    window.addEventListener("hashchange", handleRouteChange);
    handleRouteChange();
  }

  return { registerRouteHandler, start };
})();
