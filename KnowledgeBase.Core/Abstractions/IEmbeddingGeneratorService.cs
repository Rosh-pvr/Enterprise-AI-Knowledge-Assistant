namespace KnowledgeBase.Core.Abstractions;

using System.Threading;
using System.Threading.Tasks;

public interface IEmbeddingGeneratorService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
}
