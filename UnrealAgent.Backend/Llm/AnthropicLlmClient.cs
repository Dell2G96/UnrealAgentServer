using Anthropic;
using Anthropic.Models.Messages;
using System.Runtime.CompilerServices;
using System.Text;
using UnrealAgent.Backend.Conversation;
using UnrealAgent.Backend.Core;

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

    public async IAsyncEnumerable<ChatEvent> GenerateEventsAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        MessageCreateParams parameters = CreateParameters(prompt);
        ApiSteamSpan span = new();

        await foreach (RawMessageStreamEvent streamEvent in Client.Messages
                           .CreateStreaming(parameters, cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            ChatEvent? chatEvent = span.Process(streamEvent);

            if (chatEvent is not null)
                yield return chatEvent;
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
