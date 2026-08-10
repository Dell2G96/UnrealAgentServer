namespace UnrealAgent.Backend.Command.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AgentCommandAttribute(string name, string description, string icon = "terminal") : Attribute
{
    // 슬래시 커맨드 이름
    public string Name { get; } = name; 

    // 사용자에게 표시할 커맨드 설명
    public string Description { get; } = description;

    // Material Symbols 아이콘 이름
    public string Icon { get; } = icon;
}