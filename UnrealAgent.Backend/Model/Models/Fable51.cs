using UnrealAgent.Backend.Tool.Attributes;

namespace UnrealAgent.Backend.Model.Models;

// Claude Fable 5.1 모델 (가장 성능이 좋은 모델)
// thinking 을 끌 수 없다 (ModelSettings.GetThinking 참고)
[AgentModel(Order = 0)]
public sealed class Fable51 : IModel
{
    public const string ModelId = "claude-fable-5-1";
    public string Id => ModelId;
    public string DisplayName => "Fable 5.1";
    public string Description => "가장 똑똑하지만 가장 비싼 모델.";
    public int MaxOutputTokens => 128_000;
    public int ContextWindow => 1_000_000;
    public double InputPricePerMTok => 10.0;
    public double OutputPricePerMTok => 50.0;
}
