using Anthropic;
using Anthropic.Models.Messages;
using System.Runtime.CompilerServices;
using System.Text;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Conversation;
using UnrealAgent.Backend.Tool.Tools;
using ConversationModel = UnrealAgent.Backend.Conversation.Conversation;

namespace UnrealAgent.Backend.Llm;

public sealed class AnthropicLlmClient : ILlmClient
{
    private readonly AnthropicClient Client;
    private readonly string Model;
    private readonly ToolRegistry ToolRegistry;

    public AnthropicLlmClient(AnthropicClient client, string model, ToolRegistry toolRegistry)
    {
        Client = client;
        Model = model;
        ToolRegistry = toolRegistry;
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
        ApiStreamSpan span = new();

        await foreach (RawMessageStreamEvent streamEvent in Client.Messages
                           .CreateStreaming(parameters, cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            ChatEvent? chatEvent = span.Process(streamEvent);

            if (chatEvent is not null)
                yield return chatEvent;
        }
    }

    public async Task<AssistantSpan> GenerateAssistantSpanAsync(
        ConversationModel conversation,
        Func<ChatEvent, Task>? onEvent = null,
        CancellationToken cancellationToken = default)
    {
        MessageCreateParams parameters = CreateParameters(conversation);
        ApiStreamSpan span = new();

        await foreach (RawMessageStreamEvent streamEvent in Client.Messages
                           .CreateStreaming(parameters, cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            ChatEvent? chatEvent = span.Process(streamEvent);

            if (chatEvent is not null && onEvent is not null)
                await onEvent(chatEvent);
        }

        return span.Complete() switch
        {
            ApiStreamSpan.Result.Continue { CompletedSpan: AssistantSpan completedSpan } => completedSpan,
            ApiStreamSpan.Result.EndSpan { CompleteSpan: AssistantSpan completeSpan } => completeSpan,
            _ => new AssistantSpan { AssistantBlocks = [] }
        };
    }

    private MessageCreateParams CreateParameters(string prompt)
    {
        return new MessageCreateParams
        {
            System = "너는 언리얼에 특화되어 있는 에이전트야",
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

    private MessageCreateParams CreateParameters(ConversationModel conversation)
    {
        return new MessageCreateParams
        {
            Model = Model,
            MaxTokens = 1024,
            System = new List<TextBlockParam>
            {
                new()
                {
                    Text = """
                           You are UnrealAgent, an AI assistant that controls Unreal Editor.
                           Use the available tools when you need current state, external data, or editor automation.
                           Do not claim that no tools are available if tools are present in the request.
                           """
                }
            },
            Messages = conversation.ToAnthropicMessages(),
            Tools = ToolRegistry.GetAllSchemas().Select(schema => (ToolUnion)schema).ToList(),
            Thinking = new ThinkingConfigAdaptive(),
            OutputConfig = new OutputConfig
            {
                Effort = Effort.High
            }
        };
    }
}
