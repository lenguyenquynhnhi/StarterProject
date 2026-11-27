#!/usr/bin/env bash
# =============================================================================
# plant-pt1.sh — Periodic Test 1: put the three scenarios into ONE team's repo.
#
# INSTRUCTOR ONLY. Run it inside a fresh clone of the team's repository, with
# write access to it:
#
#   git clone https://github.com/<org>/<team-repo>.git && cd <team-repo>
#   bash /path/to/plant-pt1.sh              # dry run: local branches only, shows the diffs
#   bash /path/to/plant-pt1.sh --push       # push the three branches
#   bash /path/to/plant-pt1.sh --push --pr  # push and open the three pull requests (needs gh)
#
# The three scenarios are the same SHAPES as the Session 7 plants P1-P3, in
# DIFFERENT places, so a team cannot pass by remembering what it did last week:
#
#   pt1/s1-cve     A clean package whose dependency has a published advisory:
#                  Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
#                    -> Microsoft.IdentityModel.Protocols.OpenIdConnect 7.0.3
#                    -> System.IdentityModel.Tokens.Jwt 7.0.3   (CVE-2024-21319, Moderate,
#                       fixed in 7.1.2; also GHSA-8g9c-28fc-mcx2)
#                    -> Microsoft.IdentityModel.JsonWebTokens 7.0.3 (same advisories)
#                  Moderate: Trivy's HIGH,CRITICAL image gate will NOT stop it.
#                  Only the team's dependency gate can. That is the test.
#
#   pt1/s2-root    Runtime base changed to the full Ubuntu image and the USER line
#                  removed — "for on-call diagnostics". The full image runs as root.
#                  Expected: Trivy DS002 (no USER), and verify-nonroot.sh if reached.
#
#   pt1/s3-secret  An invented production connection string in launchSettings.json —
#                  a file nobody thinks of as configuration.
#                  Expected: the repository secret scan (course rule).
#
# Every value is invented. Never plant a real credential.
# Every edit is verified after it is made; the script stops if a team's files do
# not look as expected, rather than planting something that does not work.
# =============================================================================
set -euo pipefail

PUSH=0
OPEN_PR=0
for arg in "$@"; do
    case "$arg" in
        --push) PUSH=1 ;;
        --pr)   OPEN_PR=1 ;;
        -h|--help) sed -n '2,40p' "$0"; exit 0 ;;
        *) printf 'unknown option: %s\n' "$arg" >&2; exit 2 ;;
    esac
done
[ "$OPEN_PR" -eq 0 ] || [ "$PUSH" -eq 1 ] || { printf -- '--pr needs --push\n' >&2; exit 2; }

fail() { printf '\nFAIL  %s\n' "$1" >&2; exit 1; }
ok()   { printf 'OK    %s\n' "$1"; }

git rev-parse --is-inside-work-tree >/dev/null 2>&1 || fail "run this inside a clone of the team repository"
[ -z "$(git status --porcelain)" ] || fail "the working tree is not clean"

git fetch --quiet origin main
BASE="origin/main"
printf 'Team repository : %s\n' "$(git remote get-url origin)"
printf 'Base            : %s (%s)\n\n' "$BASE" "$(git rev-parse --short "$BASE")"

CSPROJ="src/DevOpsLab.WebApi/DevOpsLab.WebApi.csproj"
LAUNCH="src/DevOpsLab.WebApi/Properties/launchSettings.json"
CREATED=()

# --- Pre-flight: check EVERYTHING before creating any branch ----------------------
# A team whose files differ gets nothing planted, rather than one scenario out of three.
# Checked against origin/main, not the local working tree.
preflight() {
    local f
    for f in "$CSPROJ" Dockerfile "$LAUNCH"; do
        git cat-file -e "$BASE:$f" 2>/dev/null || fail "$f not found on $BASE"
    done
    git show "$BASE:$CSPROJ" | grep -q 'Microsoft.AspNetCore.Authentication.JwtBearer' \
        && fail "the team already references JwtBearer — pick another package for scenario 1"
    git show "$BASE:$CSPROJ" | grep -q '<PackageReference' \
        || fail "$CSPROJ has no PackageReference ItemGroup — edit scenario 1 by hand"
    git show "$BASE:Dockerfile" | grep -Eq '^FROM mcr\.microsoft\.com/dotnet/aspnet:8\.0-noble-chiseled(-extra)? AS runtime' \
        || fail "runtime stage is not 'aspnet:8.0-noble-chiseled[-extra] AS runtime' — edit scenario 2 by hand"
    git show "$BASE:$LAUNCH" | grep -Eq '"ASPNETCORE_ENVIRONMENT": "Development"'$'\r''?$' \
        || fail "$LAUNCH has no ASPNETCORE_ENVIRONMENT line — edit scenario 3 by hand"
    local b
    for b in pt1/s1-cve pt1/s2-root pt1/s3-secret; do
        git show-ref --verify --quiet "refs/heads/$b" && fail "branch $b already exists locally — delete it first"
    done
    if [ "$OPEN_PR" -eq 1 ]; then
        command -v gh >/dev/null 2>&1 || fail "gh is not installed — use --push and open the PRs in the browser"
    fi
    ok "pre-flight: all three scenarios can be applied"
}
preflight

