using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Chat;
using static UnrealAgent.Frontend.UI.Messages.ToolBlock;

namespace UnrealAgent.Frontend.UI.Messages.ToolRenderers;

// Web_fetch 도구의 콘텐츠 렌더러
// URL 헤더와 페치된 콘텐츠를 표시한다
public partial class WebFetchBlock : ComponentBase
{
    // 표시할 TOol 메시지
    [Parameter] public ChatUIMessage.Tool Message { get; set; } = default!;
    
    // 이 도구의 summary 바 메타데이터
    public static ToolMeta GetInfo(ChatUIMessage.Tool Msg)
        => new("language", "Web Fetch", "font-mono", GetDomain(Msg));
    
    // 권한 다이얼로그에 표시할 요약
    public static string GetPermissionSummary(string InputJson)
        => ChatUIMessage.Tool.GetInputField(InputJson, "url");
    
    // 입력 JSON에서 원본 URL을 가져온다
    private string FetchUrl => ChatUIMessage.Tool.GetInputField(Message.Input,"url");
    
    // URL의 도메인을 추출한다
    private string Domain => GetDomain(Message);
    
    // 입력 JSON 에서 URL 의 도메인을 추출한다
    private static string GetDomain(ChatUIMessage.Tool Msg)
    {
        string Url = ChatUIMessage.Tool.GetInputField(Msg.Input, "url");
        return Uri.TryCreate(Url, UriKind.Absolute, out Uri? Parsed) ? Parsed.Host : Url;
    }
}

