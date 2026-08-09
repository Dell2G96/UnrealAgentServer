namespace UnrealAgent.Backend.Mode;

/// <summary>
/// 에이전트 실행 모드
/// </summary>
public enum AgentMode
{
    // 일반적인 상태
    Normal,
    
    // 모든 도구가 자동 승인된다
    Edit,
    
    // 모든 도구가 차단되며, 계획적인 분석을 수행
    Plan
    
}