# If anything below fails, return to the branch we started on.
START_BRANCH="$(git rev-parse --abbrev-ref HEAD)"
trap 'git switch --quiet "$START_BRANCH" 2>/dev/null || true' EXIT

start_branch() {
    local name="$1"
    git show-ref --verify --quiet "refs/heads/$name" && fail "branch $name already exists locally — delete it first"
    git switch --quiet -c "$name" "$BASE"
}

finish_branch() {
    local name="$1" msg="$2"
    git add -A
    # --no-verify: the instructor's clone may have the team's hooks installed;
    # the test is the SERVER-side pipeline, not the instructor's laptop.
    git commit --quiet --no-verify -m "$msg"
    printf '\n---- %s : %s\n' "$name" "$msg"
    git --no-pager show --stat --format='' HEAD
    CREATED+=("$name|$msg")
    git switch --quiet -
}

# --- Scenario 1: a CVE nobody chose ---------------------------------------------
start_branch pt1/s1-cve
[ -f "$CSPROJ" ] || fail "$CSPROJ not found"
grep -q 'Microsoft.AspNetCore.Authentication.JwtBearer' "$CSPROJ" && fail "the team already references JwtBearer — pick another package"
awk '
    /<PackageReference/ { seen = 1 }
    seen && !done && /<\/ItemGroup>/ {
        print "    <PackageReference Include=\"Microsoft.AspNetCore.Authentication.JwtBearer\" Version=\"8.0.0\" />"
        done = 1
    }
    { print }
' "$CSPROJ" > "$CSPROJ.tmp" && mv "$CSPROJ.tmp" "$CSPROJ"
grep -q 'Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0"' "$CSPROJ" \
    || fail "could not add the package reference to $CSPROJ"
ok "scenario 1 applied to $CSPROJ"
finish_branch pt1/s1-cve "feat: groundwork for JWT authentication"

# --- Scenario 2: a Dockerfile that runs as root ----------------------------------
start_branch pt1/s2-root
[ -f Dockerfile ] || fail "Dockerfile not found at the repository root"
grep -Eq '^FROM mcr\.microsoft\.com/dotnet/aspnet:8\.0-noble-chiseled(-extra)? AS runtime' Dockerfile \
    || fail "runtime stage is not 'aspnet:8.0-noble-chiseled[-extra] AS runtime' — edit by hand"
sed -E -i.bak 's#^FROM mcr\.microsoft\.com/dotnet/aspnet:8\.0-noble-chiseled(-extra)? AS runtime#FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble AS runtime#' Dockerfile
sed -E -i.bak '/^USER[[:space:]]/d' Dockerfile
rm -f Dockerfile.bak
grep -q '^FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble AS runtime' Dockerfile || fail "base image not changed"
! grep -q '^USER' Dockerfile || fail "a USER line is still present"
ok "scenario 2 applied to Dockerfile"
finish_branch pt1/s2-root "build: full Ubuntu runtime image, for on-call diagnostics"

# --- Scenario 3: a secret in a file nobody reads ----------------------------------
start_branch pt1/s3-secret
[ -f "$LAUNCH" ] || fail "$LAUNCH not found"
# Tolerates Windows line endings (a clone made with core.autocrlf=true).
grep -Eq '"ASPNETCORE_ENVIRONMENT": "Development"'$'\r''?$' "$LAUNCH" \
    || fail "$LAUNCH does not contain the expected ASPNETCORE_ENVIRONMENT line — edit by hand"
sed -E -i.bak 's#("ASPNETCORE_ENVIRONMENT": "Development")('$'\r''?)$#\1,\2\n        "ConnectionStrings__Default": "Server=db-prod.internal,1433;Database=DevOpsLab;User Id=app;Password=Q7m-Rv2026!x;Encrypt=True"\2#' "$LAUNCH"
rm -f "$LAUNCH.bak"
grep -q 'Password=Q7m-Rv2026!x' "$LAUNCH" || fail "secret not planted"
ok "scenario 3 applied to $LAUNCH"
finish_branch pt1/s3-secret "chore: local launch profile for the reporting database"

# --- Push and open pull requests ---------------------------------------------------
printf '\n'
if [ "$PUSH" -eq 0 ]; then
    printf 'DRY RUN — three local branches created, nothing pushed.\n'
    printf 'Re-run with --push (and --pr) when you are ready. To discard:\n'
    printf '  git branch -D pt1/s1-cve pt1/s2-root pt1/s3-secret\n'
    exit 0
fi

for entry in "${CREATED[@]}"; do
    branch="${entry%%|*}"
    git push --quiet -u origin "$branch" || fail "push of $branch was rejected (Push Protection? permissions?)"
    ok "pushed $branch"
done

if [ "$OPEN_PR" -eq 1 ]; then
    for entry in "${CREATED[@]}"; do
        branch="${entry%%|*}"; title="${entry#*|}"
        url="$(gh pr create --base main --head "$branch" --title "$title" \
               --body "Periodic Test 1. Do not merge. Show where the pipeline stops this, and why.")"
        ok "$url"
    done
fi

printf '\nAfter the team has presented: close the three PRs without merging, then\n'
printf '  git push origin --delete pt1/s1-cve pt1/s2-root pt1/s3-secret\n'
