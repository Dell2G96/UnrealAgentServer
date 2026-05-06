using Anthropic;
using Anthropic.Models.Messages;
using System.Runtime.CompilerServices;
using System.Text;

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
        StringBuilder response = new();

        await foreach (string chunk in GenerateStreamingAsync(prompt, cancellationToken))
        {
            response.Append(chunk);
        }

        return response.ToString();
    }

    public async IAsyncEnumerable<string> GenerateStreamingAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        MessageCreateParams parameters = CreateParameters(prompt);

        await foreach (RawMessageStreamEvent streamEvent in Client.Messages
                           .CreateStreaming(parameters, cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            if (streamEvent.TryPickContentBlockDelta(out RawContentBlockDeltaEvent? delta) &&
                delta.Delta.TryPickText(out TextDelta? text))
            {
                yield return text.Text;
            }
        }
    }

    private MessageCreateParams CreateParameters(string prompt)
    {
        return new MessageCreateParams
        {
            Model = Model,
            MaxTokens = 1024,
            Messages =
            [
                new() { Role = Role.User, Content = prompt }
            ],
            Thinking = new ThinkingConfigAdaptive(),
            OutputConfig = new OutputConfig
            {
                Effort = Effort.High
            }
        };
    }
}
