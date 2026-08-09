using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace UnrealAgent.Frontend.Infrastructure;

public abstract class JsComponentBase : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime Js { get; set; } = null!;
    
    // 로드된 JS 모듈 참조 . 
    // OnModuleLoaded() 이후 사용 가능
    protected IJSObjectReference Module = null!;
    
    // 빌드 시각 기반 캐시 무효화 버전
    private static readonly long CacheBuster = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Module = await Js.InvokeAsync<IJSObjectReference>("import", GetModulePath());
            await OnModuleLoaded();

        }
    }

    // JS 모듈 완료 후 호출된다
    // JS 함수 호출은 이 함수를 통해 진행한다
    protected virtual Task OnModuleLoaded() => Task.CompletedTask;

    private string GetModulePath()
    {
        // ChatInput 컴포넌트 기준
        
        // 'UnrealAgent.Frontend.Ui.Input'
        string Namespace = GetType().Namespace!;
        
        // 'UnrealAgent.Frontend.
        const string prefix = "UnrealAgent.Frontend.";
        
        // UI.Input -> UI/Input
        string Relative = Namespace[prefix.Length..].Replace('.', '/');
        
        // "ChatInput"
        string Name = GetType().Name;
        
        // "./UI/Input/ChatInput.razor.js"
        return $"./{Relative}/{Name}.razor.js?v={CacheBuster}";
    }

    public async ValueTask DisposeAsync()
    {
        // 첫 렌더 전에 컴포넌트가 사라지거나 import 가 실패하면 Module 은 null 인 채로 남는다
        if (Module is null)
            return;

        try
        {
            await Module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // 서킷이 이미 끊긴 경우 무시
        }
    }
}