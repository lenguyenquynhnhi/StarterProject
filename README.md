# DevOpsLab — Starter Project

Reference application for **MH24 — Software Application Development (DevOps)**,
Van Lang Saigon College.

You are **not** here to write CRUD features. The application below already works.
Over 15 in-class sessions (plus online support on MS Teams) you will wrap it in a production-grade delivery pipeline:
source control governance, automated testing, CI, containers, supply-chain
security, continuous delivery, zero-downtime deployment, observability and
incident response.

---

## 1. What you are given

```
DevOpsLab/
├── src/
│   ├── DevOpsLab.Domain/           Entities and business rules. No dependencies.
│   ├── DevOpsLab.Application/      Use cases and abstractions. No ORM, no web.
│   ├── DevOpsLab.Infrastructure/   EF Core, SQL Server, repositories, seeding.
│   └── DevOpsLab.WebApi/           Minimal API, health checks, Serilog, Swagger.
├── tests/
│   ├── DevOpsLab.UnitTests/        Fast, no I/O.
│   └── DevOpsLab.IntegrationTests/ Boots the real pipeline in memory.
├── scripts/setup.sh | setup.ps1    One-command environment setup.
├── docker-compose.yml              Local SQL Server (development dependency).
└── *.todo files                    Your work, session by session.
```

A small product-catalog domain: `Category` and `Product`, with real business
rules (SKU format, positive price, a maximum price increase per change, stock
reservation). Those rules exist so that later sessions have something meaningful
to test, break on purpose, and protect.

## 2. Prerequisites

| Tool | Version | Check |
|---|---|---|
| .NET SDK | 8.0.x | `dotnet --version` |
| Docker Desktop / Engine | current | `docker info` |
| Git | ≥ 2.30 | `git --version` |

Hardware virtualisation (VT-x / AMD-V) must be enabled in BIOS, and you need
administrator rights to install Docker.

## 3. Setup — one command

```bash
# macOS / Linux / WSL
bash scripts/setup.sh

# Windows PowerShell
powershell -ExecutionPolicy Bypass -File scripts/setup.ps1
```

The script checks your tooling, starts SQL Server, waits until it is genuinely
healthy, builds the solution, creates and applies the database migration, seeds
the catalog and runs the full test suite. If it finishes green, your environment
is ready.

Run the API:

```bash
dotnet run --project src/DevOpsLab.WebApi     # or: make run
```

| URL | Purpose |
|---|---|
| <http://localhost:5080/swagger> | API explorer |
| <http://localhost:5080/healthz/live> | Liveness |
| <http://localhost:5080/healthz/ready> | Readiness |
| <http://localhost:5080/api/products> | Seeded catalog |

## 4. Liveness is not readiness

This distinction is the single most important thing in the starter project.

| Endpoint | Question it answers | Who consumes it | Consequence of failure |
|---|---|---|---|
| `/healthz/live` | Is the process alive? | Container runtime | Restart the container |
| `/healthz/ready` | Can it serve a real request right now? | Load balancer, deploy script | Stop sending traffic |

`/healthz/live` deliberately touches **no** external dependency. If it called the
database, a brief database blip would restart a perfectly healthy container and
turn a small incident into an outage.

`/healthz/ready` checks that the database is reachable **and** that no migration
is pending. In Session 11 your Blue-Green deployment will refuse to switch
traffic until this endpoint returns `Healthy` — a process that has started but is
not ready is exactly how "zero-downtime" deployments cause downtime.

Try it yourself:

```bash
docker compose stop sqlserver
curl -i localhost:5080/healthz/live      # 200 Healthy
curl -i localhost:5080/healthz/ready     # 503 Unhealthy
docker compose start sqlserver
```

## 5. Everyday commands

```bash
make help        # list all targets
make db-up       # start SQL Server
make db-reset    # stop the database and delete its volume
make build       # restore + build
make test        # full test suite
make coverage    # tests with code coverage
make health      # probe both health endpoints
```

## 6. Ground rules

1. **Built-in quality.** You are responsible for the quality of what you push.
   Reviewers and pipelines are a safety net, not a substitute.
2. **Red build = stop and fix first.** A red pipeline stops all releases. Fixing it is
   the team's highest priority, ahead of any new feature.
3. **Never bypass a gate.** No `--no-verify`, no force-push to `main`, no
   disabling a failing test to go green.
4. **Never commit a secret.** Configuration lives in `.env` (git-ignored) or in
   GitHub Secrets. If a secret ever leaks: rotate it first, scrub history second.
5. **Blameless postmortems.** When something breaks we fix the system that
   allowed it, not the person who tripped over it.

## 7. Session map

15 in-class sessions of 4 hours, plus 15 hours of online support on MS Teams
(weekly Q&A, pull-request reviews, pipeline troubleshooting, and the integration
review before Demo Day).

| Session | You build |
|---|---|
| 1 | Onboarding, team formation, Kanban |
| 2 | Git hooks, branch protection, secret push protection |
| 3–4 | Unit tests, integration tests with Testcontainers, coverage gate |
| 5 | CI pipeline and required status checks |
| 6 | Multi-stage Dockerfile, non-root, health checks, Compose orchestration |
| 7 | Secrets, SAST, SCA, Trivy, SBOM |
| 8 | Registry, build provenance, SemVer, build once – promote everywhere |
| 9–10 | Continuous Delivery, Expand/Contract migrations, production gate |
| 11 | Blue-Green deployment, zero downtime, automated rollback |
| 12–13 | Structured logging, metrics, dashboards, independent alerting, SLOs, runbook |
| 14 | Incident response, Game Day, blameless postmortem |
| 15 | Demo Day (release v1.0.0 is reviewed online beforehand) |

## 8. Where to record your work

| File | Created in | Purpose |
|---|---|---|
| `DevOps-Backlog.md` | Session 1 | Your team's backlog, mirrored on the Kanban board |
| `docs/TEAM-CHARTER.md` | Session 1 | Roles, working agreements, quality pledge |
| `docs/adr-*.md` | Session 6 onwards | Short records of technical decisions |
| `RUNBOOK.md` | Session 13 | How to operate and rescue the system |
| `POSTMORTEM.md` | Session 14 | Incident analysis, preventive actions |
