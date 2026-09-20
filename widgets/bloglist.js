/*
 * Behavior for the bloglist widget (see bloglist.html). Event delegation
 * on document (per WIDGETS.md) so instances spliced in by the router work
 * without any init call.
 *
 * Persisted per browser (localStorage, every access guarded):
 *   bloglist-view   "list" | "tiles" (default "tiles"; one key for every list)
 *   bloglist-filter "all" | "unread"
 *   bloglist-read   JSON array of post urls marked read -- shared by every
 *                   bloglist on the site, so a post read from one list is
 *                   read in all of them.
 */
(function () {
  const VIEW_KEY = "bloglist-view";
  const FILTER_KEY = "bloglist-filter";
  const READ_KEY = "bloglist-read";

  function load(key) {
    try { return localStorage.getItem(key); } catch (e) { return null; }
  }
  function save(key, value) {
    try { localStorage.setItem(key, value); } catch (e) { /* storage blocked */ }
  }

  function loadRead() {
    try {
      const parsed = JSON.parse(load(READ_KEY) || "[]");
      return new Set(Array.isArray(parsed) ? parsed : []);
    } catch (e) {
      return new Set();
    }
  }

  // Cached for the page's lifetime; the "storage" event refreshes it when
  // another tab changes it.
  let readSet = loadRead();

  function setPressed(root, attr, value) {
    root.querySelectorAll("[" + attr + "]").forEach((btn) => {
      btn.setAttribute("aria-pressed", String(btn.getAttribute(attr) === value));
    });
  }

  function setView(root, view) {
    root.dataset.view = view;
    setPressed(root, "data-view-btn", view);
  }

  function setFilter(root, filter) {
    root.dataset.filter = filter;
    setPressed(root, "data-filter-btn", filter);
    refresh(root);
  }

  function sortValue(item, key) {
    if (key === "name") return item.dataset.name.toLowerCase();
    if (key === "size") return Number(item.dataset.bytes) || 0;
    return item.dataset.date || "0"; // yyyymmdd sorts lexically; undated sorts as oldest
  }

  function applySort(root, key, dir) {
    root.dataset.sort = key;
    root.dataset.dir = dir;
    const list = root.querySelector(".bl-items");
    const items = Array.from(list.querySelectorAll(".bl-item"));
    const sign = dir === "asc" ? 1 : -1;
    items.sort((a, b) => {
      const va = sortValue(a, key);
      const vb = sortValue(b, key);
      if (va < vb) return -sign;
      if (va > vb) return sign;
      // Ties keep the generated (source) order -- matters for a hand-curated
      // feed where same-day items are deliberately ordered.
      return Number(a.dataset.idx) - Number(b.dataset.idx);
    });
    items.forEach((item) => list.appendChild(item));

    root.querySelectorAll("[data-sort-btn]").forEach((btn) => {
      const active = btn.dataset.sortBtn === key;
      btn.classList.toggle("active", active);
      btn.querySelector(".bl-arrow").textContent = active ? (dir === "asc" ? "▲" : "▼") : "";
    });
  }

  // Explorer-style: whole KB rounded up, MB with one decimal past 1 MB.
  // Mirrors FormatSize in tools/blog-list-generator.cs.
  function formatSize(bytes) {
    if (bytes >= 1024 * 1024) return (bytes / 1024 / 1024).toFixed(1) + " MB";
    return Math.max(1, Math.ceil(bytes / 1024)).toLocaleString("en-US") + " KB";
  }

  // Re-applies read state + filter to one widget, then updates its status bar
  // (count and size follow whatever is currently visible).
  function refresh(root) {
    const unreadOnly = root.dataset.filter === "unread";
    const query = (root.querySelector(".bl-search").value || "").trim().toLowerCase();
    let shown = 0;
    let bytes = 0;
    root.querySelectorAll(".bl-item").forEach((item) => {
      const isRead = readSet.has(item.dataset.url);
      item.classList.toggle("is-read", isRead);
      item.querySelector(".bl-readbox").checked = isRead;
      const hide = (unreadOnly && isRead)
        || (query !== "" && !item.dataset.name.toLowerCase().includes(query));
      item.hidden = hide;
      if (!hide) {
        shown++;
        bytes += Number(item.dataset.bytes) || 0;
      }
    });

    const empty = root.querySelector(".bl-empty");
    empty.hidden = shown > 0;
    empty.textContent = query !== ""
      ? "No posts match “" + query + "”."
      : "All caught up. No unread posts.";
    const count = root.querySelector(".bl-count");
    const total = Number(count.dataset.totalCount);
    count.textContent = shown !== total
      ? shown + " of " + total + " object(s)"
      : shown + " object(s)";
    root.querySelector(".bl-total").textContent = formatSize(bytes);
  }

  function refreshAll() {
    document.querySelectorAll(".bloglist").forEach(refresh);
  }

  function init(root) {
    if (root.dataset.blReady) return;
    root.dataset.blReady = "1";
    root.querySelectorAll(".bl-item").forEach((item, i) => { item.dataset.idx = i; });
    const view = load(VIEW_KEY);
    if (view === "list" || view === "tiles") setView(root, view);
    const filter = load(FILTER_KEY);
    root.dataset.filter = filter === "unread" ? "unread" : "all";
    setPressed(root, "data-filter-btn", root.dataset.filter);
    applySort(root, root.dataset.sort, root.dataset.dir);
    refresh(root);
  }

  document.addEventListener("click", (e) => {
    const viewBtn = e.target.closest("[data-view-btn]");
    if (viewBtn) {
      setView(viewBtn.closest(".bloglist"), viewBtn.dataset.viewBtn);
      save(VIEW_KEY, viewBtn.dataset.viewBtn);
      return;
    }
    const filterBtn = e.target.closest("[data-filter-btn]");
    if (filterBtn) {
      setFilter(filterBtn.closest(".bloglist"), filterBtn.dataset.filterBtn);
      save(FILTER_KEY, filterBtn.dataset.filterBtn);
      return;
    }
    const sortBtn = e.target.closest("[data-sort-btn]");
    if (sortBtn) {
      const root = sortBtn.closest(".bloglist");
      const key = sortBtn.dataset.sortBtn;
      // Same column toggles direction; a new one starts at its natural order.
      const dir = root.dataset.sort === key
        ? (root.dataset.dir === "asc" ? "desc" : "asc")
        : (key === "name" ? "asc" : "desc");
      applySort(root, key, dir);
    }
  });

  document.addEventListener("change", (e) => {
    const box = e.target.closest(".bl-readbox");
    if (!box) return;
    const url = box.closest(".bl-item").dataset.url;
    if (box.checked) readSet.add(url);
    else readSet.delete(url);
    save(READ_KEY, JSON.stringify(Array.from(readSet)));
    refreshAll(); // the same post can appear in more than one list on a page
  });

  document.addEventListener("input", (e) => {
    if (e.target.classList && e.target.classList.contains("bl-search")) {
      refresh(e.target.closest(".bloglist"));
    }
  });

  // Escape clears the search box (type=search's own clear button covers the mouse).
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && e.target.classList && e.target.classList.contains("bl-search") && e.target.value) {
      e.target.value = "";
      refresh(e.target.closest(".bloglist"));
    }
  });

  window.addEventListener("storage", (e) => {
    if (e.key !== READ_KEY && e.key !== null) return;
    readSet = loadRead();
    refreshAll();
  });

  new MutationObserver(() => {
    document.querySelectorAll(".bloglist:not([data-bl-ready])").forEach(init);
  }).observe(document.documentElement, { childList: true, subtree: true });

  const start = () => document.querySelectorAll(".bloglist").forEach(init);
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", start);
  else start();
})();
