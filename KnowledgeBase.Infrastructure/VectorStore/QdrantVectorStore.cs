using KnowledgeBase.Core.Domain;
using KnowledgeBase.Infrastructure.Abstractions;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Collections.Generic;
using System.Linq;

namespace KnowledgeBase.Infrastructure.VectorStore;

public class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;
    private const string CollectionName = "rag_documents";
    private const ulong DefaultVectorSize = 768; // nomic-embed-text embedding dimension
    private bool _collectionEnsured = false;

    public QdrantVectorStore(QdrantClient client)
    {
        _client = client;
    }

    private async Task EnsureCollectionAsync(ulong vectorSize, CancellationToken ct = default)
    {
        if (_collectionEnsured) return;
        try
        {
            var collections = await _client.ListCollectionsAsync(cancellationToken: ct);
            if (!collections.Any(c => c == CollectionName))
            {
                await _client.CreateCollectionAsync(CollectionName, new VectorParams
                {
                    Size = vectorSize,
                    Distance = Distance.Cosine
                }, cancellationToken: ct);
            }
            _collectionEnsured = true;
        }
        catch
        {
            // If Qdrant is momentarily unreachable during startup, retry on next call
        }
    }

    public async Task IndexChunkAsync(Chunk chunk, CancellationToken ct = default)
    {
        if (chunk.Embedding is null)
            throw new InvalidOperationException("Chunk embedding is missing.");

        await EnsureCollectionAsync((ulong)chunk.Embedding.Length, ct);

        var pointId = Guid.NewGuid();
        var payload = new Dictionary<string, Value>
        {
            ["document_id"] = new Value { StringValue = chunk.DocumentId.ToString() },
            ["index"] = new Value { IntegerValue = chunk.Index },
            ["text"] = new Value { StringValue = chunk.Text }
        };

        var point = new PointStruct
        {
            Id = new PointId { Uuid = pointId.ToString() },
            Vectors = chunk.Embedding,
            Payload = { payload }
        };

        await _client.UpsertAsync(CollectionName, new List<PointStruct> { point }, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<Chunk>> SearchAsync(float[] embedding, int limit = 5, CancellationToken ct = default)
    {
        await EnsureCollectionAsync((ulong)embedding.Length, ct);
        var results = await _client.SearchAsync(CollectionName, embedding, limit: (ulong)limit, cancellationToken: ct);
        return results.Select(r => new Chunk(
            Guid.Parse(r.Payload["document_id"].StringValue),
            (int)r.Payload["index"].IntegerValue,
            r.Payload["text"].StringValue
        )).ToList();
    }
}
