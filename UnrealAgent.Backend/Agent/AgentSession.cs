namespace UnrealAgent.Backend.Agent;

/// <summary>
/// 에이전트 세션
/// 프로세스 아이덴티티, 대화 상태, 미들웨어 파이프라인을 통합한다
/// </summary>
public sealed class AgentSession
{
    // 세션의 대화 히스토리
    public Conversation.Conversation Conversation { get; } = new();
}