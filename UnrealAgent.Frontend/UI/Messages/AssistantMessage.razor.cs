
using Markdig;
using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Chat;

namespace UnrealAgent.Frontend.UI.Messages;

public partial class AssistantMessage
{
    // 표시할 어시스턴트 메세지
    [Parameter] public ChatUIMessage.Assistant Message { get; set; } = null!;
    // Markdig 파이프라인
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    // Markdown 텍스트를 HTML로 변환o
    private static string RenderMarkdown(string Md) => Markdown.ToHtml(Md, Pipeline);
}

