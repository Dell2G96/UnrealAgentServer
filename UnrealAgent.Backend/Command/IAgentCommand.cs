using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Chat;

namespace UnrealAgent.Backend.Command;

/// <summary>
/// 에이전트 커맨드 실행 인터페이스
/// [AgentCommand] 어트리뷰트와 함꼐 구현하면 CommandRegistry가 자동 스캔한다
/// </summary>
public interface IAgentCommand
{
    // 커맨드를 실행하고 결과 ChatEvent를 스트리밍한다
    IAsyncEnumerable<ChatEvent> ExecuteAsync(string[] Args, AgentSession Session);
}