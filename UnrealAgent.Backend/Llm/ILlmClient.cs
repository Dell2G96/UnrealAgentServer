namespace UnrealAgent.Backend.Llm;

public interface ILlmClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GenerateStreamingAsync(string prompt, CancellationToken cancellationToken = default);
}
