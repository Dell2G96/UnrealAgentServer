using UnrealAgent.Backend.Conversation;
using UnrealAgent.Backend.Chat;

namespace UnrealAgent.Backend.Agent;

/// <summary>
/// 에이전트 세션
/// 프로세스 아이덴티티, 대화 상태, 미들웨어 파이프라인을 통합한다
/// </summary>
public sealed class AgentSession(AgentLoop Loop)
{
    // 세션의 대화 히스토리
    public Conversation.Conversation Conversation { get; } = new();
    
    // 사용자 메세지를 처리한다
    public IAsyncEnumerable<ChatEvent> ProcessMessage(UserInput Input) => Loop.RunAsync(Input, this);
}