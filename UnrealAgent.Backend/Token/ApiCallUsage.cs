using Anthropic.Models.Messages;

namespace UnrealAgent.Backend.Token;

/// <summary>
/// API 호출 1회의 토큰 사용량
/// 캐시 쓰기/읽기는 단가가 달라서 일반 입력과 따로 보관한다
/// </summary>
public sealed record ApiCallUsage(long InputTokens, long CacheWriteTokens, long CacheReadTokens, long OutputTokens)
{
    // 비스트리밍 응답(Message.Usage)에서 생성한다
    public static ApiCallUsage From(Usage Usg) =>
        new(Usg.InputTokens, Usg.CacheCreationInputTokens ?? 0, Usg.CacheReadInputTokens ?? 0, Usg.OutputTokens);
}
