namespace UnrealAgent.Backend.Skill;

/// <summary>
/// Skill.md 에서 파싱된 스킬 정의
/// 프론트매터 메타데이터와 마크다운 본문을 포함한다.
/// </summary>
public sealed class SkillDefinition
{
    ///////////////////////////////////////////////////////////////////////////
    // 프론트매터 (시작 시 로딩)
    ///////////////////////////////////////////////////////////////////////////
    // 스킬 이름, 슬래시 커맨드 및 skill 도구에서 사용한다.
    public required string Name { get; init; }
    
    // 스킬 설명, 시스템 프롬프트에 포함되어 모델의 자동 호출 판단 기준이 된다
    public required string Description { get; init; }
    
    // true이면 모델의 자동 호출을 차단하고 사용자 수동 호출만 허용한다
    public bool bDisableModelInvocation { get; init; }
    
    // false 이면 사용자에게 숨시고 모델만 호출할 수 있다.
    public bool bUserInvocable { get; init; } = true;
    
    // Skll.md 파일이 위차한 디렉토리의 절대 경로
    public required string SkillRoot { get; init; }
    
    ///////////////////////////////////////////////////////////////////////////
    // 프론트매터 본문 (호출 시 로딩)
    ///////////////////////////////////////////////////////////////////////////
    
    // 스킬 프롬프트 본문 
    public required string Content { get; init; }
    
    // 스킬 본문을 system-reminder 형태로 반환한다.
    public string BuildInstruction()
    {
        return $"""
                <system-reminder>
                Skill '{Name}' has been invoked. Follow these instructions:

                {Content}
                </system-reminder>
                """;
    }
}