namespace UnrealAgent.Backend.Tool.Attributes;

// ModelRegistry가 자동 스캔하는 모델 마커 어트리뷰트
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AgentModelAttribute : Attribute
{
    // 레거시 모델 여부
    public bool bIsLegacy { get; set; } = false;
    
    // 정렬 순서 , 낮을 수록 먼저 표시된다
    public int Order { get; set; } = 100;
}