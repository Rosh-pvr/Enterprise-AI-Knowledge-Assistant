using KnowledgeBase.Core.Abstractions;
using KnowledgeBase.Core.Domain;
using KnowledgeBase.Infrastructure.Abstractions;
using NSubstitute;
using Xunit;

namespace KnowledgeBase.Tests.Unit;

public class RagServiceTests
{
    [Fact]
    public async Task ChatEndpoint_BuildsPromptWithRelevantChunks()
    {
        // Arrange
        var embed = Substitute.For<IEmbeddingGeneratorService>();
        embed.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new float[384]));

        var chat = Substitute.For<IChatClientService>();
        chat.GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("Paris is the capital of France."));

        var store = Substitute.For<IVectorStore>();
        var fakeChunks = new List<Chunk>
        {
            new(Guid.NewGuid(), 0, "Paris is the capital of France."),
            new(Guid.NewGuid(), 1, "France is a country in Europe.")
        };
        store.SearchAsync(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Chunk>>(fakeChunks));

        // Act – simulate the API endpoint logic
        var query = "What is the capital of France?";
        var queryEmbedding = await embed.GenerateEmbeddingAsync(query);
        var relevant = await store.SearchAsync(queryEmbedding, limit: 5);
        var context = string.Join("\n---\n", relevant.Select(c => c.Text));
        var prompt = $"Context:\n{context}\n\nUser Question: {query}";
        var answer = await chat.GetResponseAsync(prompt);

        // Assert
        Assert.Equal("Paris is the capital of France.", answer);
        await chat.Received(1).GetResponseAsync(
            Arg.Is<string>(p => p.Contains("Paris is the capital") && p.Contains("What is the capital of France?")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChatEndpoint_NoResults_ReturnsEmptyContext()
    {
        var embed = Substitute.For<IEmbeddingGeneratorService>();
        embed.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new float[384]));

        var chat = Substitute.For<IChatClientService>();
        chat.GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("I don't have enough information."));

        var store = Substitute.For<IVectorStore>();
        store.SearchAsync(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Chunk>>(new List<Chunk>()));

        // Act
        var queryEmbedding = await embed.GenerateEmbeddingAsync("Unknown topic");
        var relevant = await store.SearchAsync(queryEmbedding, limit: 5);
        var context = string.Join("\n---\n", relevant.Select(c => c.Text));
        var prompt = $"Context:\n{context}\n\nUser Question: Unknown topic";
        var answer = await chat.GetResponseAsync(prompt);

        // Assert
        Assert.Equal("I don't have enough information.", answer);
        Assert.Empty(relevant);
    }
}
