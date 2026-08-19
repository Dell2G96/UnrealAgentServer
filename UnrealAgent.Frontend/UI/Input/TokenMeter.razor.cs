using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Model;
using UnrealAgent.Backend.Token;

namespace UnrealAgent.Frontend.UI.Input;

public partial class TokenMeter
{
    // 토큰 추적기
    [Inject] private TokenTracker TokenTracker { get; set; } = null!;
    
    // 모델 설정
    [Inject] private ModelSettings ModelSettings { get; set; } = null!;
    
    // 현재 컨텍스트 토큰 수,
    // 부모에서 전달받아 변경 시 re-render를 트리거한다.
    [Parameter] public long ContextTokens { get; set; }
    
    // 카테고리별 컨텍스트 사용량
    private TokenUsage Usage => TokenTracker.GetTokenUsage(ContextTokens);
    
    // 현재 컨텍스트 사용률
    private double UsagePercent => Usage.UsagePercent;
    
    // 퍼센트 텍스트 색상
    // 40% 이하 녹색, 70% 이하 주황, 초과 시 빨강
    private string PercentColorClass => UsagePercent switch
    {
        <= 40 => "text-[#4ba96c]",
        <= 70 => "text-[#d68a51]",
        _ => "text-[#e05e5e]"
    };

    // 70% 초과 시 pulse 애니메이션을 추가
    private string PercentAnimClass => UsagePercent > 70 ? "animate-pulse" : "";
    
    // 바 색상
    private string BarColorClass => UsagePercent switch
    {
        <= 40 => "bg-[#4ba96c]",
        <= 70 => "bg-[#d68a51]",
        _ => "bg-[#e05e5e]"
    };

    // 바 그림자
    private string BarShadowClass => UsagePercent switch
    {
        <= 40 => "",
        <= 70 => "",
        _ => "shadow-[0_0_6px_rgba(224,94,94,0.3)]"
    };

    // 토큰 수를 축약 형식으로 표시
    private static string FormatTokens(long Tokens) => Tokens switch
    {
        >= 1_000_000 => $"{Tokens / 1_000_000.0:F1}M",
        >= 1_000 => $"{Tokens / 1_000.0:F1}k",
        _ => Tokens.ToString()
    };
    
    // 토큰 수와 컨텍스트 윈도우 대비 퍼센트를 함께 표시
    private string FormatTokensWithPct(long Tokens)
    {
        string Formatted = FormatTokens(Tokens);
        if (ModelSettings.ContextWindow <= 0) 
            return Formatted;
        
        double Percent = (double)Tokens / ModelSettings.ContextWindow * 100;
        return $"{Formatted} ({Percent:F1}%)";
    }
}