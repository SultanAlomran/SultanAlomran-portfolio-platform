# Microsoft Dev Tunnels

This development-only setup exposes the locally running Portfolio API, public website, and Admin application through one private Microsoft Dev Tunnel with a separate HTTPS URL for each port. It does not deploy or host the applications in Azure.

## Local endpoints

| Application | Local endpoint | Tunnel protocol |
| --- | --- | --- |
| Portfolio.Api | `http://localhost:5100` | Public HTTPS to local HTTP |
| Portfolio.Web | `http://localhost:4200` | Public HTTPS to local HTTP |
| Portfolio.Admin | `http://localhost:4300` | Public HTTPS to local HTTP |

The API also has the Visual Studio HTTPS profile `https://localhost:7100`. Dev Tunnels uses the HTTP endpoint on port `5100`; the external relay URL is HTTPS, so a development certificate is not required on the phone or tablet.

The tunneled Angular configurations use a relative `/api` URL. Their Angular development servers proxy `/api` to local API port `5100`. Normal local development and production environment files are unchanged.

## Prerequisites

- .NET SDK required by `global.json`
- Node.js and npm versions required by each Angular application's `package.json`
- Restored npm dependencies for both Angular applications
- A Microsoft personal account, Microsoft Entra ID account, or GitHub account
- Microsoft Dev Tunnel CLI

Install the official CLI on Windows:

```powershell
winget install --id Microsoft.devtunnel --exact
```

Restart the terminal after installation if `devtunnel` is not immediately found.

## First login

Interactive browser login:

```powershell
devtunnel user login
```

Device-code login when an interactive browser cannot open:

```powershell
devtunnel user login -d
```

Verify the account:

```powershell
devtunnel user show
```

The tunnel is private by default. A remote browser must sign in with the same account used by the CLI. Do not add anonymous access for routine development.

## Start the environment

From the repository root, start API, Web, and Admin:

```powershell
.\scripts\start-local.ps1
```

Then create or reuse the private tunnel and host all three ports:

```powershell
.\scripts\dev-tunnel-start.ps1
```

Alternatively, start everything with one command:

```powershell
.\scripts\start-dev-environment.ps1
```

The tunnel script prints the three generated HTTPS URLs. Local process logs are stored under `.dev-environment/`; tunnel state and URLs are stored under `.dev-tunnel/`. Both directories are ignored by Git and must never be committed.

## Stop the environment

Stop only the tunnel host while keeping the reusable remote tunnel:

```powershell
.\scripts\dev-tunnel-stop.ps1
```

Stop the local applications:

```powershell
.\scripts\stop-local.ps1
```

Stop everything:


```powershell
.\scripts\stop-dev-environment.ps1
```

Delete the reusable remote tunnel only when it is no longer needed:

```powershell
.\scripts\dev-tunnel-stop.ps1 -DeleteTunnel
```

Deletion is not part of the normal stop workflow. The next start creates a new tunnel and new URLs.

## Mobile and cross-device testing

1. Keep the development PC awake and connected to the internet.
2. Start the local applications and tunnel host.
3. Open the generated Web or Admin HTTPS URL on the remote device.
4. Complete the Dev Tunnel access sign-in with the same account.
5. Test API-dependent screens through the Web/Admin URL; requests to `/api` are proxied to local port `5100`.
6. Use the API tunnel URL with `/health`, `/api`, or `/openapi/v1.json` for direct API checks.

The remote device does not need to be on the same Wi-Fi network.

## Tunnel reuse and local state

The first successful start creates a 30-day private tunnel. Its generated ID is saved only in `.dev-tunnel/tunnel-id.txt`. Later starts reuse it and therefore preserve the URLs until it expires or is deleted. Never copy this local state file into tracked documentation or configuration.

## Troubleshooting

- **`devtunnel` is not recognized:** restart PowerShell, then run `devtunnel --version`.
- **Authentication required:** run `devtunnel user login` or `devtunnel user login -d`.
- **Port already in use:** stop the owning process or run `.\scripts\stop-local.ps1` if it was started by these scripts.
- **API data fails remotely:** confirm `http://localhost:5100/health` works and that the Angular app was started with `start:tunnel` through the scripts.
- **A process fails to start:** inspect `.dev-environment/logs/`.
- **Tunnel host fails:** inspect `.dev-tunnel/host.out.log` and `.dev-tunnel/host.err.log`.
- **Access prompt repeats:** verify the browser and CLI use the same Microsoft/GitHub identity.
- **Tunnel expired:** delete `.dev-tunnel/tunnel-id.txt` locally and rerun the tunnel start script, or use `-DeleteTunnel` before creating a replacement.

## Security notes

- Dev Tunnels are for development and testing, not production hosting.
- Access remains authenticated and private by default.
- Tunnel URLs are internet-routable; stop hosting when testing is finished.
- Do not expose production data, credentials, secrets, or privileged local services.
- Do not commit `.dev-tunnel/`, `.dev-environment/`, tunnel IDs, access tokens, or logs.
- A tunnel URL is not a replacement for application authentication or authorization.

Official references: [Create and host a tunnel](https://learn.microsoft.com/azure/developer/dev-tunnels/get-started), [Dev Tunnels security](https://learn.microsoft.com/azure/developer/dev-tunnels/security), and [CLI command reference](https://learn.microsoft.com/azure/developer/dev-tunnels/cli-commands).
