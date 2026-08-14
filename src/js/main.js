// Skeleton placeholder handler. Once content/ and manifest.json exist,
// this gets replaced with: fetch content/<path>.md -> parse -> render.

Router.registerRouteHandler(({ path, segments }) => {
  const app = document.getElementById("app");

  if (path === "/") {
    app.innerHTML = `
      <h1>Consoland</h1>
      <p>This is the site skeleton. The home page will render <code>content/home.md</code> here.</p>
    `;
    return;
  }

  app.innerHTML = `
    <h1>${segments[segments.length - 1]}</h1>
    <p>Placeholder for <code>content/${segments.join("/")}.md</code>.</p>
    <p><a href="#/">&larr; back home</a></p>
  `;
});

Router.start();
