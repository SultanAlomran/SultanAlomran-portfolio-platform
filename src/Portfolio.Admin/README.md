# Portfolio.Admin

The private administration application is an independent Angular 20 application built on the licensed Metronic Tailwind Demo 1 shell. Its Angular version intentionally remains separate from `Portfolio.Web`.

## Foundation

- Angular 20.3.7 with strict standalone components and route-level lazy loading
- Tailwind CSS 4 with Portfolio violet/indigo design tokens layered over Metronic
- Metronic shell runtime, KeenIcons, sidebar, responsive mobile drawer, desktop collapse, header, dropdown, and theme support
- Reusable page headers, breadcrumbs, feedback states, tables, filters, pagination, dialogs, badges, skeletons, and upload progress
- Placeholder routes only; authentication, APIs, business features, and persistence are intentionally absent

## Local development

Node.js `>=22.12.0 <23` is required.

```powershell
npm ci
npm start
```

The application runs at `http://localhost:4300/`.

## Verification

```powershell
npm run lint
npm run build
```

Use `npm install` only when intentionally changing dependencies or regenerating `package-lock.json`. Normal restores should use `npm ci`.
