
using Markdig;
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

    // Markdown 텍스트를 HTML로 변환o
    private static string RenderMarkdown(string Md) => Markdown.ToHtml(Md, Pipeline);
}

