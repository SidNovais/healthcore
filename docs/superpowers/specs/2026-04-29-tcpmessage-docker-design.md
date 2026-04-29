# TcpMessage Docker — Design

**Date:** 2026-04-29
**Phase:** 10 (final phase of TcpMessage implementation)

## Context

All TcpMessage phases 1–9 are complete. Phase 10 containerises the Worker Service so the full development stack (PostgreSQL + TcpMessage) can be started with a single `docker-compose up`.

## Approach

**Minimal dev setup** — TLS disabled, no certificate management needed locally. Follows the same multi-stage Dockerfile pattern used in the Wann project, adapted for .NET 10 and the HC.LIS multi-project source layout.

## Dockerfile (`Dockerfile.tcpmessage` at repo root)

Two-stage build:

1. **Build stage** (`sdk:10.0`) — copies `Directory.Build.props` + `Directory.Build.targets` (MSBuild needs these to resolve cross-project references), then copies the full `src/` tree, restores, and publishes `HC.LIS.TcpMessage.csproj` to `/app/out`.
2. **Runtime stage** (`runtime:10.0`) — copies published output, exposes port 8890, sets entrypoint.

`runtime:10.0` (not `aspnet:10.0`) is correct: TcpMessage is a Generic Host Worker with no ASP.NET Core dependency.

Build context is the repo root so `Directory.Build.props` / `Directory.Build.targets` are reachable.

## `.dockerignore` (`Dockerfile.tcpmessage.dockerignore`)

Named alongside the Dockerfile (Docker 20.10+ picks it up automatically). Excludes `.git`, `bin/`, `obj/`, IDE folders, and other Dockerfile/compose files to keep the build context small.

## Compose Service (`development-compose.yaml`)

Service `tcpmessage-healthcore`:
- Builds from `Dockerfile.tcpmessage` with context `.`
- `depends_on: postgres-healthcore`
- Publishes port `8890:8890`
- Environment: `DATABASE_CONNECTION_STRING` points to `postgres-healthcore` by Docker service name; `Tcp__UseTls=false`; `Tcp__Port=8890`

## Verification

```bash
docker build -f Dockerfile.tcpmessage -t hclis-tcpmessage .
docker-compose -f development-compose.yaml up -d
docker logs tcpmessage-healthcore   # expect: Listening on port 8890
```
