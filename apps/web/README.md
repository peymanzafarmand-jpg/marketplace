# Baby & Kids Marketplace — Frontend Foundation

Next.js + TypeScript + Tailwind CSS foundation for the customer-facing
Marketplace. **This is Task 1.2 — Frontend Foundation only.** No
marketplace features (catalog, cart logic, checkout, seller/admin panels)
are implemented yet; this repo establishes the base every feature will
be built on top of.

- **Language / direction:** Persian (`fa`), RTL by default, structure is
  i18n-ready for additional locales later (see `lib/i18n/`).
- **Design:** Mobile-first, centralized design tokens (see `styles/globals.css`).

## Stack

| Concern | Choice |
|---|---|
| Framework | Next.js 16 (App Router, Turbopack) |
| Language | TypeScript (strict) |
| Styling | Tailwind CSS v4 (CSS-first `@theme` tokens) |
| Icons | lucide-react |
| Variant styling | class-variance-authority |
| State | Zustand (`lib/store/`) |
| Data fetching layer | Custom fetch wrapper (`services/api-client.ts`) |
| Testing | Vitest + React Testing Library + jsdom |
| Font | Vazirmatn, self-hosted variable font (`next/font/local`) |

## Folder structure

```
app/                    routes (App Router)
  (public)/              customer-facing route group — wrapped in PublicLayout
    page.tsx              foundation showcase page (all base components)
    layout.tsx
  fonts/                 self-hosted Vazirmatn variable font
  layout.tsx             root layout — lang="fa" dir="rtl", metadata base
  error.tsx               route-level error boundary
  global-error.tsx        root-layout-level error boundary
  not-found.tsx           404 page
  loading.tsx             route-level skeleton loading state
  robots.ts               robots.txt (MetadataRoute.Robots)
  sitemap.ts              sitemap.xml (MetadataRoute.Sitemap)

components/
  ui/                    design-system primitives (Button, Input, Modal, ...)
    __tests__/            component tests
  layout/                Header, Footer, DesktopNav, MobileBottomNav,
                          MobileMenuDrawer, Container, PublicLayout

features/                empty per-feature folders (auth, cart, product) —
                          ready for feature work in the next task

lib/
  i18n/                  locale config + fa dictionary + (future) other locales
  store/                 Zustand stores: auth-store, cart-store, ui-store
  utils/                 cn() and other framework-agnostic helpers

services/                API layer: api-client.ts (interceptors, error
                          handling, auth-ready) + example products.service.ts

hooks/                   use-translations.ts (i18n access point)

types/                   shared TypeScript types (api.ts, domain.ts)

styles/globals.css       centralized design tokens (color, type, spacing,
                          radius, shadow, breakpoints) + base element styles
```

Route groups mirror the customer Sitemap agreed in Task 1.1
(`/`, `/categories`, `/cart`, etc.); those pages will be added inside
`app/(public)/` in the features phase. Seller/Admin panels are
intentionally **not** part of this repo yet — they should get their own
top-level route groups (`app/(seller)/`, `app/(admin)/`) with their own
layouts later, reusing the same `components/ui/` primitives and
`styles/globals.css` tokens. Architecture decisions above are not
changed from what was agreed in Task 1.1.

## Design tokens

All color/typography/spacing/radius/shadow/breakpoint values are defined
once, in `styles/globals.css`, using Tailwind v4's CSS-first `@theme`
block. Components reference them via CSS variables
(`var(--color-brand-500)`, `var(--radius-md)`, etc.) rather than raw
Tailwind palette classes, so the whole visual identity can be re-tuned
from one file. See that file's comments for the reasoning behind the
palette (teal + apricot, chosen deliberately against the generic
"cream + terracotta" AI-default look).

## Base components

Button · Input · Textarea · Select · Checkbox · Radio · Modal · Drawer ·
Toast (+ `ToastContainer`) · Badge · Card (+ Header/Body/Footer) ·
Skeleton · Pagination · Tabs · Dropdown

All exported from `components/ui/index.ts`. Every interactive component
has visible focus states, proper `label`/`aria-*` wiring, and respects
`prefers-reduced-motion`.

## Run instructions

```bash
# 1. install dependencies
npm install

# 2. copy env template and adjust if needed
cp .env.example .env.local

# 3. run the dev server
npm run dev
# → http://localhost:3000

# 4. production build
npm run build
npm run start
```

## Test instructions

```bash
npm run test        # run once (CI mode)
npm run test:watch  # watch mode
```

