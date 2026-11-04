using System.Text.Json;
using System.Text.Json.Nodes;
using UnrealAgent.Backend.Core;
using UnrealAgent.Backend.Model;

namespace UnrealAgent.Backend.Token;

/// <summary>
/// API 토큰 사용량과 예상 비용을 누적하는 싱글톤 (/usage 커맨드에서 조회)
/// - 이번 실행 사용량 : 서버가 켜진 뒤부터 메모리에 누적 (서버를 끄면 초기화)
/// - 추정 잔액       : ~/.unrealagent/Budget.json 에 저장. 사용자가 /usage set, /usage add 로 입력하고
///                     API 호출마다 예상 비용만큼 차감한다
/// Anthropic API는 크레딧 잔액 조회 기능이 없으므로 잔액은 어디까지나 추정치이다
/// </summary>
public sealed class UsageTracker
{
    // 캐시 단가 배율 (입력 단가 기준). 캐시 쓰기 1.25배(5분 캐시), 캐시 읽기 0.1배
    private const double CacheWriteMultiplier = 1.25;
    private const double CacheReadMultiplier = 0.1;

    // 잔액 파일 경로
    private readonly string BudgetPath = Path.Combine(AgentPaths.UserConfigDir, "Budget.json");

    // JSON 직렬화 옵션
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    // 여러 스레드(에이전트 루프, 커맨드)에서 동시에 접근하므로 잠금 사용
    private readonly Lock SyncRoot = new();

    // --- 이번 실행 누적값 ---
    public int CallCount { get; private set; }
    public long InputTokens { get; private set; }
    public long CacheWriteTokens { get; private set; }
    public long CacheReadTokens { get; private set; }
    public long OutputTokens { get; private set; }
    public double CostUsd { get; private set; }

    // API 호출 1회의 사용량을 누적하고, 잔액이 설정되어 있으면 비용만큼 차감한다
    public void Record(IModel Model, ApiCallUsage Usage)
    {
        double Cost = CalculateCost(Model, Usage);

        lock (SyncRoot)
        {
            CallCount++;
            InputTokens += Usage.InputTokens;
            CacheWriteTokens += Usage.CacheWriteTokens;
            CacheReadTokens += Usage.CacheReadTokens;
            OutputTokens += Usage.OutputTokens;
            CostUsd += Cost;

            // 다른 프로세스(팀원 에이전트)도 같은 파일을 쓰므로 매번 다시 읽고 차감한다
            if (LoadBalance() is { } Balance)
                SaveBalance(Balance - Cost);
        }
    }

    // 추정 잔액. 한 번도 설정하지 않았으면 null
    public double? GetBalance()
    {
        lock (SyncRoot)
            return LoadBalance();
    }

    // 잔액을 콘솔에서 확인한 실제 값으로 맞춘다 (/usage set 37.5)
    public void SetBalance(double Amount)
    {
        lock (SyncRoot)
            SaveBalance(Amount);
    }

    // 추가 구매한 금액을 잔액에 더한다 (/usage add 20). 잔액이 없으면 그 금액이 잔액이 된다
    public double AddBalance(double Amount)
    {
        lock (SyncRoot)
        {
            double NewBalance = (LoadBalance() ?? 0) + Amount;
            SaveBalance(NewBalance);
            return NewBalance;
        }
    }

    // 모델 단가로 API 호출 1회의 예상 비용(USD)을 계산한다
    private static double CalculateCost(IModel Model, ApiCallUsage Usage)
    {
        double InputCost = Usage.InputTokens * Model.InputPricePerMTok
                         + Usage.CacheWriteTokens * Model.InputPricePerMTok * CacheWriteMultiplier
                         + Usage.CacheReadTokens * Model.InputPricePerMTok * CacheReadMultiplier;
        double OutputCost = Usage.OutputTokens * Model.OutputPricePerMTok;

        return (InputCost + OutputCost) / 1_000_000;
    }

    // 파일에서 잔액을 읽는다. 파일이 없거나 값이 없으면 null
    private double? LoadBalance()
    {
        if (!File.Exists(BudgetPath))
            return null;

        JsonNode? Root = JsonNode.Parse(File.ReadAllText(BudgetPath));
        return Root?["balance_usd"]?.GetValue<double>();
    }

    // 잔액을 파일에 저장한다. 디렉토리가 없으면 생성
    private void SaveBalance(double Balance)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(BudgetPath)!);

        JsonObject Root = new() { ["balance_usd"] = Balance };
        File.WriteAllText(BudgetPath, Root.ToJsonString(JsonOptions));
    }
}
