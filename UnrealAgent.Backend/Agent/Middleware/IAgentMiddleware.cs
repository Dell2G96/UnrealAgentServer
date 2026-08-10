using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Conversation;

namespace UnrealAgent.Backend.Agent.Middleware;

public delegate IAsyncEnumerable<ChatEvent> AgentDelegate(UserInput Input, AgentSession Session, CancellationToken Ct);

//-----------------------------------------------------------------------------
// AgentMiddleware
//-----------------------------------------------------------------------------

/// <summary>
/// 에이전트 파이프라인 미들웨어 기본 클래스
/// 요청 전후에 로직을 삽입하거나, 요청을 가로채서 단락할 수 있다.
/// </summary>
public abstract class IAgentMiddleware
{
    // 파이프라인의 다음 단계
    protected AgentDelegate Next { get; private set; } = null!;
    
    // 다음 단계를 설정한다. 
    // AgentPipeline이 빌드 시 호출한다
    internal void SetNext(AgentDelegate Delegate) => Next = Delegate;
    
    // 미들웨어 로직을 실행한다
    public abstract IAsyncEnumerable<ChatEvent>
        InvokeAsync(UserInput Input, AgentSession Session, CancellationToken Ct);
}