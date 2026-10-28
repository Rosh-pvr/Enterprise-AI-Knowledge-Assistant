var builder = DistributedApplication.CreateBuilder(args);

// Qdrant vector database container
var qdrant = builder.AddQdrant("qdrant");

// API project
var api = builder.AddProject<Projects.KnowledgeBase_Api>("knowledgebase-api")
    .WithReference(qdrant);

// Background worker
var worker = builder.AddProject<Projects.KnowledgeBase_Worker>("knowledgebase-worker")
    .WithReference(qdrant);

builder.Build().Run();
