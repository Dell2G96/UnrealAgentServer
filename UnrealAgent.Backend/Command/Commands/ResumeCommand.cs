using System.Text;
using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Command.Attributes;
using UnrealAgent.Backend.Conversation;

namespace UnrealAgent.Backend.Command.Commands;

// 저장된 세션 목록을 보여주고(/resume), 번호로 골라 이어서 시작한다(/resume 2)
[AgentCommand("/resume", "이전 세션 목록 보기, 번호로 이어서 시작 (예: /resume 2)", icon:"history")]
public class ResumeCommand(SessionStore SessionStore) : IAgentCommand
{
    // 목록에 보여줄 최대 세션 수
    private const int MaxListCount = 10;

    // 인자가 없으면 목록 출력, 번호가 있으면 해당 세션으로 대화와 화면을 교체한다
    public async IAsyncEnumerable<ChatEvent> ExecuteAsync(string[] Args, AgentSession Session)
    {
        List<SessionStore.SessionInfo> Sessions = SessionStore.List().Take(MaxListCount).ToList();

        if (Sessions.Count == 0)
        {
            yield return new ChatEvent.System("저장된 세션이 없습니다.");
            yield break;
        }

        if (Args.Length == 0)
        {
            yield return new ChatEvent.System(BuildList(Sessions));
            yield break;
        }

        if (!int.TryParse(Args[0], out int Number) || Number < 1 || Number > Sessions.Count)
        {
            yield return new ChatEvent.System($"잘못된 번호입니다: {Args[0]} (1 ~ {Sessions.Count})");
            yield break;
        }

        SessionStore.SessionInfo Target = Sessions[Number - 1];
        if (!SessionStore.Load(Target.Id, Session.Conversation))
        {
            yield return new ChatEvent.System($"세션 파일을 읽을 수 없습니다: {Target.Id}");
            yield break;
        }

        yield return new ChatEvent.Restore(SessionStore.BuildUIMessages(Session.Conversation));
        yield return new ChatEvent.System($"세션을 불러왔습니다: {Target.Title}");
    }

    // "1. [11-01 21:30] 제목 (5턴) ← 현재" 형식의 목록 문자열을 만든다
    private string BuildList(List<SessionStore.SessionInfo> Sessions)
    {
        StringBuilder Sb = new();
        Sb.AppendLine("저장된 세션 (최근 순) — `/resume 번호` 로 이어서 시작");

        for (int i = 0; i < Sessions.Count; i++)
        {
            SessionStore.SessionInfo Info = Sessions[i];
            string CurrentMark = Info.Id == SessionStore.CurrentId ? " ← 현재" : "";
            Sb.AppendLine($"{i + 1}. [{Info.UpdatedAt:MM-dd HH:mm}] {Info.Title} ({Info.TurnCount}턴){CurrentMark}");
        }

        return Sb.ToString();
    }
}
