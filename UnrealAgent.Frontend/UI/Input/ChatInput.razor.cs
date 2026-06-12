using Microsoft.AspNetCore.Components;

namespace UnrealAgent.Frontend.UI.Input;


public partial class ChatInput
{
    // 메세지 전송 콜백
    [Parameter] public EventCallback<string> OnSend { get; set; }
    // 현재 입력 텍스트
    private string InputText = "";

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