using KnowledgeBase.Core.Abstractions;
using KnowledgeBase.Core.Domain;
using KnowledgeBase.Infrastructure.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

public class DocumentIngestionHostedService : BackgroundService
{
    private readonly IChunkingService _chunker;
    private readonly IEmbeddingGeneratorService _embedGen;
    private readonly IVectorStore _store;
    private readonly ILogger<DocumentIngestionHostedService> _logger;

    // Other services can write Document objects into this channel
    public static readonly Channel<Document> DocumentChannel = Channel.CreateUnbounded<Document>();

    public DocumentIngestionHostedService(
        IChunkingService chunker,
        IEmbeddingGeneratorService embedGen,
        IVectorStore store,
        ILogger<DocumentIngestionHostedService> logger)
    {
        _chunker = chunker;
        _embedGen = embedGen;
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document ingestion worker started. Awaiting documents...");

        await foreach (var doc in DocumentChannel.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Processing document: {Title} ({Id})", doc.Title, doc.Id);
            var chunks = _chunker.ChunkDocument(doc);
            foreach (var chunk in chunks)
            {
                var emb = await _embedGen.GenerateEmbeddingAsync(chunk.Text, stoppingToken);
                var enriched = chunk with { Embedding = emb };
                await _store.IndexChunkAsync(enriched, stoppingToken);
            }
            _logger.LogInformation("Finished processing document: {Title}", doc.Title);
        }
    }
}