**Current result:** 3 test files, 8 tests, all passing —
`Button.test.tsx`, `Input.test.tsx`, `Badge.test.tsx`.

## Lint

```bash
npm run lint
```

Passes with 0 errors / 0 warnings.

## Docker

```bash
docker build -t marketplace-web .
docker run -p 3000:3000 \
  -e NEXT_PUBLIC_SITE_URL=http://localhost:3000 \
  -e NEXT_PUBLIC_API_BASE_URL=http://localhost:8080/api/v1 \
  marketplace-web
```

The Dockerfile is a 3-stage build (`deps` → `builder` → `runner`) on
`node:22-alpine`, relying on `next.config.ts`'s `output: "standalone"`
to produce a minimal runtime image that runs as a non-root user.

> **Note on verification:** `npm run build`, `npm run lint`, and
> `npm run test` were all run and passed in the environment this repo
> was prepared in. The Docker image itself could not be built in that
> same environment (no Docker daemon available there) — the Dockerfile
> follows the standard, well-tested Next.js `standalone`-output pattern
> and the `.next/standalone` output it depends on was confirmed to be
> generated correctly by the verified `npm run build`. Please still run
> `docker build` once in your own environment as a final check before
> relying on it in CI/CD.

## Environment variables

See `.env.example`:

- `NEXT_PUBLIC_SITE_URL` — used for `metadataBase`, OpenGraph, sitemap/robots.
- `NEXT_PUBLIC_API_BASE_URL` — base URL the API client (`services/api-client.ts`) calls.

## Security — Access Token Storage

**The access token is never written to persistent storage.** It is not
in `localStorage`, not in `sessionStorage`, and not in any cookie that
JavaScript can read. It exists only in memory, inside `useAuthStore`
(`lib/store/auth-store.ts`), for the lifetime of the page.

Concretely:

- `useAuthStore`'s `persist` middleware uses an explicit `partialize`
  allow-list containing only `user` and `isAuthenticated`. `accessToken`
  is not in that list and is therefore never serialized to
  `localStorage`, no matter what else changes in the store shape later.
- Because the token lives only in memory, it does **not** survive a full
  page reload, a new tab, or a browser restart — this is intentional,
  not a bug to "fix" later.
- Session continuity across reloads is instead recovered by calling
  `bootstrapSession()` (`services/auth.service.ts`), which calls
  `POST /auth/refresh-token`. That endpoint reads the actual refresh
  token from an **HttpOnly cookie** set by the backend at login — a
  cookie JavaScript cannot read, so it cannot be exfiltrated by an XSS
  payload — and returns a fresh access token to hold in memory again.
- `bootstrapSession()` is implemented and exported now, but not yet
  called from anywhere in the app; wiring it into a root client
  component (so it runs once on app start, before any authenticated
  request) is part of the upcoming Auth UI feature work, along with the
  real login/logout calls in `services/auth.service.ts`.

This directly follows the two constraints from the original Frontend
and Security docs: authentication is Context/store + HttpOnly-cookie
based, and no token that unlocks a session is ever placed somewhere an
XSS payload could read it.

## Definition of Done — status

| Item | Status |
|---|---|
| Project runs | ✅ `npm run dev` verified (HTTP 200, RTL markup confirmed) |
| RTL correct | ✅ `<html lang="fa" dir="rtl">`, logical CSS properties (`ms-`/`me-`/`ps-`/`pe-`/`start-`/`end-`) used throughout instead of left/right |
| Responsive | ✅ mobile-first; bottom tab bar below `md`, header search/desktop nav from `sm`/`md` up |
| All base components render | ✅ see `app/(public)/page.tsx` showcase |
| Tests run | ✅ passing (see Task 1.6 additions below) |
| Docker build succeeds | ⚠️ Dockerfile written against a verified `standalone` build output; not build-tested in this sandbox (no Docker daemon available here — see note above) |
| README written | ✅ this file |

## Notes for the Product Manager / next tasks

- `features/auth`, `features/cart`, `features/product` are currently
  empty placeholders — real feature logic starts in the next task.
- `lib/store/*` hold state **shape** only (no network calls wired in yet),
  except `auth-store`'s storage boundary, which is now locked down (see
  Security section above) ahead of the real Auth UI work.
- `services/api-client.ts` already supports an auth `Authorization`
  header via interceptor once `auth-store` has a token — no changes
  needed there when real login is implemented.
- `services/auth.service.ts` is the agreed home for every future auth
  network call (`login`, `logout`, etc.) alongside `bootstrapSession()`.

