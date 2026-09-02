// Shared behavior for the wasmterm widget (see wasmterm.html for the
// markup/data contract, ../INTEROP.md for what a .NET console-wasm app
// needs to expose to be embeddable here). One <script> per page, not one
// per widget instance -- Canary's widget-script contract -- so this can't
// run its boot sequence at parse time the way a page-owned main.js could:
// it has to enhance each [data-widget="wasmterm"] node the moment that
// node appears, via the same enhanceAll()/MutationObserver pattern
// slideshow.js uses, since hybrid-mode routing can splice an instance into
// the page later without a full reload. Not written as a module (no
// static `import`) for the same reason -- Canary links widget scripts as
// plain <script defer>, so the dotnet.js loader is pulled in with a
// dynamic import() inside enhance() instead.

// Resolves a dotted JSExport path ("Tesselate.InputBridge.OnKeyEvent")
// against the assembly's exports object -- lets keyEventExport/
// audioFillExport be plain config data instead of each app needing its
// own bespoke bridging code.
function resolveExport(exports, dottedPath) {
  return dottedPath.split(".").reduce((obj, key) => obj?.[key], exports);
}

// Both app-level config files (wasmterm.manifest, config.keys) share this
// comment convention: "//" runs to end of line, full-line or trailing.
function stripComment(line) {
  const i = line.indexOf("//");
  return i === -1 ? line : line.slice(0, i);
}

// Missing file is a normal, silent case (an app with no keyboard/audio
// capability just won't have one) -- callers distinguish "absent" from
// "present but empty" themselves where that distinction matters.
async function fetchText(url) {
  try {
    const res = await fetch(url);
    return res.ok ? await res.text() : null;
  } catch {
    return null;
  }
}

// wasmterm.manifest: simple "key: value" lines, same comment rules as
// config.keys below. Not YAML -- this is parsed in the browser, not by
// Canary's own build-time YAML subset, so it stays deliberately tiny.
async function fetchManifest(appBase) {
  const text = await fetchText(`${appBase}/wasmterm.manifest`);
  const manifest = {};
  if (!text) return manifest;
  for (const rawLine of text.split("\n")) {
    const line = stripComment(rawLine).trim();
    if (!line) continue;
    const sep = line.indexOf(":");
    if (sep === -1) continue;
    const key = line.slice(0, sep).trim();
    const value = line.slice(sep + 1).trim();
    if (key) manifest[key] = value;
  }
  return manifest;
}

// config.keys: one or more whitespace-separated KeyboardEvent.key values
// per line -- deliberately plain text, not a YAML list, since an app with
// a large capture surface should be able to list keys one per line with
// inline comments instead of fighting a single giant scalar. "Space" is
// accepted as an alias for a literal space, since a bare space is awkward
// to author on its own line.
const KEY_ALIASES = { Space: " " };
async function fetchOwnedKeys(appBase) {
  const text = await fetchText(`${appBase}/config.keys`);
  const keys = new Set();
  if (!text) return keys;
  for (const rawLine of text.split("\n")) {
    const line = stripComment(rawLine).trim();
    if (!line) continue;
    for (const token of line.split(/\s+/)) {
      if (token) keys.add(KEY_ALIASES[token] ?? token);
    }
  }
  return keys;
}

// Shared site-root xterm.js, not vendored per app. Lives in
// root-copy/lib/xterm/ so Canary copies it to /lib/xterm/ at the output
// root — one copy for every wasmterm instance, and for anything else on
// the site that wants xterm.
const XTERM_BASE = "/lib/xterm";
let xtermLoad = null;
function loadXtermAssets() {
  if (window.Terminal) return Promise.resolve();
  if (xtermLoad) return xtermLoad;

  const link = document.createElement("link");
  link.rel = "stylesheet";
  link.href = `${XTERM_BASE}/xterm.css`;
  document.head.appendChild(link);

  xtermLoad = new Promise((resolve, reject) => {
    const script = document.createElement("script");
    script.src = `${XTERM_BASE}/xterm.js`;
    script.onload = resolve;
    script.onerror = () => reject(new Error(`wasmterm: failed to load ${script.src}`));
    document.head.appendChild(script);
  });
  return xtermLoad;
}

