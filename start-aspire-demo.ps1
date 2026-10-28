# start-aspire-demo.ps1
# One-click launcher for the .NET Aspire Orchestrated Stack (API + Worker + Qdrant + Dashboard)
$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Starting .NET Aspire Orchestration Stack..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Check Docker
Write-Host "`n[1/2] Checking Docker Daemon (needed for Qdrant)..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "  [✓] Docker is running." -ForegroundColor Green
} catch {
    Write-Host "  [!] Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

Write-Host "`n[2/2] Launching Aspire AppHost (Dashboard, Qdrant, API, Worker)..." -ForegroundColor Yellow
dotnet run --project Aspire.AppHost\Aspire.AppHost.csproj --launch-profile http
