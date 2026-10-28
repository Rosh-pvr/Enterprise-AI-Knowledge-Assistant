#!/usr/bin/env bash
set -e

echo "=========================================================="
echo " Starting Enterprise AI Knowledge Assistant Demo...     "
echo "=========================================================="

# 1. Install Ollama if missing
if ! command -v ollama &> /dev/null; then
    echo "[!] Ollama not found. Installing Ollama..."
    curl -fsSL https://ollama.com/install.sh | sh
fi

# 2. Start Ollama in background if not running
if ! curl -s http://localhost:11434/api/tags > /dev/null 2>&1; then
    echo "[*] Starting Ollama background service..."
    nohup ollama serve > /tmp/ollama.log 2>&1 &
    sleep 4
fi

echo "[✓] Ollama service is active."

# 3. Ensure required models are pulled
echo "[*] Checking AI models in Ollama..."
ollama pull all-minilm > /dev/null 2>&1 || true

if ! ollama list | grep -q "llama3.2"; then
    echo "[*] Pulling llama3.2 LLM (or lightweight 1B model)..."
    ollama pull llama3.2 || ollama pull llama3.2:1b
fi

echo "[✓] AI Models (all-minilm + llama3.2) ready."

# 4. Launch Blazor app on 0.0.0.0:5157
echo "----------------------------------------------------------"
echo " Launching Interactive Blazor Assistant on http://0.0.0.0:5157"
echo " (Forwarded to port 5157 in Codespaces)"
echo " Press Ctrl+C to stop."
echo "----------------------------------------------------------"

dotnet run --project NETAI.csproj --urls "http://0.0.0.0:5157"
