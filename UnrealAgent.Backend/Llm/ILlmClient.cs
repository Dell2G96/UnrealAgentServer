namespace UnrealAgent.Backend.Llm;

using UnrealAgent.Backend.Conversation;
using UnrealAgent.Backend.Core;
using ConversationModel = UnrealAgent.Backend.Conversation.Conversation;

public interface ILlmClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GenerateStreamingAsync(string prompt, CancellationToken cancellationToken = default);

    IAsyncEnumerable<ChatEvent> GenerateEventsAsync(string prompt, CancellationToken cancellationToken = default);

    Task<AssistantSpan> GenerateAssistantSpanAsync(
        ConversationModel conversation,
        Func<ChatEvent, Task>? onEvent = null,
        CancellationToken cancellationToken = default);
}
