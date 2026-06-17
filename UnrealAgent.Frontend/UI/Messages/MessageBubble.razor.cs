using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Chat;

namespace UnrealAgent.Frontend.UI.Messages;

public partial class MessageBubble
{
    // 표시할 메세지
    [Parameter] public ChatUIMessage UIMessage { get; set; } = null!;
}