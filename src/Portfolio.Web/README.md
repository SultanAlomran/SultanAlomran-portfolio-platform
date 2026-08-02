# Portfolio.Web

The custom public portfolio application targets stable Angular 22 with a compatible TypeScript 6.0 release and Tailwind CSS v4+. It is independently installed, built, deployed, and upgraded from `Portfolio.Admin`; the applications do not share Angular runtime packages.

Node.js 22 LTS (`>=22.12.0 <23`) is the common supported runtime selected for both Angular applications. Generate and commit this application's own `package-lock.json` from an environment with npm registry access before using `npm ci`.
