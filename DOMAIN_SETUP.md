# Pointing a custom domain at this site

Your site is hosted on GitHub Pages at https://haberling.github.io/consolandWebsite/. This doc covers buying a domain and pointing it there.

## 1. Buy a domain

Any registrar works, but a few notes:

- **Cloudflare Registrar** — sells at wholesale price (no markup), no upsells. Requires your domain to use Cloudflare's DNS, which is also just good DNS. Recommended if you don't already have a registrar you like.
- **Namecheap / Porkbun** — also cheap, no strong reason to avoid either.
- Avoid GoDaddy if you can — aggressive upsells and a worse renewal-price track record. Not broken, just annoying.

Pick a `.com`/`.dev`/`.gg`/whatever reads well for a game/utility site — GitHub Pages doesn't care about TLD.

## 2. Decide apex vs. subdomain

- `consoland.com` (apex/root domain) → needs **A** and **AAAA** records
- `www.consoland.com` or `games.consoland.com` (subdomain) → needs a **CNAME** record

Most people want the apex to work (`consoland.com`, no `www.`) and optionally have `www.consoland.com` redirect to it. You can set up both.

## 3. DNS records to add

At your registrar's DNS settings (or Cloudflare's, if using Cloudflare Registrar):

**For the apex domain** (`consoland.com`), add four A records, all pointing at GitHub's Pages IPs:

| Type | Name | Value |
|------|------|-------|
| A | @ | 185.199.108.153 |
| A | @ | 185.199.109.153 |
| A | @ | 185.199.110.153 |
| A | @ | 185.199.111.153 |
| AAAA | @ | 2606:50c0:8000::153 |
| AAAA | @ | 2606:50c0:8001::153 |
| AAAA | @ | 2606:50c0:8002::153 |
| AAAA | @ | 2606:50c0:8003::153 |

**For a `www` subdomain** (optional, if you want `www.consoland.com` to also work):

| Type | Name | Value |
|------|------|-------|
| CNAME | www | haberling.github.io |

> If using Cloudflare DNS: set these records to **DNS only** (grey cloud), not proxied, at least until the domain is verified and HTTPS is issued on GitHub's side. You can switch to proxied afterward if you want Cloudflare's CDN/protection in front.

## 4. Tell GitHub about the domain

In the repo: **Settings → Pages → Custom domain**, enter `consoland.com` (or whichever you're using) and save. GitHub will:

- Create a `CNAME` file in the `docs/` folder automatically (containing just the domain name) — **don't delete this file**, and note the C# build tool must not wipe it on rebuild (either regenerate it as part of `build`, or exclude it from cleanup).
- Attempt to verify DNS. This can take a few minutes to a few hours to propagate.
- Once DNS resolves, GitHub auto-provisions an HTTPS certificate for the domain (via Let's Encrypt) — this can take up to 24 hours the first time.

## 5. Verify domain ownership (recommended)

GitHub also offers domain **verification** (separate from just pointing DNS at it) under **Settings → Pages → Custom domain → Verify**. This adds a TXT record and proves you own the domain to GitHub, which prevents someone else from claiming your domain for their own GitHub Pages site if your DNS ever gets misconfigured. Worth doing once, takes a few minutes.

## 6. Enforce HTTPS

Once the certificate is issued (Settings → Pages will show a checkbox become available), check **Enforce HTTPS**. This redirects all `http://` traffic to `https://`.

## 7. Sanity check

- `dig consoland.com` (or `nslookup`) should show the four GitHub IPs.
- Visiting `http://consoland.com` should eventually redirect to `https://consoland.com` and show the site.
- Both apex and `www` (if configured) should resolve to the same site.

DNS propagation is the only genuinely slow part here — if it doesn't work immediately, wait an hour and try again before assuming something's misconfigured.
