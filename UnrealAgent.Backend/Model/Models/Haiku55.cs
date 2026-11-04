using UnrealAgent.Backend.Tool.Attributes;

namespace UnrealAgent.Backend.Model.Models;

// Claude Haiku 5.5 모델
// thinking 은 Effort 가 High 이하일 때만 끌 수 있다 (ModelSettings.GetThinking 참고)
[AgentModel(Order = 3)]
public sealed class Haiku55 : IModel
{
    public const string ModelId = "claude-haiku-5-5";
    public string Id => ModelId;
    public string DisplayName => "Haiku 5.5";
    public string Description => "가장 저렴한 모델.";
    public int MaxOutputTokens => 128_000;
    public int ContextWindow => 1_000_000;
    // 입력 10만 토큰 이하 기준 단가 (초과 시 $0.50 / $2.50)
    public double InputPricePerMTok => 0.10;
    public double OutputPricePerMTok => 0.50;
}
