using KnowledgeBase.Core.Abstractions;
using KnowledgeBase.Core.Domain;
using KnowledgeBase.Core.Services;
using KnowledgeBase.Infrastructure.Abstractions;
using KnowledgeBase.Infrastructure.AI;
using KnowledgeBase.Infrastructure.VectorStore;
using Qdrant.Client;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// ServiceDefaults: health checks, resilience, telemetry
builder.Services.AddServiceDefaults();

// Anti-forgery services
builder.Services.AddAntiforgery();

// AI clients (provider-agnostic, configured in appsettings)
builder.Services.AddAIClients(builder.Configuration);

// Qdrant vector DB: works seamlessly with both Aspire and Standalone
var connectionString = builder.Configuration.GetConnectionString("qdrant");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddSingleton(new QdrantClient(new Uri(connectionString)));
}
else
{
    var qdrantEndpoint = builder.Configuration["Qdrant:Endpoint"] ?? "localhost";
    var qdrantPort = int.Parse(builder.Configuration["Qdrant:GrpcPort"] ?? "6334");
    builder.Services.AddSingleton(new QdrantClient(qdrantEndpoint, qdrantPort));
}
builder.Services.AddSingleton<IVectorStore, QdrantVectorStore>();

// Domain services
builder.Services.AddSingleton<IChunkingService, ChunkingService>();

var app = builder.Build();

app.UseAntiforgery();

app.MapHealthChecks("/health");

// ──────────── Document Upload ────────────
app.MapPost("/documents", async (IFormFile file, IChunkingService chunker, IEmbeddingGeneratorService embed, IVectorStore store) =>
{
    if (file.Length == 0) return Results.BadRequest("Empty file");
    using var sr = new StreamReader(file.OpenReadStream());
    var content = await sr.ReadToEndAsync();
    var doc = new Document(Guid.NewGuid(), file.FileName, content);
    foreach (var chunk in chunker.ChunkDocument(doc))
    {
        var emb = await embed.GenerateEmbeddingAsync(chunk.Text);
        var enriched = chunk with { Embedding = emb };
        await store.IndexChunkAsync(enriched);
    }
    return Results.Ok(new { doc.Id, file.FileName });
}).DisableAntiforgery();

// ──────────── RAG Chat ────────────
app.MapPost("/chat", async (ChatRequest request, IEmbeddingGeneratorService embed, IVectorStore store, IChatClientService chat) =>
{
    var queryEmb = await embed.GenerateEmbeddingAsync(request.Query);
    var relevant = await store.SearchAsync(queryEmb, limit: 5);
    var context = string.Join("\n---\n", relevant.Select(c => c.Text));
    var prompt = $"""
        You are a helpful assistant. Use the following context to answer the user's question.
        If the context does not contain the answer, say "I don't have enough information."

        Context:
        {context}

        User Question: {request.Query}
        """;
    var answer = await chat.GetResponseAsync(prompt);
    return Results.Ok(new { answer, sources = relevant.Select(c => new { c.DocumentId, c.Index }) });
});

app.Run();

public record ChatRequest(string Query);

public partial class Program { }
