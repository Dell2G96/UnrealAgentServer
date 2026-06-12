namespace UnrealAgent.Backend.Core;

// 프로젝트 경로를 제공하는 정적 클래스
public static class AgentPaths
{
    public static readonly string UserConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".unrealagent");
}