
using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Chat;

namespace UnrealAgent.Frontend.UI.Messages;

public partial class UserMessage
{
    // 표시할 유저 메시지
    [Parameter] public ChatUIMessage.User Message { get; set; } = null!;
}

