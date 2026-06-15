using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Chat;

namespace UnrealAgent.Frontend.Page;

public partial class Chat : IAsyncDisposable
{
    // 에이전트 실행 서비스
    [Inject] private AgentRunner AgentRunner { get; set; } = null!;
    
    // 설정 패널 표시 여부
    private bool bShowSettings;

    // 설정 패널을 토클한다
    private void ToggleSettings( ) => bShowSettings = !bShowSettings;
    
    // 플랜 사용량 패널 표시 여부
    private bool bShowUsage;

    // 플랜 사용량 패널 토글
    private void ToggleUsage() => bShowUsage = !bShowUsage;

    protected override void OnInitialized()
    {
        AgentRunner.OnChatEvent += OnChatEvent;
    }

    public async ValueTask DisposeAsync()
    {
        AgentRunner.OnChatEvent -= OnChatEvent;
        await ValueTask.CompletedTask;
    }

    // AgentRunner의 Chatevent를 UI 스레드에서 처리한다
    // Sotre 수정과 렌더링이 같은 스레드에서 실행되어 스레드 안전성을 보장한다
    private Task OnChatEvent(ChatEvent Evt) => InvokeAsync(() =>
    {
        AgentRunner.Store.Process(Evt);

        // 변경된 상태를 Blazor에 렌더링 요청
        StateHasChanged();
    });
}

















