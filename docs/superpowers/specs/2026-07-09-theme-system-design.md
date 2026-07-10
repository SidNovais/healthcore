# Theme System (Dark / Light) — Design Spec

**Date:** 2026-07-09  
**Status:** Approved

## Problem

The HC.LIS Angular SPA uses a comprehensive CSS custom-property design system (`src/styles.css`) but only supports a single light theme. Users working in low-light environments have no way to reduce eye strain.

## Decision

Add user-controlled dark/light switching with `localStorage` persistence. Approach: CSS `[data-theme="dark"]` attribute on `<html>`, toggled by a `ThemeService`. No external dependencies.

## Organizational Boundaries

Each concern lives in exactly one file — swapping the approach later requires touching only one seam:

| Seam | Owner | To change, edit only… |
|---|---|---|
| Storage | `ThemeService` | `theme.service.ts` |
| CSS token overrides | `styles.css` | `[data-theme="dark"]` block |
| DOM mutation | `ThemeService.apply()` | one private method |

## Architecture

### CSS layer (`src/styles.css`)

A `[data-theme="dark"]` block overrides all `--color-*` and `--shadow-*` tokens. Semantic colors (`--color-error`, `--color-success`, `--color-warning`) and role badge colors are unchanged — they're vivid enough for both themes. A `transition` on `body` gives a smooth 200ms fade.

### ThemeService (`src/app/core/application/theme.service.ts`)

```ts
theme = signal<Theme>(this.load())  // reactive state
init(): void                         // called once on app startup
toggle(): void                       // called by the shell toggle button
private apply(t: Theme): void        // sets data-theme attr + localStorage + signal
private load(): Theme                // reads localStorage, defaults to 'light'
```

### App initialization (`src/app/app.ts`)

`ThemeService.init()` is called in the `AppComponent` constructor so the correct theme attribute is set before any child component renders, preventing a flash of wrong theme.

### Toggle UI (`src/app/core/shell/`)

A sun/moon icon button sits in the `sidebar-footer`, between the username and the logout button. It mirrors the `.logout-btn` style (icon-only, ghost, same padding/radius). Sun icon = currently dark (click to go light). Moon icon = currently light (click to go dark).

## Files Changed

| File | Change |
|---|---|
| `src/styles.css` | Added `[data-theme="dark"]` block + `body` transition |
| `src/app/core/application/theme.service.ts` | New — signal, toggle(), apply(), localStorage |
| `src/app/app.ts` | Injected ThemeService for pre-render init |
| `src/app/core/shell/shell.component.ts` | Injected ThemeService, exposed as `themeService` |
| `src/app/core/shell/shell.component.html` | Added theme toggle button to `sidebar-footer` |
| `src/app/core/shell/shell.component.css` | Added `.theme-toggle-btn` (shares base style with `.logout-btn`) |

## Verification

1. Log in — moon icon visible in sidebar footer (light mode default)
2. Click toggle — smooth dark transition; all surfaces, text, and borders reflect dark palette
3. Reload — dark mode persists via localStorage
4. Click again — returns to light mode
5. DevTools → Elements → confirm `<html data-theme="dark">` attribute toggling correctly
6. `yarn e2e` — no existing E2E tests regress