async function enhance(root) {
  if (root.__enhanced) return;
  root.__enhanced = true;

  const appBase = root.dataset.wasmtermApp;
  const cols = parseInt(root.dataset.wasmtermCols, 10) || 80;
  const rows = parseInt(root.dataset.wasmtermRows, 10) || 32;
  const fontSize = parseInt(root.dataset.wasmtermFontsize, 10) || 24;

  const mount = root.querySelector("[data-wasmterm-mount]");
  const viewport = root.querySelector("[data-wasmterm-viewport]");
  const loadingEl = root.querySelector("[data-wasmterm-loading]");
  const loadingFill = root.querySelector("[data-wasmterm-loading-fill]");
  const loadingLabel = root.querySelector("[data-wasmterm-loading-label]");

  // These three are independent fetches -- the shared xterm library, and
  // this app's two small config files -- so they run in parallel rather
  // than serialized one after another.
  const [, manifest, ownedKeys] = await Promise.all([
    loadXtermAssets(),
    fetchManifest(appBase),
    fetchOwnedKeys(appBase),
  ]);
  const keyExportPath = manifest.keyEventExport;
  const audioExportPath = manifest.audioFillExport;

  // A manifest that wants key capture but has no (or an empty) config.keys
  // next to it is almost certainly an app-author mistake, not an
  // intentional "capture nothing" -- worth surfacing loudly rather than
  // silently doing nothing, unlike a plain missing manifest/config.keys
  // pair, which just means "this app doesn't use that capability."
  if (keyExportPath && ownedKeys.size === 0) {
    console.warn(`wasmterm: ${appBase}/wasmterm.manifest declares keyEventExport but ${appBase}/config.keys is missing or empty -- no keys will be captured`);
  }

  const term = new Terminal({
    cols,
    rows,
    fontSize,
    cursorBlink: false,
    convertEol: false,
    allowTransparency: false,
    scrollback: 0, // fixed single-screen grid, no history to scroll to
  });
  term.open(mount);

  // Measures this instance's own frame, not the browser window -- the
  // terminal scales to fit whatever box the surrounding page gives it,
  // same as an <iframe>/<video> would.
  function fitViewport() {
    const termEl = mount.querySelector(".xterm-screen") ?? mount.querySelector(".xterm");
    if (!termEl) return;
    const naturalWidth = termEl.offsetWidth || viewport.offsetWidth;
    const naturalHeight = termEl.offsetHeight || viewport.offsetHeight;
    if (!naturalWidth || !naturalHeight) return;

    // The frame's CSS aspect-ratio is only a rough placeholder until this
    // runs -- match it to the terminal's own measured cell metrics instead
    // of guessing from cols:rows.
    root.style.aspectRatio = `${naturalWidth} / ${naturalHeight}`;

    // Capped at 1x -- see wasmterm.js's known-issues note in ../README.md
    // for why upscaling past 1x isn't enabled yet.
    const scale = Math.min(root.clientWidth / naturalWidth, root.clientHeight / naturalHeight, 1) * 0.98;
    viewport.style.transform = `scale(${scale})`;
  }
  window.addEventListener("resize", fitViewport);
  requestAnimationFrame(() => requestAnimationFrame(fitViewport));

  // Web Audio backend: a ScriptProcessorNode's callback runs synchronously
  // on the main thread via Web Audio's own scheduler, so no postMessage
  // relay to a worklet realm is needed. Always wired (harmless if the app
  // never calls audio.start) -- only actually pulls samples if
  // audioExportPath resolves to a real export.
  let audioCtx = null;
  let audioNode = null;
  let exports = null; // assigned once getAssemblyExports resolves, below

  function audioStart(sampleRate) {
    audioStop();
    if (!audioExportPath) return;
    audioCtx = new (window.AudioContext || window.webkitAudioContext)({ sampleRate });
    const bufferSize = 2048;
    audioNode = audioCtx.createScriptProcessor(bufferSize, 0, 2);
    audioNode.onaudioprocess = (e) => {
      const fillBuffer = resolveExport(exports, audioExportPath);
      const samples = fillBuffer?.(bufferSize * 2) ?? [];
      const left = e.outputBuffer.getChannelData(0);
      const right = e.outputBuffer.getChannelData(1);
      for (let i = 0; i < bufferSize; i++) {
        left[i] = samples[i * 2] ?? 0;
        right[i] = samples[(i * 2) + 1] ?? 0;
      }
    };
    audioNode.connect(audioCtx.destination);
  }

  function audioStop() {
    if (audioNode) {
      audioNode.disconnect();
      audioNode.onaudioprocess = null;
      audioNode = null;
    }
    if (audioCtx) {
      audioCtx.close();
      audioCtx = null;
    }
  }

  // Raw KeyboardEvent (not xterm's parsed VT sequences) -- gives keyup as
  // well as keydown, which held-key DAS/ARR-style tracking needs.
  // Returning false tells xterm not to also handle keys the app owns
  // (arrows/space/etc. would otherwise scroll or get intercepted by
  // xterm's own key bindings).
  if (keyExportPath) {
    term.attachCustomKeyEventHandler((e) => {
      if (!ownedKeys.has(e.key)) return true;

      // Opportunistically nudge a suspended AudioContext on every owned
      // keypress -- maximizes the chance a browser's autoplay policy
      // counts audio start as gesture-initiated, since audio actually
      // starts some event-loop ticks after this handler returns, not
      // synchronously within it.
      if (audioCtx && audioCtx.state === "suspended") {
        audioCtx.resume();
      }

      const onKeyEvent = resolveExport(exports, keyExportPath);
      onKeyEvent?.(e.key, e.type === "keydown", e.shiftKey, e.altKey, e.ctrlKey);
      return false;
    });
  }

  function setLoadingProgress(fraction, label) {
    loadingFill.classList.add("determinate");
    loadingFill.style.width = `${Math.max(0, Math.min(1, fraction)) * 100}%`;
    if (label) loadingLabel.textContent = label;
  }

  function hideLoading() {
    loadingEl.remove();
  }

  const { dotnet } = await import(`${appBase}/_framework/dotnet.js`);
  const { setModuleImports, getAssemblyExports, getConfig, runMain } = await dotnet.create();

  // Module name is fixed to "main.js" -- not this file's actual name, a
  // logical JSImport module identifier the .NET app's own C# JSImport
  // attributes must target verbatim. See ../INTEROP.md.
  // The APP's own base directory, not document.baseURI -- an app that
  // fetches its own relative assets (e.g. Tesselate's soundfonts via an
  // HttpClient BaseAddress) needs a base it can resolve relative paths
  // against, and that has to be wherever the app's files actually live,
  // which is unrelated to whatever page happens to be embedding it. A
  // trailing slash matters here: without one, resolving "assets/x" against
  // ".../wwwroot" drops "wwwroot" instead of appending to it (a base
  // ending in a segment with no trailing slash is treated as a filename,
  // per standard URL-combining rules).
  const appBaseUri = new URL(appBase.endsWith("/") ? appBase : `${appBase}/`, document.baseURI).href;

  setModuleImports("main.js", {
    page: {
      baseUri: () => appBaseUri,
    },
    term: {
      write: (text) => term.write(text),
    },
    storage: {
      getItem: (key) => localStorage.getItem(key),
      setItem: (key, value) => localStorage.setItem(key, value),
    },
    audio: {
      start: audioStart,
      stop: audioStop,
    },
    loading: {
      setProgress: setLoadingProgress,
      hide: hideLoading,
    },
  });

  const config = getConfig();
  exports = await getAssemblyExports(config.mainAssemblyName);

  // Runs the app's real top-level code (menu/game loop, whatever) and
  // keeps the runtime alive to keep servicing JSExport calls afterward.
  await runMain();
}

function enhanceAll(scopeRoot) {
  scopeRoot.querySelectorAll('[data-widget="wasmterm"]').forEach((root) => {
    enhance(root).catch((err) => console.error("wasmterm widget failed to start", err));
  });
}

enhanceAll(document);
new MutationObserver(() => enhanceAll(document)).observe(document.body, { childList: true, subtree: true });
