using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UnrealAgent.Backend.Chat; 
using UnrealAgent.Frontend.Infrastructure;

namespace UnrealAgent.Frontend.UI.Messages.ToolRenderers;


// 코드 실행 도구 전용 렌더러
// 도구 이름에 포함된 언어 키워드를 감지하여 적절한 Syntx highlight 클래스를 적용한다
public partial class CodeBlock : JsComponentBase
{
    // 도구 메세지
    [Parameter] public ChatUIMessage.Tool Message { get; set; } = null!;
    
    // code 요소 참조
    private ElementReference CodeRef;
    
    //--------------------------------------------------------------------------
    // 언어 매핑
    //--------------------------------------------------------------------------
    
    // 도구 이름 키워드 -> Prism 언어 클래스 매핑
    private static readonly (string Keyword, string language)[] LanguageMap =
    [
        ("python", "python"),
        ("java", "java"),
        ("cpp", "cpp"),
        ("csharp", "csharp"),
        ("js", "javascript"),
        ("lua", "lua"),
    ];
    
    
    // 도구 이름에서 Prism 언어를 감지한다.
    private string DetectedLanguage
    {
        get
        {
            var ToolName = Message.Name.ToLowerInvariant();
            foreach (var (Keyword, Language) in LanguageMap)
            {
                if (ToolName.Contains(Keyword))
                    return Language;
            }

            return "plaintext";
        }
    }
    
    // Prism.js 에 전달할 언어 CSS 클래스
    private string LanguageClass => $"language-{DetectedLanguage}";
    
    //--------------------------------------------------------------------------
    // 정적 헬퍼
    //--------------------------------------------------------------------------
    
    // 이 도구가 Codeblock으로 렌더링 되어야 하는지 판별한다
    public static bool IsCodeTool(string ToolName)
    {
        var Lower = ToolName.ToLowerInvariant();
        return LanguageMap.Any(Entry => Lower.Contains(Entry.Keyword));
    }
    
    // ToolBlock Summary 바에 표시할 메타데이터를 반환한다
    public static ToolBlock.ToolMeta GetInfo(ChatUIMessage.Tool Msg)
        => new("code", Msg.Name, "font-mono", Msg.GetInputField("purpose"));
    
    //--------------------------------------------------------------------------
    // 라이프사이클
    //--------------------------------------------------------------------------

    protected override async Task OnModuleLoaded()
    {
        if (!string.IsNullOrEmpty(Message.Input))
            await Module.InvokeVoidAsync("highlightCode", CodeRef);
    }
    
    //--------------------------------------------------------------------------
    // 포맷터
    //--------------------------------------------------------------------------
    // JSON INPUT 에 서 Code 필드를 추출한다
    private static string ExtractCode(string JsonInput)
    {
        if (string.IsNullOrEmpty(JsonInput))
            return "";

        try
        {
            JsonDocument Doc = JsonDocument.Parse(JsonInput);
            if (Doc.RootElement.TryGetProperty("code", out var CodeEl))
                return CodeEl.GetString() ?? JsonInput;
        }
        catch
        {
            
        }

        return JsonInput;
    }
    
    // 출력을 줄 단위로 분리한다
    private string[] OutputLines()
    {
        if (string.IsNullOrEmpty(Message.Content))
            return [];

        return Message.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }
    
    // 출력 줄에 색상을 적용한다
    private static string FormatOutputLine(string Line)
    {
        if (Line.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
            return $"<span class=\"text-[#e05e5e]\">{System.Net.WebUtility.HtmlEncode(Line)}</span>";
        if (Line.Contains("WARN", StringComparison.OrdinalIgnoreCase))
            return $"<span class=\"text-[#e5c07b]\">{System.Net.WebUtility.HtmlEncode(Line)}</span>";
        if (Line.Contains("SUCCESS", StringComparison.OrdinalIgnoreCase))
            return $"<span class=\"text-[#98c379]\">{System.Net.WebUtility.HtmlEncode(Line)}</span>";
        return $"<span class=\"text-[#aaa]\">{System.Net.WebUtility.HtmlEncode(Line)}</span>";
    }
}






















