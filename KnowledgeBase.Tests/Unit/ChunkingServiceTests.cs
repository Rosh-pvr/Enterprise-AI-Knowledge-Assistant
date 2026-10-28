using KnowledgeBase.Core.Domain;
using KnowledgeBase.Core.Services;
using Xunit;

namespace KnowledgeBase.Tests.Unit;

public class ChunkingServiceTests
{
    private readonly ChunkingService _svc = new();

    [Fact]
    public void EmptyDocument_ReturnsNoChunks()
    {
        var doc = new Document(Guid.NewGuid(), "Empty", string.Empty);
        var result = _svc.ChunkDocument(doc).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void WhitespaceOnlyDocument_ReturnsNoChunks()
    {
        var doc = new Document(Guid.NewGuid(), "Whitespace", "   \n\n   \t  ");
        var result = _svc.ChunkDocument(doc).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void SingleParagraph_ReturnsSingleChunk()
    {
        var doc = new Document(Guid.NewGuid(), "Small", "Hello world, this is a test.");
        var chunks = _svc.ChunkDocument(doc, maxTokens: 500).ToList();
        Assert.Single(chunks);
        Assert.Equal("Hello world, this is a test.", chunks[0].Text);
    }

    [Fact]
    public void LargeDocument_IsChunkedRespectingMaxTokens()
    {
        // 2500 chars > 2000 chars (500 tokens * 4 chars/token)
        var content = string.Join("\n\n", Enumerable.Range(0, 50).Select(i => new string((char)('a' + i % 26), 100)));
        var doc = new Document(Guid.NewGuid(), "Large", content);
        var chunks = _svc.ChunkDocument(doc, maxTokens: 500).ToList();

        Assert.True(chunks.Count >= 2, $"Expected >=2 chunks, got {chunks.Count}");
        // Verify every chunk has reasonable length
        foreach (var c in chunks)
        {
            Assert.True(c.Text.Length > 0, "Chunk should not be empty");
            Assert.True(c.Text.Length <= 2200, $"Chunk too large: {c.Text.Length} chars");
        }
    }

    [Fact]
    public void ChunkIndex_IsSequential()
    {
        var content = string.Join("\n\n", Enumerable.Range(0, 20).Select(i => $"Paragraph {i}: " + new string('x', 300)));
        var doc = new Document(Guid.NewGuid(), "Indexed", content);
        var chunks = _svc.ChunkDocument(doc, maxTokens: 200).ToList();

        for (int i = 0; i < chunks.Count; i++)
        {
            Assert.Equal(i, chunks[i].Index);
            Assert.Equal(doc.Id, chunks[i].DocumentId);
        }
    }
}
