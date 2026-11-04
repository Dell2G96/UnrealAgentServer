using System.Text.Json.Serialization;

namespace UnrealAgent.Backend.Core;

// 어시스트 응답의 콘텐츠 블록
// 안트로픽 SDK 타입 대신 도메인 전체에서 사용
// 26.11.01 - 세션 파일 저장 시 블록 종류를 "$type"으로 함께 기록하기 위한 JSON 다형성 특성 추가
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Text), "text")]
[JsonDerivedType(typeof(Thinking), "thinking")]
[JsonDerivedType(typeof(ToolUse), "tool_use")]
public abstract record Block
{
    // 텍스트 응답 블록
    public sealed record Text(string Content) : Block;

    // 사고 과정 블록
    public sealed record Thinking(string Content, string? Signature) : Block;
    
    // 도구 호출 블록
    public sealed record ToolUse(string Id, string Name, string InputJson) : Block;
}