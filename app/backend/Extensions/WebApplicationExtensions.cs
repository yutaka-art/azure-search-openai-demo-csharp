// Copyright (c) Microsoft. All rights reserved.

using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace MinimalApi.Extensions;

internal static class WebApplicationExtensions
{
    internal static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup("api");

        // Blazor 📎 Clippy streaming endpoint
        api.MapPost("openai/chat", OnPostChatPromptAsync);

        // Long-form chat w/ contextual history endpoint
        api.MapPost("chat", OnPostChatAsync);

        // Upload a document
        api.MapPost("documents", OnPostDocumentAsync);

        // Get all documents
        api.MapGet("documents", OnGetDocumentsAsync);

        // Get DALL-E image result from prompt
        //api.MapPost("images", OnPostImagePromptAsync);

        api.MapGet("enableLogout", OnGetEnableLogout);

        return app;
    }

    private static IResult OnGetEnableLogout(HttpContext context)
    {
        var header = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"];
        var enableLogout = !string.IsNullOrEmpty(header);

        return TypedResults.Ok(enableLogout);
    }

    private static async IAsyncEnumerable<ChatChunkResponse> OnPostChatPromptAsync(
        PromptRequest prompt,
        AzureOpenAIClient client,
        IConfiguration config,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var deploymentId = config["AZURE_OPENAI_CHATGPT_DEPLOYMENT"];
        var chatClient = client.GetChatClient(deploymentId);
        
        var messages = new List<OpenAI.Chat.ChatMessage>
        {
            new SystemChatMessage("""
                You're an AI assistant for developers, helping them write code more efficiently.
                You're name is **Blazor 📎 Clippy** and you're an expert Blazor developer.
                You're also an expert in ASP.NET Core, C#, TypeScript, and even JavaScript.
                You will always reply with a Markdown formatted response.
                """),
            new UserChatMessage("What's your name?"),
            new AssistantChatMessage("Hi, my name is **Blazor 📎 Clippy**! Nice to meet you."),
            new UserChatMessage(prompt.Prompt)
        };

        var response = chatClient.CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken);

        await foreach (var chatUpdate in response.WithCancellation(cancellationToken))
        {
            if (chatUpdate.ContentUpdate.Count > 0)
            {
                foreach (var contentPart in chatUpdate.ContentUpdate)
                {
                    if (contentPart.Text?.Length > 0)
                    {
                        yield return new ChatChunkResponse(contentPart.Text.Length, contentPart.Text);
                    }
                }
            }
        }
    }

    private static async Task<IResult> OnPostChatAsync(
        ChatRequest request,
        ReadRetrieveReadChatService chatService,
        CancellationToken cancellationToken)
    {
        if (request is { History.Length: > 0 })
        {
            var response = await chatService.ReplyAsync(
                request.History, request.Overrides, cancellationToken);

            return TypedResults.Ok(response);
        }

        return Results.BadRequest();
    }

    private static async Task<IResult> OnPostDocumentAsync(
        [FromForm] IFormFileCollection files,
        [FromServices] AzureBlobStorageService service,
        [FromServices] ILogger<AzureBlobStorageService> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Upload documents");

        var response = await service.UploadFilesAsync(files, cancellationToken);

        logger.LogInformation("Upload documents: {x}", response);

        return TypedResults.Ok(response);
    }

    private static async IAsyncEnumerable<DocumentResponse> OnGetDocumentsAsync(
        BlobContainerClient client,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var blob in client.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            if (blob is not null and { Deleted: false })
            {
                var props = blob.Properties;
                var baseUri = client.Uri;
                var builder = new UriBuilder(baseUri);
                builder.Path += $"/{blob.Name}";

                var metadata = blob.Metadata;
                var documentProcessingStatus = GetMetadataEnumOrDefault<DocumentProcessingStatus>(
                    metadata, nameof(DocumentProcessingStatus), DocumentProcessingStatus.NotProcessed);
                var embeddingType = GetMetadataEnumOrDefault<EmbeddingType>(
                    metadata, nameof(EmbeddingType), EmbeddingType.AzureSearch);

                yield return new(
                    blob.Name,
                    props.ContentType,
                    props.ContentLength ?? 0,
                    props.LastModified,
                    builder.Uri,
                    documentProcessingStatus,
                    embeddingType);

                static TEnum GetMetadataEnumOrDefault<TEnum>(
                    IDictionary<string, string> metadata,
                    string key,
                    TEnum @default) where TEnum : struct => metadata.TryGetValue(key, out var value)
                        && Enum.TryParse<TEnum>(value, out var status)
                            ? status
                            : @default;
            }
        }
    }

    private static async Task<IResult> OnPostImagePromptAsync(
        PromptRequest prompt,
        OpenAI.OpenAIClient client,
        IConfiguration config,
        CancellationToken cancellationToken)
    {
        var imageClient = client.GetImageClient("dall-e-3");
        var result = await imageClient.GenerateImageAsync(prompt.Prompt, new OpenAI.Images.ImageGenerationOptions
        {
            Size = OpenAI.Images.GeneratedImageSize.W1024xH1024,
            Quality = OpenAI.Images.GeneratedImageQuality.Standard
        }, cancellationToken);

        var imageUrls = new List<Uri> { result.Value.ImageUri };
        var response = new ImageResponse(DateTimeOffset.UtcNow, imageUrls);

        return TypedResults.Ok(response);
    }
}
