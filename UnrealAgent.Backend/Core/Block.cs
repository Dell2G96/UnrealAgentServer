namespace UnrealAgent.Backend.Core;

// 어시스트 응답의 콘텐츠 블록
// 안트로픽 SDK 타입 대신 도메인 전체에서 사용
public abstract record Block
{
    // 텍스트 응답 블록
    public sealed record Text(string Content) : Block;

    // 사고 과정 블록
    public sealed record Thinking(string Content, string? Signature) : Block;
}