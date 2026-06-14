
using Anthropic.Models.Messages;
using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Model;

namespace UnrealAgent.Frontend.UI.Input;

public partial class EffortSelector
{
    // 모델 설정 서비스
    [Inject] private ModelSettings Settings { get; set; } = null!;
    
    // Effort 레벨 정의
    private record EffortLevel(string Label, Effort Value, string Description);

    // 선택 가능한 Effort 레벨 목록
    private static readonly EffortLevel[] Levels =
    [
        new("Low", Effort.Low, "빠른 응답, 기본로직"),
        new("Mid", Effort.Medium, "속도와 복잡성의 균형"),
        new("High", Effort.High, "심층 분석, 복잡한 문제 해결")
    ];

    // High 선택 시 Effort 라벨을 흰색으로 바꾼다
    private string EffortLabelClass => Settings.Effort == Effort.High ? "text-white" : "text-[#666]";

    // Effort 레벨을 변경
    private void SelectEffort(Effort Level)
    {
        Settings.Effort = Level;
    }

}