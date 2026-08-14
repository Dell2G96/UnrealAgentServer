using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Mode;

namespace UnrealAgent.Frontend.UI.Input;

public partial class ModeSwitcher : ComponentBase
{
    // 현재 대화 섹션
    [Inject] private AgentSession Session { get; set; } = null;
    
    // 현재 모드
    private AgentMode CurrentMode => Session.Mode;
    
    // 모드를 직접 설정
    private void SetMode(AgentMode Mode)
    {
        Session.Mode = Mode;
    }
    
    // 모드를 단축키로 전환
    public void CycleMode()
    {
        Session.Mode = Session.Mode switch
        {
            AgentMode.Normal => AgentMode.Edit,
            AgentMode.Edit   => AgentMode.Plan,
            _                => AgentMode.Normal
        };
        
        StateHasChanged();
    }
    
    // 모드별 도트 색상 클래스 반환
    // 흰 입력바 위에 놓이므로, 밝은 배경에서도 대비가 나오는 진한 톤을 쓴다.
    private static string DotColor(AgentMode Mode, bool bIsActive) => Mode switch
    {
        AgentMode.Normal => bIsActive ? "bg-[#475569]" : "bg-[#94a3b8]/60",
        AgentMode.Edit   => bIsActive ? "bg-[#9333ea]" : "bg-[#94a3b8]/60",
        AgentMode.Plan   => bIsActive ? "bg-[#16a34a]" : "bg-[#94a3b8]/60",
        _                => "bg-[#94a3b8]/60"
    };

    /// <summary>모드별 텍스트 색상 클래스를 반환합니다.</summary>
    private static string TextColor(AgentMode Mode, bool bIsActive) => Mode switch
    {
        AgentMode.Normal => bIsActive ? "text-[#334155]" : "text-[#8a97a4]",
        AgentMode.Edit   => bIsActive ? "text-[#9333ea]" : "text-[#8a97a4]",
        AgentMode.Plan   => bIsActive ? "text-[#16a34a]" : "text-[#8a97a4]",
        _                => "text-[#8a97a4]"
    };
    
}






















