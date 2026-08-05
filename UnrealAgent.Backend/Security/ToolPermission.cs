namespace UnrealAgent.Backend.Security;


/// <summary>
/// 도구 실행 권한 판정 결과
/// </summary>
public enum ToolPermission
{
    // 실행 허용
    Allow,
    // 실행 거부
    Deny,
    // 사용자에게 확인 요청
    Ask,
    // 이 도구를 항상 허용하도록 권한 엔진에 등록
    AlwaysAllow
}