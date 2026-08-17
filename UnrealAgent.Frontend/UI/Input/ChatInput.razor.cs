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
    
    // 멘션 팝업 참조
    private MentionPopup MenPopup = null!;
    
    // textarea 바인딩 값, 변경 시 커맨드 팝업을 갱신
    // 현재 입력 텍스트
    private string InputText
    {
        get;
        set
        {
            field = value;
            CmdPopup.Update(value);
            MenPopup.Update(value);
        }
    } = "";
    
    // [+] 패널(모델/Thinking/Effort/모드)이 펼쳐져 있는지 여부
    private bool bShowPanel;

    // 전송 버튼을 노란색으로 활성화할지 판단한다
    private bool bHasText => !string.IsNullOrWhiteSpace(InputText);
    
    // [+] 버튼 : 패널을 여닫는다
    private void TogglePanel() => bShowPanel = !bShowPanel;


    // JS 모듈 로드 후 Enter 키 바인딩 설정
    protected override async Task OnModuleLoaded()
    {
        DotNetRef = DotNetObjectReference.Create(this);
        await Module.InvokeVoidAsync("setupKeyBindings", TextAreaRef, DotNetRef);
        // 입력 내용에 따라 textarea 높이를 늘려 준다 (카톡과 동일한 동작)
        await Module.InvokeVoidAsync("setupAutoGrow", TextAreaRef);
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
    
    
    ///////////////////////////////////////////////////////////////////////////
    ///  멘션 팝업 
    ///////////////////////////////////////////////////////////////////////////
    
    // 멘션 팝업에서 방향키로 항목을 탐색한다.
    [JSInvokable]
    public async Task MentionNavigate(int Direction) => await MenPopup.Navigate(Direction);
    
    // 멘션 팝업에서 Enter로 최종 선택
    [JSInvokable]
    public void MentionSelect()
    {
        string? Path = MenPopup.Select();
        if (Path is null)
            return;

        int AtIndex = InputText.LastIndexOf('@');
        if (AtIndex < 0)
            return;

        InputText = InputText[..AtIndex] + Path + " ";
        
        StateHasChanged();
    }
    
    /// <summary>멘션 팝업에서 Tab으로 드릴다운/선택합니다.</summary>
    [JSInvokable]
    public void MentionTab()
    {
        string? Result = MenPopup.Tab();
        if (Result is null) 
            return;

        int AtIndex = InputText.LastIndexOf('@');
        if (AtIndex < 0) 
            return;

        InputText = Result.EndsWith('/')
            ? InputText[..AtIndex] + "@" + Result
            : InputText[..AtIndex] + Result + " ";
        
        StateHasChanged();
    }
    
    /// <summary>멘션 팝업에서 ← 키로 상위 이동합니다.</summary>
    [JSInvokable]
    public void MentionGoBack()
    {
        string? Result = MenPopup.GoBack();
        if (Result is null) 
            return;

        int AtIndex = InputText.LastIndexOf('@');
        if (AtIndex < 0) 
            return;

        InputText = InputText[..AtIndex] + "@" + Result;
        StateHasChanged();
    }

    /// <summary>멘션 팝업을 닫습니다.</summary>
    [JSInvokable]
    public void MentionClose() => MenPopup.Close();

    /// <summary>폼 제출 시 메시지를 전송합니다.</summary>
    private async Task HandleSubmit()
    {
        string Trimmed = InputText.Trim();

        if (string.IsNullOrEmpty(Trimmed))
            return;

        InputText = "";
        // 늘어나 있던 textarea 높이를 한 줄로 되돌린다.
        // Module은 첫 렌더 이후에만 채워지므로 null 검사를 함께 한다.
        if (Module is not null)
            await Module.InvokeVoidAsync("resetHeight", TextAreaRef);

        await OnSend.InvokeAsync(Trimmed);    }
    
    
}