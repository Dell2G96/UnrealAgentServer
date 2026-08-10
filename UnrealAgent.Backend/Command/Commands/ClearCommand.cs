using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Command.Attributes;

namespace UnrealAgent.Backend.Command.Commands;

[AgentCommand("/clear", "대화 내역 초기화", icon:"restart_alt")]
public class ClearCommand : IAgentCommand
{
    public async IAsyncEnumerable<ChatEvent> ExecuteAsync(string[] Args, AgentSession Session)
    {
        Session.Conversation.Clear();

        yield return new ChatEvent.Command("clear", "");
    }
    
}