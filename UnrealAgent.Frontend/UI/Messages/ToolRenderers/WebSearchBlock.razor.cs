using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Chat;
using static UnrealAgent.Frontend.UI.Messages.ToolBlock;


namespace UnrealAgent.Frontend.UI.Messages.ToolRenderers;

// Web_search 도구의 콘텐츠 렌더러
// 검색 결과를 제목 , URL, 게시일 목록으로 표시
public partial class WebSearchBlock : ComponentBase
{
    // 표시할 Tool 메시지
    [Parameter] public ChatUIMessage.Tool Message { get; set; } = default!;
    
    // 이 도구의 summary 바 메타데이터
    public static ToolMeta GetInfo(ChatUIMessage.Tool Msg)
        => new("language", "Web Search", "font-mono", Msg.GetInputField("query", "web_search"));
    
    //--------------------------------------------------------------------------
    // 검색 결과 파싱
    //--------------------------------------------------------------------------
    
    // 검색 결과 항목
    private sealed record SearchResult(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("page_age")]
        string? PageAge)
    {
        // URL 에서 추출한 도메인
        public string Domain => Uri.TryCreate(Url, UriKind.Absolute, out Uri? Parsed)
            ? Parsed.Host
            : Url;
    }
    
    // COntent Json을 파싱한 검색 결과 목록
    private List<SearchResult> Results => ParseResults();

    // CONTENT JSON 배열을 SearchResult 목록으로 파싱 
    private List<SearchResult> ParseResults()
    {
        if (string.IsNullOrEmpty(Message.Content))
            return [];
        try
        {
            return JsonSerializer.Deserialize<List<SearchResult>>(Message.Content) ?? [];
        }
        catch
        {
            return [];
        }
        
            
        
        
        
        
        
        
        
        
        
        
    }
}