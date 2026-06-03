namespace UnrealAgent.Backend.Core;

// 에이전트에서 UI로 전달되는 스트리밍 이벤트
public abstract record ChatEvent
{
    //
    public sealed record Text(string Content) : ChatEvent;
    
    // Claude의 사고과정 응답
    public sealed record Thinking(string Content) : ChatEvent;
    
    // 도구 실행 시작
    public sealed record ToolStart(string ToolUseId, string Name, string Input) : ChatEvent;
    
    // 도구 실행 결과
    public sealed record ToolEnd(string ToolUseId, string Name, string Result) : ChatEvent;

    // 스트림 종료
    public sealed record Done : ChatEvent;
}