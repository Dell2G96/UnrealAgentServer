
using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Model;

namespace UnrealAgent.Frontend.UI.Input;

public partial class ThinkingToggle
{
    // 모델 설정 서비스
    [Inject] private ModelSettings Settings { get; set; } = null!;
    
    // 토클 상태로 전환
    private void Toggle() {Settings.bThinkingEnabled = !Settings.bThinkingEnabled;}
    
    // 라벨 색상,
    private string LabelColorClass => Settings.bThinkingEnabled ? "text-white" : "text-[#666]";
    
    // 트랙 CSS 클래스
    private string TrackClass => Settings.bThinkingEnabled ? "think-on" : "think-off";
}