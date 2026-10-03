# DevOps Backlog — Team `<your-team-name>`

Mirror this backlog on your GitHub Projects Kanban board
(`Backlog → Ready → In Progress → In Review → Done`).

A card may only move to **Done** when its Definition of Done is met **and** the
evidence column contains a link — a pull request, a workflow run, a screenshot
or a terminal capture. "It works on my machine" is not evidence.

---

## Definition of Done (applies to every card)

- [ ] Work delivered through a Pull Request, never pushed straight to `main`
- [ ] At least one teammate reviewed and approved
- [ ] All automated checks green — no gate skipped, no test disabled
- [ ] No secret, credential or connection string added to the repository
- [ ] Evidence link recorded on the card
- [ ] Documentation updated when behaviour changed

---

## Sprint 1 — From commit to a secure artifact (Sessions 1–8)

| # | Card | Session | Owner | Definition of Done | Evidence |
|---|---|---|---|---|---|
| 1 | Environment ready for every member | 1 | | `setup` script green on all machines; `/healthz/ready` returns Healthy | |
| 2 | Team charter signed | 1 | | `docs/TEAM-CHARTER.md` committed, roles assigned | |
| 3 | Kanban board created | 1 | | Board with 5 columns; this backlog mirrored | |
| 4 | Conventional Commits adopted | 2 | | `commit-msg` hook rejects a non-conforming message | |
| 5 | Git hooks installed (Husky.Net) | 2 | | Bad format and a staged secret are both blocked locally | |
| 6 | Branch protection enabled | 2 | | Direct push to `main` rejected; review required | |
| 7 | Secret push protection enabled | 2 | | A pushed test secret is blocked server-side | |
| 8 | Unit tests extended | 3 | | ≥ 3 new failure-case tests; all green | |
| 9 | Integration tests on Testcontainers | 4 | | Tests pass locally and on a clean machine | |
| 10 | Smoke test script | 4 | | Returns a non-zero exit code on failure | |
| 11 | Coverage gate ≥ 75% | 4 | | Build fails below the threshold | |
| 12 | CI workflow | 5 | | Runs on every PR; required status check configured | |
| 13 | Quality gate proven | 5 | | A deliberately broken PR cannot be merged | |
| 14 | Multi-stage Dockerfile | 6 | | Image builds; SDK absent from the final image | |
| 15 | Non-root container + base image ADR | 6 | | `docker run --rm <image> id` shows a non-root user | |
| 16 | Compose orchestration | 6 | | API + database up with one command, healthchecks wired | |
| 17 | Secrets moved out of the repo | 7 | | No credential in git history or working tree | |
| 18 | SAST with CodeQL | 7 | | Findings visible in the Security tab | |
| 19 | Dependency scanning that really fails | 7 | | A known-vulnerable package turns the job red | |
| 20 | Trivy image scan | 8 | | No Critical/High, or a justified `.trivyignore` entry | |
| 21 | SBOM generated | 8 | | SBOM published as a build artifact | |
| 22 | Build provenance attested | 8 | | Attestation verifiable for the published image | |
| 23 | Image published to the registry | 8 | | Tagged with SemVer, pushed only when CI is green | |

## Sprint 2 — From artifact to operated production (Sessions 8–15, plus the online review)

| # | Card | Session | Owner | Definition of Done | Evidence |
|---|---|---|---|---|---|
| 24 | CI optimised with caching | 8 | | Measured before/after build time | |
| 25 | Reusable workflow extracted | 8 | | Shared job used by more than one workflow | |
| 26 | Automatic SemVer (release-please) | 8 | | A `feat:` commit bumps the minor version | |
| 27 | Promotion by image digest | 8 | | Deployments reference `sha256:…`, never a mutable tag | |
| 28 | Staging environment | 9 | | Auto-deploy on merge to `main` | |
| 29 | Smoke tests on Staging | 9 | | Pipeline stops when they fail | |
| 30 | Expand/Contract migration | 10 | | Previous version still runs after the Expand phase | |
| 31 | Production approval gate | 10 | | Deployment waits for a named reviewer | |
| 32 | Nginx Blue-Green proxy | 11 | | Traffic switches between colours without dropped connections | |
| 33 | Zero-downtime deploy proven | 11 | | 0% failed requests under load during a deploy | |
| 34 | Automated rollback | 11 | | A deliberately broken release rolls back by itself | |
| 35 | Serilog JSON in production | 12 | | Logs queryable with full context | |
| 36 | Metrics and dashboard | 12 | | Golden Signals visible in Grafana | |
| 37 | Alerting independent of the app | 13 | | Alert still fires when the application is down | |
| 38 | SLO published | 13 | | Availability and latency SLO with an error budget | |
| 39 | RUNBOOK.md | 13 | | A teammate can recover the system using it alone | |
| 40 | Game Day survived | 14 | | Detected, diagnosed and recovered within the time limit | |
| 41 | POSTMORTEM.md | 14 | | Root cause and preventive actions with owners | |
| 42 | Release v1.0.0 | Online | | Tagged, published, reproducible from the pipeline | |
| 43 | Demo Day rehearsed | Online | | Both scenarios executed end to end | |
