// Copyright (c) Microsoft. All rights reserved.

using Azure.Core;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace MinimalApi.Services;

public class ReadRetrieveReadChatService
{
    private readonly ISearchService _searchClient;
    private readonly AzureOpenAIClient _azureOpenAIClient;
    private readonly IConfiguration _configuration;
    private readonly IComputerVisionService? _visionService;
    private readonly TokenCredential? _tokenCredential;

    public ReadRetrieveReadChatService(
        ISearchService searchClient,
        AzureOpenAIClient client,
        IConfiguration configuration,
        IComputerVisionService? visionService = null,
        TokenCredential? tokenCredential = null)
    {
        _searchClient = searchClient;
        _azureOpenAIClient = client;
        _configuration = configuration;
        _visionService = visionService;
        _tokenCredential = tokenCredential;
    }

    public async Task<ChatAppResponse> ReplyAsync(
        Shared.Models.ChatMessage[] history,
        RequestOverrides? overrides,
        CancellationToken cancellationToken = default)
    {
        var top = overrides?.Top ?? 3;
        var useSemanticCaptions = overrides?.SemanticCaptions ?? false;
        var useSemanticRanker = overrides?.SemanticRanker ?? false;
        var excludeCategory = overrides?.ExcludeCategory ?? null;
        var filter = excludeCategory is null ? null : $"category ne '{excludeCategory}'";
        
        var question = history.LastOrDefault(m => m.IsUser)?.Content is { } userQuestion
            ? userQuestion
            : throw new InvalidOperationException("User question is null");

        string[]? followUpQuestionList = null;
        float[]? embeddings = null;
        
        // Generate embeddings if needed
        if (overrides?.RetrievalMode != RetrievalMode.Text)
        {
            var embeddingDeployment = _configuration["AzureOpenAiEmbeddingDeployment"];
            if (!string.IsNullOrEmpty(embeddingDeployment))
            {
                var embeddingClient = _azureOpenAIClient.GetEmbeddingClient(embeddingDeployment);
                var embeddingResponse = await embeddingClient.GenerateEmbeddingAsync(question, cancellationToken: cancellationToken);
                embeddings = embeddingResponse.Value.ToFloats().ToArray();
            }
        }

        // step 1
        // use llm to get query if retrieval mode is not vector
        string? query = null;
        if (overrides?.RetrievalMode != RetrievalMode.Vector)
        {
            var chatDeployment = _configuration["AzureOpenAiChatGptDeployment"];
            ArgumentNullException.ThrowIfNullOrWhiteSpace(chatDeployment);
            
            var chatClient = _azureOpenAIClient.GetChatClient(chatDeployment);
            var queryMessages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage(@"You are a helpful AI assistant, generate search query for followup question.
Make your respond simple and precise. Return the query only, do not return any other text.
e.g.
Northwind Health Plus AND standard plan.
standard plan AND dental AND employee benefit."),
                new UserChatMessage(question)
            };

            var queryResponse = await chatClient.CompleteChatAsync(queryMessages, cancellationToken: cancellationToken);
            query = queryResponse.Value.Content[0].Text ?? throw new InvalidOperationException("Failed to get search query");
        }

        // step 2
        // use query to search related docs
        var documentContentList = await _searchClient.QueryDocumentsAsync(query, embeddings, overrides, cancellationToken);

        string documentContents = string.Empty;
        if (documentContentList.Length == 0)
        {
            documentContents = "no source available.";
        }
        else
        {
            documentContents = string.Join("\r", documentContentList.Select(x => $"{x.Title}:{x.Content}"));
        }

        // step 2.5
        // retrieve images if _visionService is available
        SupportingImageRecord[]? images = default;
        if (_visionService is not null)
        {
            var queryEmbeddings = await _visionService.VectorizeTextAsync(query ?? question, cancellationToken);
            images = await _searchClient.QueryImagesAsync(query, queryEmbeddings.vector, overrides, cancellationToken);
        }

        // step 3
        // put together related docs and conversation history to generate answer
        var answerMessages = new List<OpenAI.Chat.ChatMessage>
        {
            new SystemChatMessage("You are a system assistant who helps the company employees with their questions. Be brief in your answers")
        };

        // add chat history
        foreach (var message in history)
        {
            if (message.IsUser)
            {
                answerMessages.Add(new UserChatMessage(message.Content));
            }
            else
            {
                answerMessages.Add(new AssistantChatMessage(message.Content));
            }
        }

        if (images != null && images.Length > 0)
        {
            var prompt = @$"## Source ##
{documentContents}
## End ##

Answer question based on available source and images.
Your answer needs to be a json object with answer and thoughts field.
Don't put your answer between ```json and ```, return the json string directly. e.g {{""answer"": ""I don't know"", ""thoughts"": ""I don't know""}}";

            // Note: For image support, you might need to handle this differently depending on your requirements
            answerMessages.Add(new UserChatMessage(prompt));
        }
        else
        {
            var prompt = @$" ## Source ##
{documentContents}
## End ##

You answer needs to be a json object with the following format.
{{
    ""answer"": // the answer to the question, add a source reference to the end of each sentence. e.g. Apple is a fruit [reference1.pdf][reference2.pdf]. If no source available, put the answer as I don't know.
    ""thoughts"": // brief thoughts on how you came up with the answer, e.g. what sources you used, what you thought about, etc.
}}";
            answerMessages.Add(new UserChatMessage(prompt));
        }

        var chatDeploymentForAnswer = _configuration["AzureOpenAiChatGptDeployment"];
        ArgumentNullException.ThrowIfNullOrWhiteSpace(chatDeploymentForAnswer);
        
        var answerChatClient = _azureOpenAIClient.GetChatClient(chatDeploymentForAnswer);
        var answerOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 1024,
            Temperature = (float)(overrides?.Temperature ?? 0.7) // Keep as is, already in correct range
        };

