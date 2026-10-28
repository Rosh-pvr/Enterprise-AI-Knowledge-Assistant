using KnowledgeBase.Core.Abstractions;
using KnowledgeBase.Core.Domain;
using KnowledgeBase.Core.Services;
using KnowledgeBase.Infrastructure.Abstractions;
using KnowledgeBase.Infrastructure.AI;
using KnowledgeBase.Infrastructure.VectorStore;
using Qdrant.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddAIClients(ctx.Configuration);

        var connectionString = ctx.Configuration.GetConnectionString("qdrant");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddSingleton(new QdrantClient(new Uri(connectionString)));
        }
        else
        {
            var qdrantEndpoint = ctx.Configuration["Qdrant:Endpoint"] ?? "localhost";
            var qdrantPort = int.Parse(ctx.Configuration["Qdrant:GrpcPort"] ?? "6334");
            services.AddSingleton(new QdrantClient(qdrantEndpoint, qdrantPort));
        }
        services.AddSingleton<IVectorStore, QdrantVectorStore>();
        services.AddSingleton<IChunkingService, ChunkingService>();
        services.AddHostedService<DocumentIngestionHostedService>();
    })
    .Build();

await host.RunAsync();
