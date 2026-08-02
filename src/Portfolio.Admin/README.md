# Portfolio.Admin

The private administration shell targets stable Angular 20 with compatible TypeScript 5.9 and Tailwind CSS v4+. Angular 20 is intentional because the current official Metronic Tailwind Angular integration example targets Angular 20 with Tailwind CSS v4+.

Metronic integration remains deferred until the licensed package is supplied and reviewed for version, licensing, bundle-size, and integration constraints. No Metronic assets are included in this foundation.

This application is independently installed, built, deployed, and upgraded from `Portfolio.Web`; it does not share Angular runtime packages. Node.js 22 LTS (`>=22.12.0 <23`) is the selected common runtime. Generate and commit this application's own `package-lock.json` from an environment with npm registry access before using `npm ci`.
