using OpenAI.Chat;
using System.Runtime.CompilerServices;
using System.Text;
using UnrealAgent.Backend.Conversation;
using UnrealAgent.Backend.Core;
using Block = UnrealAgent.Backend.Core.Block;
using ConversationModel = UnrealAgent.Backend.Conversation.Conversation;

namespace UnrealAgent.Backend.Llm;

public sealed class OpenAiLlmClient : ILlmClient
{
    private readonly ChatClient Client;

    public OpenAiLlmClient(string apiKey, string model)
    {
        Client = new ChatClient(model, apiKey);
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

        /*
         * 26.05.11 - 기존 단발 응답 기반 스트리밍 대체 코드 보존
         * yield return await GenerateAsync(prompt, cancellationToken);
         */
    }

    public async IAsyncEnumerable<ChatEvent> GenerateEventsAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (string chunk in GenerateStreamingAsync(prompt, cancellationToken))
        {
            yield return new ChatEvent.Text(chunk);
        }

        /*
         * 26.05.11 - 기존 단발 응답 이벤트 코드 보존
         * yield return new ChatEvent.Text(await GenerateAsync(prompt, cancellationToken));
         */
    }

    public async Task<AssistantSpan> GenerateAssistantSpanAsync(
        ConversationModel conversation,
        Func<ChatEvent, Task>? onEvent = null,
        CancellationToken cancellationToken = default)
    {
        List<ChatMessage> messages = ToOpenAiMessages(conversation);
        StringBuilder response = new();

        await foreach (StreamingChatCompletionUpdate update in Client
                           .CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            foreach (ChatMessageContentPart contentPart in update.ContentUpdate)
            {
                if (string.IsNullOrEmpty(contentPart.Text))
                    continue;

                response.Append(contentPart.Text);

                if (onEvent is not null)
                    await onEvent(new ChatEvent.Text(contentPart.Text));
            }
        }

        List<Block> assistantBlocks = [];

        if (response.Length > 0)
            assistantBlocks.Add(new Block.Text(response.ToString()));

        return new AssistantSpan
        {
            AssistantBlocks = assistantBlocks
        };
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
}
