using System.Globalization;
using System.Text;
using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Command.Attributes;
using UnrealAgent.Backend.Token;

namespace UnrealAgent.Backend.Command.Commands;

// 토큰 사용량, 예상 비용, 추정 잔액을 보여준다
// /usage            → 현황 보기
// /usage set 37.5   → 콘솔에서 확인한 실제 잔액으로 맞추기
// /usage add 20     → 추가 구매한 금액을 잔액에 더하기
[AgentCommand("/usage", "토큰 사용량·예상 비용·추정 잔액 보기 (/usage set 잔액, /usage add 구매액)", icon:"payments")]
public class UsageCommand(UsageTracker Tracker) : IAgentCommand
{
    public async IAsyncEnumerable<ChatEvent> ExecuteAsync(string[] Args, AgentSession Session)
    {
        if (Args.Length == 0)
        {
            yield return new ChatEvent.System(BuildReport());
            yield break;
        }

        // set / add 는 금액 인자가 반드시 필요하다
        string Sub = Args[0].ToLowerInvariant();
        if ((Sub != "set" && Sub != "add") || Args.Length < 2 || !TryParseAmount(Args[1], out double Amount))
        {
            yield return new ChatEvent.System("사용법: `/usage`, `/usage set <콘솔 잔액>`, `/usage add <추가 구매액>` (단위: USD)");
            yield break;
        }

        if (Sub == "set")
        {
            Tracker.SetBalance(Amount);
            yield return new ChatEvent.System($"추정 잔액을 ${Amount:F2} 로 설정했습니다.");
        }
        else
        {
            double NewBalance = Tracker.AddBalance(Amount);
            yield return new ChatEvent.System($"${Amount:F2} 를 더했습니다. 추정 잔액: ${NewBalance:F2}");
        }
    }

    // "$20", "20", "37.5" 모두 받는다
    private static bool TryParseAmount(string Text, out double Amount) =>
        double.TryParse(Text.TrimStart('$'), NumberStyles.Float, CultureInfo.InvariantCulture, out Amount) && Amount >= 0;

    // 현황 문자열을 만든다
    private string BuildReport()
    {
        StringBuilder Sb = new();
        Sb.AppendLine("이번 실행 사용량 (서버 시작 이후, 모델 단가 기준 추정)");
        Sb.AppendLine($"- API 호출: {Tracker.CallCount}회");
        Sb.AppendLine($"- 입력: {Tracker.InputTokens:N0} 토큰 (캐시 쓰기 {Tracker.CacheWriteTokens:N0} / 캐시 읽기 {Tracker.CacheReadTokens:N0})");
        Sb.AppendLine($"- 출력: {Tracker.OutputTokens:N0} 토큰");
        Sb.AppendLine($"- 예상 비용: ${Tracker.CostUsd:F4}");
        Sb.AppendLine();

        if (Tracker.GetBalance() is { } Balance)
            Sb.AppendLine($"추정 잔액: ${Balance:F2} (이 앱에서 쓴 비용만 차감됨 — 오차는 `/usage set <콘솔 잔액>` 으로 보정)");
        else
            Sb.AppendLine("추정 잔액: 미설정 — 콘솔의 잔액을 `/usage set <금액>` 으로 입력하세요");

        return Sb.ToString();
    }
}
