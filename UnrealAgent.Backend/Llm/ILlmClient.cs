namespace UnrealAgent.Backend.Llm;

using UnrealAgent.Backend.Core;

public interface ILlmClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GenerateStreamingAsync(string prompt, CancellationToken cancellationToken = default);

    IAsyncEnumerable<ChatEvent> GenerateEventsAsync(string prompt, CancellationToken cancellationToken = default);
}