        // get answer
        var answerResponse = await answerChatClient.CompleteChatAsync(answerMessages, answerOptions, cancellationToken);
        var answerJson = answerResponse.Value.Content[0].Text ?? throw new InvalidOperationException("Failed to get answer");
        
        var answerObject = JsonSerializer.Deserialize<JsonElement>(answerJson);
        var ans = answerObject.GetProperty("answer").GetString() ?? throw new InvalidOperationException("Failed to get answer");
        var thoughts = answerObject.GetProperty("thoughts").GetString() ?? throw new InvalidOperationException("Failed to get thoughts");

        // step 4
        // add follow up questions if requested
        if (overrides?.SuggestFollowupQuestions is true)
        {
            var followUpMessages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage("You are a helpful AI assistant"),
                new UserChatMessage($@"Generate three follow-up question based on the answer you just generated.
# Answer
{ans}

# Format of the response
Return the follow-up question as a json string list. Don't put your answer between ```json and ```, return the json string directly.
e.g.
[
    ""What is the deductible?"",
    ""What is the co-pay?"",
    ""What is the out-of-pocket maximum?""
]")
            };

            var followUpResponse = await answerChatClient.CompleteChatAsync(followUpMessages, answerOptions, cancellationToken);
            var followUpQuestionsJson = followUpResponse.Value.Content[0].Text ?? throw new InvalidOperationException("Failed to get follow-up questions");
            var followUpQuestionsObject = JsonSerializer.Deserialize<JsonElement>(followUpQuestionsJson);
            var followUpQuestionsList = followUpQuestionsObject.EnumerateArray().Select(x => x.GetString()!).ToList();
            foreach (var followUpQuestion in followUpQuestionsList)
            {
                ans += $" <<{followUpQuestion}>> ";
            }

            followUpQuestionList = followUpQuestionsList.ToArray();
        }

        var responseMessage = new ResponseMessage("assistant", ans);
        var responseContext = new ResponseContext(
            DataPointsContent: documentContentList.Select(x => new SupportingContentRecord(x.Title, x.Content)).ToArray(),
            DataPointsImages: images?.Select(x => new SupportingImageRecord(x.Title, x.Url)).ToArray(),
            FollowupQuestions: followUpQuestionList ?? Array.Empty<string>(),
            Thoughts: new[] { new Thoughts("Thoughts", thoughts) });

        var choice = new ResponseChoice(
            Index: 0,
            Message: responseMessage,
            Context: responseContext,
            CitationBaseUrl: _configuration.ToCitationBaseUrl());

        return new ChatAppResponse(new[] { choice });
    }
}
