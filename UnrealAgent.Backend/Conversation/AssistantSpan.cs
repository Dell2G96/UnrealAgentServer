using UnrealAgent.Backend.Core;

namespace UnrealAgent.Backend.Conversation;

/// <summary>
/// Claude API 호출 1회의 결과
/// 어시스턴트 응답 블록과 도구 실행 결과를 포함
/// </summary>
public sealed record AssistantSpan
{
    /// 어시스턴드 응답 블록 목록
    public required IReadOnlyList<Block> AssistantBlocks { get; init; }
}