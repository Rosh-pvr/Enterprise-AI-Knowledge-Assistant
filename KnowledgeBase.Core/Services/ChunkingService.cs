using KnowledgeBase.Core.Abstractions;
using KnowledgeBase.Core.Domain;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KnowledgeBase.Core.Services;

public class ChunkingService : IChunkingService
{
    private const int ApproxCharsPerToken = 4; // rough estimate

    public IEnumerable<Chunk> ChunkDocument(Document document, int maxTokens = 500)
    {
        if (string.IsNullOrWhiteSpace(document.Content))
            yield break;

        var maxChars = maxTokens * ApproxCharsPerToken;
        var paragraphs = Regex.Split(document.Content, "\r?\n\r?\n")
            .Where(p => !string.IsNullOrWhiteSpace(p));

        var buffer = new List<string>();
        var currentLen = 0;
        var index = 0;

        foreach (var para in paragraphs)
        {
            var paraLen = para.Length;
            if (currentLen + paraLen > maxChars && buffer.Count > 0)
            {
                var chunkText = string.Join("\n\n", buffer);
                yield return new Chunk(document.Id, index++, chunkText);
                buffer.Clear();
                currentLen = 0;
            }
            buffer.Add(para);
            currentLen += paraLen;
        }

        if (buffer.Count > 0)
        {
            var chunkText = string.Join("\n\n", buffer);
            yield return new Chunk(document.Id, index, chunkText);
        }
    }
}
