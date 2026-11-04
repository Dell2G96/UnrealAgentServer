using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Frontend.Infrastructure;

namespace UnrealAgent.Frontend.UI.Messages;

public partial class ChatMessages : JsComponentBase
{
    // 표시할 메세지 목록
    [Parameter] public List<ChatUIMessage> Messages { get; set; } = [];

    // 응답 수신 시작 여부, true 면 shimmer를 숨긴다
    [Parameter] public bool bIsReceiving { get; set; }

    // 스크롤되는 메세지 영역 (JS 에 넘겨 자동 스크롤을 건다)
    private ElementReference Container;

    // 맨 아래로 이동 버튼 (표시/숨김과 클릭 처리는 JS 가 담당)
    private ElementReference BottomButton;

    // 자동 스크롤은 전부 JS 에서 처리한다
    // DOM 이 바뀔 때마다 JS 가 직접 바닥을 따라가므로 렌더링마다 C# → JS 호출이 필요 없다
    protected override async Task OnModuleLoaded()
    {
        await Module.InvokeVoidAsync("setup", Container, BottomButton);
    }
}
