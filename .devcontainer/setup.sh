#!/usr/bin/env bash
set -e

echo "=== [1/4] Installing Ollama ==="
if ! command -v ollama &> /dev/null; then
    curl -fsSL https://ollama.com/install.sh | sh
fi

echo "=== [2/4] Starting Ollama background service ==="
ollama serve &
sleep 5

echo "=== [3/4] Pulling lightweight models for cloud VM inference ==="
# Pull all-minilm embedding model (45MB)
ollama pull all-minilm

# In cloud VMs without dedicated GPUs, pull llama3.2:1b or llama3.2 for fast CPU responses
ollama pull llama3.2:1b || true
ollama pull llama3.2 || true

echo "=== [4/4] Restoring & Building .NET 9 solution ==="
dotnet restore NETAI.sln
dotnet build NETAI.sln -c Release

echo "=========================================================="
echo " Setup complete! Run './start-demo.sh' to launch the demo."
echo "=========================================================="
