using UnrealAgent.Backend.Tool.Attributes;

namespace UnrealAgent.Backend.Model.Models;

// Claude Sonnet 5.5 모델
// thinking 을 끌 수 없다 (ModelSettings.GetThinking 참고)
[AgentModel(Order = 2)]
public sealed class Sonnet55 : IModel
{
    public const string ModelId = "claude-sonnet-5-5";
    public string Id => ModelId;
    public string DisplayName => "Sonnet 5.5";
    public string Description => "속도와 지능의 최적 균형을 갖춘 모델입니다.";
    public int MaxOutputTokens => 128_000;
    public int ContextWindow => 1_000_000;
    public double InputPricePerMTok => 2.0;
    public double OutputPricePerMTok => 10.0;
}
