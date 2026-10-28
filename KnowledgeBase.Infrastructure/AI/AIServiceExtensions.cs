using KnowledgeBase.Core.Abstractions;
using KnowledgeBase.Core.Domain;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Infrastructure.AI;

public static class AIServiceExtensions
{
    public static IServiceCollection AddAIClients(this IServiceCollection services, IConfiguration config)
    {
        var ai = config.GetSection("AI");
        var provider = ai.GetValue<string>("Provider") ?? "Ollama";

        var ollamaEndpoint = ai["Ollama:Endpoint"] ?? "http://localhost:11434";
        var chatModel = ai["Ollama:ChatModel"] ?? "llama3.2";
        var embeddingModel = ai["Ollama:EmbeddingModel"] ?? "nomic-embed-text";

        // Register IChatClient using OllamaSharp with 5-minute timeout for local CPU inference
        services.AddSingleton<IChatClient>(sp =>
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(ollamaEndpoint), Timeout = TimeSpan.FromMinutes(5) };
            return new OllamaSharp.OllamaApiClient(httpClient, chatModel);
        });

        // Register IEmbeddingGenerator using OllamaSharp with 5-minute timeout
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(ollamaEndpoint), Timeout = TimeSpan.FromMinutes(5) };
            return new OllamaSharp.OllamaApiClient(httpClient, embeddingModel);
        });

        // Adapter wrappers that hide the generic type args
        services.AddSingleton<IEmbeddingGeneratorService, EmbeddingAdapter>();
        services.AddSingleton<IChatClientService, ChatAdapter>();

        return services;
    }

    private sealed class EmbeddingAdapter : IEmbeddingGeneratorService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _inner;
        public EmbeddingAdapter(IEmbeddingGenerator<string, Embedding<float>> inner) => _inner = inner;

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
        {
            var result = await _inner.GenerateAsync([text], cancellationToken: ct);
            return result[0].Vector.ToArray();
        }
    }

    private sealed class ChatAdapter : IChatClientService
    {
        private readonly IChatClient _inner;
        public ChatAdapter(IChatClient inner) => _inner = inner;

        public async Task<string> GetResponseAsync(string prompt, CancellationToken ct = default)
        {
            var msgs = new List<ChatMessage> { new(ChatRole.User, prompt) };
            var resp = await _inner.GetResponseAsync(msgs, cancellationToken: ct);
            return resp?.Text ?? string.Empty;
        }
    }
}
