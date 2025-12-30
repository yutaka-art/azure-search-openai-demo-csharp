// Copyright (c) Microsoft. All rights reserved.

using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;

namespace MinimalApi.Extensions;

internal static class ServiceCollectionExtensions
{
    private static readonly DefaultAzureCredential s_azureCredential = new();

    internal static IServiceCollection AddAzureServices(this IServiceCollection services)
    {
        services.AddSingleton<BlobServiceClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureStorageAccountEndpoint = config["AzureStorageAccountEndpoint"];
            ArgumentNullException.ThrowIfNullOrEmpty(azureStorageAccountEndpoint);

            var blobServiceClient = new BlobServiceClient(
                new Uri(azureStorageAccountEndpoint), s_azureCredential);

            return blobServiceClient;
        });

        services.AddSingleton<BlobContainerClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureStorageContainer = config["AzureStorageContainer"];
            return sp.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(azureStorageContainer);
        });

        services.AddSingleton<ISearchService, AzureSearchService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureSearchServiceEndpoint = config["AzureSearchServiceEndpoint"];
            ArgumentNullException.ThrowIfNullOrEmpty(azureSearchServiceEndpoint);

            var azureSearchIndex = config["AzureSearchIndex"];
            ArgumentNullException.ThrowIfNullOrEmpty(azureSearchIndex);

            var searchClient = new SearchClient(
                               new Uri(azureSearchServiceEndpoint), azureSearchIndex, s_azureCredential);

            return new AzureSearchService(searchClient);
        });

        services.AddSingleton<DocumentAnalysisClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureOpenAiServiceEndpoint = config["AzureOpenAiServiceEndpoint"] ?? throw new ArgumentNullException();

            var documentAnalysisClient = new DocumentAnalysisClient(
                new Uri(azureOpenAiServiceEndpoint), s_azureCredential);
            return documentAnalysisClient;
        });

        services.AddSingleton<OpenAIClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var useAOAI = config["UseAOAI"] == "true";
            if (useAOAI)
            {
                // For Azure OpenAI, we don't need the OpenAIClient from OpenAI library
                // We'll use AzureOpenAIClient instead
                throw new InvalidOperationException("Use AzureOpenAIClient for Azure OpenAI services");
            }
            else
            {
                var openAIApiKey = config["OpenAIApiKey"];
                ArgumentNullException.ThrowIfNullOrEmpty(openAIApiKey);

                var openAIClient = new OpenAI.OpenAIClient(openAIApiKey);
                return openAIClient;
            }
        });

        services.AddSingleton<AzureOpenAIClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var useAOAI = config["UseAOAI"] == "true";
            if (useAOAI)
            {
                var azureOpenAiServiceEndpoint = config["AzureOpenAiServiceEndpoint"];
                ArgumentNullException.ThrowIfNullOrEmpty(azureOpenAiServiceEndpoint);

                var azureOpenAIClient = new AzureOpenAIClient(new Uri(azureOpenAiServiceEndpoint), s_azureCredential);

                return azureOpenAIClient;
            }
            else
            {
                // For non-Azure OpenAI, we need to use OpenAIClient wrapped in AzureOpenAIClient
                // But AzureOpenAIClient doesn't support OpenAI directly, so we'll need to handle this differently
                throw new InvalidOperationException("AzureOpenAIClient doesn't support OpenAI API key directly. Use OpenAIClient instead.");
            }
        });

        services.AddSingleton<AzureBlobStorageService>();
        services.AddSingleton<ReadRetrieveReadChatService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var useVision = config["UseVision"] == "true";
            var azureOpenAIClient = sp.GetRequiredService<AzureOpenAIClient>();
            var searchClient = sp.GetRequiredService<ISearchService>();
            if (useVision)
            {
                var azureComputerVisionServiceEndpoint = config["AzureComputerVisionServiceEndpoint"];
                ArgumentNullException.ThrowIfNullOrEmpty(azureComputerVisionServiceEndpoint);
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
                
                var visionService = new AzureComputerVisionService(httpClient, azureComputerVisionServiceEndpoint, s_azureCredential);
                return new ReadRetrieveReadChatService(searchClient, azureOpenAIClient, config, visionService, s_azureCredential);
            }
            else
            {
                return new ReadRetrieveReadChatService(searchClient, azureOpenAIClient, config, tokenCredential: s_azureCredential);
            }
        });

        return services;
    }

    internal static IServiceCollection AddCrossOriginResourceSharing(this IServiceCollection services)
    {
        services.AddCors(
            options =>
                options.AddDefaultPolicy(
                    policy =>
                        policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()));

        return services;
    }
}
