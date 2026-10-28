using System;

namespace KnowledgeBase.Core.Domain;

public record Chunk(Guid DocumentId, int Index, string Text, float[]? Embedding = null);