using OpenAI.Chat;
using System.Runtime.CompilerServices;
using System.Text;
using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Conversation;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Tool;
using UnrealAgent.Backend.Tool.Tools;
using Block = UnrealAgent.Backend.Core.Block;
using ConversationModel = UnrealAgent.Backend.Conversation.Conversation;

namespace UnrealAgent.Backend.Llm;

public sealed class OpenAiLlmClient : ILlmClient
{
    private const int MaxToolSteps = 8;

    private readonly ChatClient Client;
    private readonly ToolRegistry ToolRegistry;
    private readonly AgentSession AgentSession;

    public OpenAiLlmClient(
        string apiKey,
        string model,
        ToolRegistry toolRegistry,
        AgentSession agentSession)
    {
        Client = new ChatClient(model, apiKey);
        ToolRegistry = toolRegistry;
        AgentSession = agentSession;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        List<ChatMessage> messages =
        [
            new UserChatMessage(prompt)
        ];

        ChatCompletion completion = await Client.CompleteChatAsync(messages, cancellationToken: cancellationToken);

        if (completion.Content.Count == 0)
            return string.Empty;

        return string.Join(Environment.NewLine, completion.Content.Select(part => part.Text));
    }

    public async IAsyncEnumerable<string> GenerateStreamingAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        List<ChatMessage> messages =
        [
            new UserChatMessage(prompt)
        ];

        await foreach (StreamingChatCompletionUpdate update in Client
                           .CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            foreach (ChatMessageContentPart contentPart in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(contentPart.Text))
                    yield return contentPart.Text;
            }
        }
    }

    public async IAsyncEnumerable<ChatEvent> GenerateEventsAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (string chunk in GenerateStreamingAsync(prompt, cancellationToken))
        {
            yield return new ChatEvent.Assistant(chunk);
        }
    }

    public async Task<AssistantSpan> GenerateAssistantSpanAsync(
        ConversationModel conversation,
        Func<ChatEvent, Task>? onEvent = null,
        CancellationToken cancellationToken = default)
    {
        List<ChatMessage> messages = ToOpenAiMessages(conversation);
        ChatCompletionOptions options = CreateOptions();
        List<AssistantSpan.ToolExecution> toolExecutions = [];

        for (int step = 0; step < MaxToolSteps; step++)
        {
            ChatCompletion completion = await Client.CompleteChatAsync(
                messages,
                options,
                cancellationToken);

            if (completion.FinishReason == ChatFinishReason.ToolCalls)
            {
                messages.Add(new AssistantChatMessage(completion));

                foreach (ChatToolCall toolCall in completion.ToolCalls)
                {
                    string inputJson = toolCall.FunctionArguments.ToString();

                    // 26.06.03 - OpenAI 도구 호출 시작을 콘솔/UI 이벤트로 전달합니다.
                    if (onEvent is not null)
                        await onEvent(new ChatEvent.ToolStart(toolCall.Id, toolCall.FunctionName, inputJson));

                    ToolResult result = await ExecuteOpenAiToolCallAsync(toolCall, cancellationToken);
                    toolExecutions.Add(new AssistantSpan.ToolExecution(
                        toolCall.Id,
                        toolCall.FunctionName,
                        result.Content,
                        !result.bIsSuccess));

                    // 26.06.03 - OpenAI 도구 실행 결과를 콘솔/UI 이벤트로 전달합니다.
                    if (onEvent is not null)
                        await onEvent(new ChatEvent.ToolEnd(toolCall.Id, toolCall.FunctionName, result.Content));

                    messages.Add(new ToolChatMessage(toolCall.Id, result.Content));
                }

                continue;
            }

            string responseText = GetCompletionText(completion);

            if (onEvent is not null && !string.IsNullOrWhiteSpace(responseText))
                await onEvent(new ChatEvent.Assistant(responseText));

            AssistantSpan assistantSpan = new()
            {
                AssistantBlocks = string.IsNullOrWhiteSpace(responseText)
                    ? []
                    : [new Block.Text(responseText)]
            };

            // 26.06.03 - OpenAI 도구 실행 기록을 AssistantSpan에 저장해 Claude 경로와 동일한 기록 형태를 유지합니다.
            assistantSpan.ToolExecutions.AddRange(toolExecutions);
            return assistantSpan;
        }

        const string tooManyToolCalls = "도구 호출이 너무 많이 반복되어 중단했습니다.";

        if (onEvent is not null)
            await onEvent(new ChatEvent.Assistant(tooManyToolCalls));

        AssistantSpan fallbackSpan = new()
        {
            AssistantBlocks = [new Block.Text(tooManyToolCalls)]
        };
        fallbackSpan.ToolExecutions.AddRange(toolExecutions);
        return fallbackSpan;
    }

    private ChatCompletionOptions CreateOptions()
    {
        ChatCompletionOptions options = new()
        {
            AllowParallelToolCalls = false
        };

        foreach (ChatTool tool in ToolRegistry.GetAllOpenAiTools())
            options.Tools.Add(tool);

        return options;
    }

    private async Task<ToolResult> ExecuteOpenAiToolCallAsync(
        ChatToolCall toolCall,
        CancellationToken cancellationToken)
    {
        if (!ToolRegistry.TryGetTool(toolCall.FunctionName, out IAgentTool? tool) || tool is null)
            return ToolResult.Error($"ERROR: Unknown tool: {toolCall.FunctionName}");

        try
        {
            return await tool.ExecuteAsync(
                toolCall.FunctionArguments.ToString(),
                AgentSession,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return ToolResult.Error($"ERROR: Tool '{toolCall.FunctionName}' failed: {ex.Message}");
        }
    }

    private static List<ChatMessage> ToOpenAiMessages(ConversationModel conversation)
    {
        List<ChatMessage> messages = [];

        foreach (MessageSpan messageSpan in conversation.Spans)
        {
            if (messageSpan.UserInput is { Text: { } userText } &&
                !string.IsNullOrWhiteSpace(userText))
            {
                messages.Add(new UserChatMessage(userText));
            }

            foreach (AssistantSpan assistantSpan in messageSpan.AssistantSpans)
            {
                string assistantText = ToOpenAiAssistantText(assistantSpan.AssistantBlocks);

                if (!string.IsNullOrWhiteSpace(assistantText))
                    messages.Add(new AssistantChatMessage(assistantText));
            }
        }

        return messages;
    }

    private static string ToOpenAiAssistantText(IReadOnlyList<Block> assistantBlocks)
    {
        IEnumerable<string> textBlocks = assistantBlocks
            .OfType<Block.Text>()
            .Select(block => block.Content)
            .Where(content => !string.IsNullOrWhiteSpace(content));

        return string.Join(Environment.NewLine, textBlocks);
    }

    private static string GetCompletionText(ChatCompletion completion)
    {
        if (completion.Content.Count == 0)
            return string.Empty;

        return string.Join(Environment.NewLine, completion.Content.Select(part => part.Text));
    }
}
