using UnrealAgent.Backend.Tool.Attributes;

namespace UnrealAgent.Backend.Model.Models;

// Claude Opus 4.8 모델
[AgentModel(Order = 1)]
public sealed class Opus48 : IModel
{
    public const string ModelId = "claude-opus-4-8";
    public string Id => ModelId;
    
    public string DisplayName => "Opus 4.8";
    public string Description => "계획, 리뷰 등등에 사용하자";
    public int MaxOutputTokens => 128_000;
    public int ContextWindow => 200_000;
}