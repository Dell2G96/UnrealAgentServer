using UnrealAgent.Backend.Core;

namespace UnrealAgent.Backend.Security;


/// <summary>
/// 도구 실행 권한 엔진
/// </summary>
public sealed class PermissionEngine
{
    // 항상 허용 되는 도구 이름 집합
    private readonly HashSet<string> AllowedTools = new(StringComparer.OrdinalIgnoreCase);

    // 도구를 허용 목록에 추가한다
    public void Allow(string ToolName) => AllowedTools.Add(ToolName);

    // 도구 호출의 실행 권한을 조회한다
    public Task<ToolPermission> GetPermissionAsync(Block.ToolUse ToolCall)
    {
        // 허용 목록 확인
        if(AllowedTools.Contains(ToolCall.Name))
            return Task.FromResult(ToolPermission.Allow);
        
        // 사용자에게 확인을 요청
        return Task.FromResult(ToolPermission.Ask);
    }

    //
}