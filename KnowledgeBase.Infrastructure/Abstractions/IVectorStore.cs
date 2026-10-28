using KnowledgeBase.Core.Domain;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KnowledgeBase.Infrastructure.Abstractions;

public interface IVectorStore
{
    Task IndexChunkAsync(Chunk chunk, CancellationToken ct = default);
    Task<IReadOnlyList<Chunk>> SearchAsync(float[] embedding, int limit = 5, CancellationToken ct = default);
}
