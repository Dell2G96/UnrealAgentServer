using UnrealAgent.Backend.Tool.Attributes;

namespace UnrealAgent.Backend.Model.Models;

// Claude Opus 5.5 모델 (기본 모델)
// thinking 을 끌 수 없다 (ModelSettings.GetThinking 참고)
[AgentModel(Order = 1)]
public sealed class Opus55 : IModel
{
    public const string ModelId = "claude-opus-5-5";
    public string Id => ModelId;
    public string DisplayName => "Opus 5.5";
    public string Description => "계획, 리뷰 등등에 사용하자";
    public int MaxOutputTokens => 128_000;
    public int ContextWindow => 1_000_000;
    public double InputPricePerMTok => 4.0;
    public double OutputPricePerMTok => 20.0;
}
