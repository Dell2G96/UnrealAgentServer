using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UnrealAgent.Backend.Conversation;
using UnrealAgent.Frontend.Infrastructure;

namespace UnrealAgent.Frontend.UI.Input;


public partial class ChatInput :JsComponentBase
{
    // 메세지 전송 콜백
    [Parameter] public EventCallback<UserInput> OnSend { get; set; }
    // 현재 입력 텍스트
    private string InputText = "";

    
    // textarea 요소 참조
    private ElementReference TextAreaRef;
    
    // JS 모듈 로드 후 Enter 키 바인딩 설정
    protected override async Task OnModuleLoaded()
    {
        await Module.InvokeVoidAsync("setupEnterSubmit", TextAreaRef);
    }

    // 폼 제출 시 메세지 전송
    private async Task HandleSubmit()
    {
        string Trimmed = InputText.Trim();
        
        if (string.IsNullOrEmpty(Trimmed))
            return;

        InputText = "";
        await OnSend.InvokeAsync(Trimmed);
    }
}