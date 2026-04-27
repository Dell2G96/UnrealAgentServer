using OpenAI.Chat;

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
}
