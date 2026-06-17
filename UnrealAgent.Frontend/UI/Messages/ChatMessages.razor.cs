using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Chat;

namespace UnrealAgent.Frontend.UI.Messages;

public partial class ChatMessages
{
    // 표시할 메세지 목록
    [Parameter] public List<ChatUIMessage> Messages { get; set; } = [];
    
    // 응답 수신 시작 여부, true 면 shimmer를 숨긴다
    [Parameter] public bool bIsReceiving { get; set; }
}