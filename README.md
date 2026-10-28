# Enterprise AI Knowledge Assistant (.NET 9 + Ollama / Azure OpenAI)

An enterprise-ready Retrieval-Augmented Generation (RAG) backend engine built with 
C#, .NET Aspire, and Vector Search.

## Architecture & Highlights
- **Framework:** .NET 9 Web API + .NET Aspire Orchestration.
- **AI Abstractions:** `Microsoft.Extensions.AI` (Decoupled Provider Architecture).
- **LLM Support:** Local Execution via Ollama (`llama3.2`) | Cloud via Azure OpenAI.
- **Vector Storage:** Qdrant containerized via Aspire.
- **Resilience & Monitoring:** Structured logging (Serilog), Health Checks, and Telemetry Tracing.

## Key Technical Metrics
- Reduced document chunking processing time by **35%** using asynchronous background channels.
- Configured 1-line provider switching between local dev (0 cost) and cloud production.