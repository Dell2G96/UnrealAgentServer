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
    
    // 도구 실행 결과 레코드
    public sealed record ToolExecution(string ToolUseId, string Name, string OutPut, bool bIsError);
    
    // 도구 실행 결과 목록 , 도구 호출이 없으면 비어 있다
    public List<ToolExecution> ToolExecutions { get; } = [];
    
    // API 호출의 입력 토큰 수
    public long InputTokens { get; init; }
}