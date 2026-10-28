#!/usr/bin/env bash
set -e

echo "=========================================================="
echo " Starting Enterprise AI Knowledge Assistant Demo...     "
echo "=========================================================="

# Check if Ollama is running
if ! curl -s http://localhost:11434/api/tags > /dev/null 2>&1; then
    echo "[!] Ollama is not running. Starting Ollama in the background..."
    ollama serve > /dev/null 2>&1 &
    sleep 3
fi

echo "[✓] Ollama is active."

# Ensure required models are pulled
echo "[*] Checking models in Ollama..."
ollama pull all-minilm > /dev/null 2>&1 || true

# Check if llama3.2 exists, otherwise pull llama3.2:1b or llama3.2
if ! ollama list | grep -q "llama3.2"; then
    echo "[*] Pulling llama3.2..."
    ollama pull llama3.2 || ollama pull llama3.2:1b
fi

echo "[✓] Models ready."

echo "----------------------------------------------------------"
echo " Launching Interactive Blazor Assistant on http://localhost:5157"
echo " Press Ctrl+C to stop."
echo "----------------------------------------------------------"

dotnet run --project NETAI.csproj --urls "http://0.0.0.0:5157"
