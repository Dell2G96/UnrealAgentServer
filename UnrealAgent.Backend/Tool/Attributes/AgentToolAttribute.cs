namespace UnrealAgent.Backend.Tool.Tools;

// ToolRegistry가 자동 스캔하는 도구 마커 어트리뷰트
// Claude API에 전달할 도구 이름과 설명을 지정한다
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AgentToolAttribute(string name , string description) : Attribute
{
    // 클로드 API 에 전달할 도구 이름
    public string Name { get; } = name;
    
    
    // 클로드에게 보여줄 도구 설명
    public string Description { get; } = description;
}