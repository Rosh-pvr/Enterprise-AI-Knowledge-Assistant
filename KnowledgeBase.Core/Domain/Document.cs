using System;

namespace KnowledgeBase.Core.Domain;

public record Document(Guid Id, string Title, string Content, string? TenantId = null);