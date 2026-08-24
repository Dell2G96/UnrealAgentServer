


using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Team;

namespace UnrealAgent.Frontend.UI.Layout;


public partial class AgentTabBar : IDisposable
{
    // 팀 정보
    [Parameter, EditorRequired] public Team Team { get; set; } = null!;

    // 탭 선택시 호출된다.
    // null 이면 리더 | 값 이면 팀원 포트
    [Parameter] public EventCallback<int?> OnTabSelected { get; set; }
    
    // 현재 선택된 포트 , null : 리더 탭
    private int? SelectedPort;

    protected override void OnInitialized()
    {
        Team.OnTeamChanged += OnTeamChanged;
    }

    public void Dispose()
    {
        Team.OnTeamChanged -= OnTeamChanged;
    }

    private void OnTeamChanged() => InvokeAsync(StateHasChanged);
    
    // 탭을 선택하고 부모에게 알린다.
    private async Task SelectTab(int? Port)
    {
        SelectedPort = Port;
        await OnTabSelected.InvokeAsync(Port);
    }

    // 리더 탭의 CSS 클래스 반환
    private string LeaderTabClass()
    {
        string Base = "h-full flex items-center gap-2 px-5 border-t-2 text-[11px] font-semibold tracking-wide transition-colors relative";
        return SelectedPort is null
            ? $"{Base} border-t-transparent border-b border-b-[#0070e0] bg-[#1a1a1a] text-[#fff]"
            : $"{Base} border-t-transparent border-b border-b-transparent text-[#888] hover:text-[#ccc] hover:bg-[#1a1a1a]/50";
    }

    // 팀원 탭의 CSS 반환
    private string TeammateTabClass(int Port)
    {
        string Base = "h-full flex items-center gap-2 px-4 border-t-2 text-[11px] font-semibold tracking-wide transition-colors relative";
        return SelectedPort == Port
            ? $"{Base} border-t-transparent border-b border-b-[#10b981] bg-[#1a1a1a] text-[#fff]"
            : $"{Base} border-t-transparent border-b border-b-transparent text-[#888] hover:text-[#ccc] hover:bg-[#1a1a1a]/50";
    }
}