using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Command.Attributes;
using UnrealAgent.Backend.Conversation;

namespace UnrealAgent.Backend.Command.Commands;

[AgentCommand("/clear", "대화 내역 초기화 (이전 대화는 /resume 으로 다시 열 수 있음)", icon:"restart_alt")]
public class ClearCommand(SessionStore SessionStore) : IAgentCommand
{
    // 대화를 비우고 새 세션 파일로 전환한다. 이전 세션 파일은 남겨 둔다
    public async IAsyncEnumerable<ChatEvent> ExecuteAsync(string[] Args, AgentSession Session)
    {
        Session.Conversation.Clear();
        SessionStore.StartNew();

        yield return new ChatEvent.Command("clear", "");
    }
    
}