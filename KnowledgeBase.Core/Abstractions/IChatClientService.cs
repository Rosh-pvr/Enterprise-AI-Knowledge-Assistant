using Microsoft.Extensions.AI;

namespace KnowledgeBase.Core.Abstractions;

public interface IChatClientService
{
    Task<string> GetResponseAsync(string prompt, System.Threading.CancellationToken ct = default);
}
