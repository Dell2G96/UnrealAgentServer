using UnrealAgent.Backend.Tool.Attributes;

namespace UnrealAgent.Backend.Model.Models;

// Claude Haiku 4.5 모델
[AgentModel(Order = 3)]
public sealed class Haiku45 : IModel
{
    public const string ModelId = "claude-haiku-4-5-20251001";
    public string Id => ModelId;
    public string DisplayName => "Haiku 4.5";
    public string Description => "가장 저렴한 모델.";
    public int MaxOutputTokens => 64_000;
    public int ContextWindow => 200_000;
}