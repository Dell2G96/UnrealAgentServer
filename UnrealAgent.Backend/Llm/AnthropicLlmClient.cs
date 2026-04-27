using Anthropic;
using Anthropic.Models.Messages;

namespace UnrealAgent.Backend.Llm;

public sealed class AnthropicLlmClient : ILlmClient
{
    private readonly AnthropicClient Client;
    private readonly string Model;

    public AnthropicLlmClient(AnthropicClient client, string model)
    {
        Client = client;
        Model = model;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        MessageCreateParams parameters = new()
        {
            Model = Model,
            MaxTokens = 1024,
            Messages =
            [
                new() { Role = Role.User, Content = prompt }
            ]
        };

        Message response = await Client.Messages.Create(parameters, cancellationToken);
        List<string> texts = [];

        foreach (ContentBlock block in response.Content)
        {
            if (block.TryPickText(out var text))
                texts.Add(text.Text);
        }

        return string.Join(Environment.NewLine, texts);
    }
}
