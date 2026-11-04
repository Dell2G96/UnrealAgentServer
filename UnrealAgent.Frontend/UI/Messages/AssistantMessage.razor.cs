
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Chat;

namespace UnrealAgent.Frontend.UI.Messages;

public partial class AssistantMessage
{
    // 표시할 어시스턴트 메세지
    [Parameter] public ChatUIMessage.Assistant Message { get; set; } = null!;
    // Markdig 파이프라인
    // DisableHtml(): 메시지 안에 원본 HTML(<script> 등)이 섞여 있어도 태그를 그대로 렌더링하지 않고
    // 이스케이프 처리한다. 세션 파일 복원 등으로 조작된 텍스트가 들어와도 XSS로 이어지지 않도록 하기 위함
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().DisableHtml().Build();

    // 링크 href로 허용하는 URL 스킴. DisableHtml()은 <script> 등 원본 태그만 막고
    // [클릭](javascript:...) 같은 링크의 href는 그대로 통과시키므로 별도로 막아야 한다
    private static readonly string[] AllowedUrlSchemes = ["http://", "https://", "mailto:"];

    // Markdown 텍스트를 HTML로 변환
    private static string RenderMarkdown(string Md)
    {
        MarkdownDocument Document = Markdown.Parse(Md, Pipeline);
        NeutralizeUnsafeLinks(Document);

        using StringWriter Writer = new();
        HtmlRenderer Renderer = new(Writer);
        Pipeline.Setup(Renderer);
        Renderer.Render(Document);
        Writer.Flush();

        return Writer.ToString();
    }

    // 링크/자동링크 중 http/https/mailto가 아닌 스킴(javascript: 등)은 href를 "#"으로 무력화한다
    private static void NeutralizeUnsafeLinks(MarkdownObject Document)
    {
        foreach (MarkdownObject Node in Document.Descendants())
        {
            switch (Node)
            {
                case LinkInline { IsImage: false } Link when !IsAllowedUrl(Link.Url):
                    Link.Url = "#";
                    break;

                case AutolinkInline Auto when !IsAllowedUrl(Auto.Url):
                    Auto.Url = "#";
                    break;
            }
        }
    }

    private static bool IsAllowedUrl(string? Url) =>
        !string.IsNullOrEmpty(Url) &&
        AllowedUrlSchemes.Any(Scheme => Url.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase));
}

