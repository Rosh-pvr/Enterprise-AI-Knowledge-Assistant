# Start-Demo.ps1
# One-click launcher for Enterprise AI Knowledge Assistant
$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Starting Enterprise AI Knowledge Assistant Demo (MVP)..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Check Ollama
Write-Host "`n[1/3] Checking Ollama Service..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:11434/api/tags" -Method Get -TimeoutSec 3 -ErrorAction Stop
    Write-Host "  [✓] Ollama is running." -ForegroundColor Green
} catch {
    Write-Host "  [!] Ollama is not running. Attempting to start Ollama..." -ForegroundColor Yellow
    Start-Process "ollama" -ArgumentList "serve" -WindowStyle Hidden
    Start-Sleep -Seconds 3
}

# 2. Check models
Write-Host "`n[2/3] Checking Required AI Models..." -ForegroundColor Yellow
$models = ollama list
if ($models -notmatch "all-minilm") {
    Write-Host "  [*] Pulling all-minilm embedding model..." -ForegroundColor Yellow
    ollama pull all-minilm
}
if ($models -notmatch "llama3.2") {
    Write-Host "  [*] Pulling llama3.2 LLM..." -ForegroundColor Yellow
    ollama pull llama3.2
}
Write-Host "  [✓] AI Models (llama3.2 + all-minilm) ready." -ForegroundColor Green

# 3. Launch Blazor Application
Write-Host "`n[3/3] Launching Web UI on http://localhost:5157 ..." -ForegroundColor Yellow

# Open browser after a brief delay
Start-Job -ScriptBlock {
    Start-Sleep -Seconds 4
    Start-Process "http://localhost:5157"
} | Out-Null

Write-Host "`n>>> Demo is live! Opening browser at http://localhost:5157 ..." -ForegroundColor Green
Write-Host ">>> Press Ctrl+C in this terminal when finished.`n" -ForegroundColor Gray

dotnet run --project NETAI.csproj --urls "http://localhost:5157"
