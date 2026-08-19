namespace UnrealAgent.Backend.Token;


/// <summary>
/// 카테고리별 컨텍스트 토큰 사용량
/// Count Token API와 런타임 Usage를 조합하여 계산한다.
/// </summary>
public sealed record  TokenUsage(long SystemPrompt, long UnrealAgentMd, long Skills, long Tools, long Messages, long ContextWindow)
{
    // 총 입력 토큰 수 
    private long TotalInput => SystemPrompt + UnrealAgentMd + Skills + Tools + Messages;
    
    // 남은 토큰 수 
    public long FreeSpace => ContextWindow - TotalInput;
    
    // 컨텍스트 윈도우 사용률
    public double UsagePercent => ContextWindow > 0 ? (double)TotalInput / ContextWindow * 100 : 0;
}