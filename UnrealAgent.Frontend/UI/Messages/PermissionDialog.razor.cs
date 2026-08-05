
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Security;
using UnrealAgent.Frontend.Infrastructure;
using UnrealAgent.Frontend.UI.Messages.ToolRenderers;

namespace UnrealAgent.Frontend.UI.Messages;

public partial class PermissionDialog : JsComponentBase
{
    // 표시할 권한 요청
    [Parameter] public ChatEvent.ToolPermissionRequest Request { get; set; } = null;

    // 사용자 판정 결과 콜백
    [Parameter] public EventCallback<ToolPermission> OnDecision { get; set; }

    // JS에서 C# 매서드를 호출하기 위한 .NET 객체 참조
    private DotNetObjectReference<PermissionDialog>? DotNetRef;

    protected override  async Task OnModuleLoaded()
    {
        DotNetRef = DotNetObjectReference.Create(this);
        await Module.InvokeVoidAsync("setup", DotNetRef);
    }
    
    // JS에서 1,2,3 키 입력시 호출
    [JSInvokable]
    public async Task HandlePermissionKey(string Key)
    {
        ToolPermission? Decision = Key switch
        {
            "1" => ToolPermission.Allow,
            "2" => ToolPermission.AlwaysAllow,
            "3" => ToolPermission.Deny,
            _ => null
        };
        
        if(Decision is {} Permission)
            await OnDecision.InvokeAsync(Permission);
    }
    
    // 도구 입력에서 사용자가 읽을 수 있는 요약을 추출
    private string Summary => Request.ToolName switch
    {
        "web_search" => WebSearchBlock.GetPermissionSummary(Request.InputJson),
        "web_fetch" => WebFetchBlock.GetPermissionSummary(Request.InputJson),
        _ when CodeBlock.IsCodeTool(Request.ToolName) => CodeBlock.GetPermissionSummary(Request.InputJson),
        _ => ""
    };
}




















