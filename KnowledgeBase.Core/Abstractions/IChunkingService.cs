using KnowledgeBase.Core.Domain;

namespace KnowledgeBase.Core.Abstractions;

public interface IChunkingService
{
    IEnumerable<Chunk> ChunkDocument(Document document, int maxTokens = 500);
}
