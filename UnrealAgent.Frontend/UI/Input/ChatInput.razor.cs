using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UnrealAgent.Backend.Conversation;
using UnrealAgent.Frontend.Infrastructure;

namespace UnrealAgent.Frontend.UI.Input;


public partial class ChatInput :JsComponentBase
{
    // 메세지 전송 콜백
    [Parameter] public EventCallback<UserInput> OnSend { get; set; }
    

    
    // textarea 요소 참조
    private ElementReference TextAreaRef;
    
    // .Net에서 JS 가 호출할 수 있는 참조
    private DotNetObjectReference<ChatInput>? DotNetRef;
    
    // 모드 스위치 컴포넌트 참조
    private ModeSwitcher ModeSwitcherRef = null!;
    
    // 커맨드 팝업 참조
    private CommandPopup CmdPopup = null!;
    
    // textarea 바인딩 값, 변경 시 커맨드 팝업을 갱신
    // 현재 입력 텍스트
    private string InputText
    {
        get;
        set
        {
            field = value;
            CmdPopup.Update(value);
        }
    }
   

    // JS 모듈 로드 후 Enter 키 바인딩 설정
    protected override async Task OnModuleLoaded()
    {
        DotNetRef = DotNetObjectReference.Create(this);
        await Module.InvokeVoidAsync("setupKeyBindings", TextAreaRef, DotNetRef);
        
    }
    
    // Shift + Tab 시 JS에서 호출된다
    [JSInvokable]
    public void CycleMode() => ModeSwitcherRef.CycleMode();
    
    // 팝업에서 방향키로 항목을 탐색
    [JSInvokable]
    public async Task PopupNavigate(int Direction) => await CmdPopup.Navigate(Direction);

    // 팝업에서 현재 선택된 항목을 적용한다
    [JSInvokable]
    public void PopupSelect()
    {
        string? Result = CmdPopup.Select();

        if (Result is not null)
        {
            InputText = Result;
            StateHasChanged();
        }
    }
    
    
    // 팝업을 닫는다
    [JSInvokable]
    public void PopupClose() => CmdPopup.Close();
    
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