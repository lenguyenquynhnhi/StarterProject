<#
.SYNOPSIS
  One-command environment setup for the DevOpsLab starter project (Windows / PowerShell).
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts/setup.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$api   = 'src/DevOpsLab.WebApi'
$infra = 'src/DevOpsLab.Infrastructure'

function Step($m) { Write-Host "`n==> $m" -ForegroundColor Cyan }
function Ok($m)   { Write-Host "  OK   $m" -ForegroundColor Green }
function Die($m)  { Write-Host "  FAIL $m" -ForegroundColor Red; exit 1 }

Step '1/7  Checking prerequisites'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { Die '.NET SDK not found. Install .NET 8 SDK and re-run.' }
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { Die 'Docker not found. Install Docker Desktop and re-run.' }
docker info > $null 2>&1
if ($LASTEXITCODE -ne 0) { Die 'Docker is installed but not running. Start Docker Desktop and re-run.' }

$sdk = (dotnet --version).Trim()
if (-not $sdk.StartsWith('8.')) { Die ".NET 8 SDK required, found $sdk" }
Ok ".NET SDK $sdk"
Ok 'Docker is running'

Step '2/7  Preparing local configuration'
if (-not (Test-Path '.env')) { Copy-Item '.env.example' '.env'; Ok 'Created .env from .env.example' }
else { Ok '.env already exists, leaving it untouched' }

Step '3/7  Starting SQL Server'
docker compose up -d
if ($LASTEXITCODE -ne 0) { Die 'docker compose up failed.' }

Write-Host '  waiting for the database to report healthy' -NoNewline
$status = 'starting'
for ($i = 0; $i -lt 60; $i++) {
    $status = (docker inspect --format '{{.State.Health.Status}}' devopslab-sqlserver 2>$null)
    if ($status -eq 'healthy') { break }
    Write-Host '.' -NoNewline
    Start-Sleep -Seconds 3
}
Write-Host ''
if ($status -ne 'healthy') { Die 'SQL Server did not become healthy in time. Check: docker compose logs sqlserver' }
Ok 'SQL Server is healthy'

Step '4/7  Restoring and building the solution'
dotnet restore DevOpsLab.sln
if ($LASTEXITCODE -ne 0) { Die 'Restore failed.' }
dotnet build DevOpsLab.sln -c Debug --no-restore
if ($LASTEXITCODE -ne 0) { Die 'Build failed.' }
Ok 'Build succeeded'

Step '5/7  Preparing database migrations'
dotnet ef --version *> $null
if ($LASTEXITCODE -ne 0) {
    dotnet tool install --global dotnet-ef *> $null
    $env:PATH = "$env:PATH;$env:USERPROFILE\.dotnet\tools"
}
if (-not (Test-Path "$infra/Migrations")) {
    dotnet ef migrations add InitialCreate --project $infra --startup-project $api
    if ($LASTEXITCODE -ne 0) { Die 'Creating the initial migration failed.' }
    Ok 'Created the InitialCreate migration'
} else { Ok 'Migrations folder already present' }

dotnet ef database update --project $infra --startup-project $api
if ($LASTEXITCODE -ne 0) { Die 'Applying migrations failed.' }
Ok 'Database schema is up to date'

Step '6/7  Running the test suite'
dotnet test DevOpsLab.sln -c Debug --no-build
if ($LASTEXITCODE -ne 0) { Die 'Tests failed.' }
Ok 'All tests passed'

Step '7/7  Done'
@'

  Start the API with:   dotnet run --project src/DevOpsLab.WebApi

  Then check:
    http://localhost:5080/healthz/live    liveness  - is the process alive?
    http://localhost:5080/healthz/ready   readiness - can it actually serve traffic?
    http://localhost:5080/swagger         API explorer

  Reminder: a process that is alive is NOT necessarily ready. You will rely on
  that difference in Session 11 when you switch traffic with zero downtime.

'@ | Write-Host
