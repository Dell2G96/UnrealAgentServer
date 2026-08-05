using UnrealAgent.Backend.Tool.Attributes;

namespace UnrealAgent.Backend.Model.Models;
[AgentModel(Order = 0)]
public class Fable5 : IModel
{
    public const string ModelId = "claude-fable-5";
    public string Id => ModelId;
    public string DisplayName => "존나게 비싼 Fable";
    public string Description => "존나 비쌈";
    public int MaxOutputTokens => 128_000;
    public int ContextWindow => 1_000_000;
}