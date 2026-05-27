namespace UnrealAgent.Backend.Tool.Tools;

// 도구 실행 결과
// param name = "bIsSuccess" 실행 성공 여부
// parma name = "Content" 실행 결과 또는 에러메시지
public sealed class ToolResult(bool bIsSuccess, string Content)
{
    // 성공 결과를 생성
    public static ToolResult Success(string Content) => new(true, Content);
    
    // 에러 결과를 생성 , Error : 접두사 없이 원문 그대로 저장
    public static ToolResult Error(string Error) => new(false, Error);
}