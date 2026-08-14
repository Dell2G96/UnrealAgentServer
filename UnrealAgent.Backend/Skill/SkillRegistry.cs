using System.Text;
using UnrealAgent.Backend.Core;

namespace UnrealAgent.Backend.Skill;

/// <summary>
/// 파일시스템에서 Skill.md를 발견하고 관리한다.
/// 스킬 목록을 시슨템 프롬프트에 주입하고, 호출 시 본문을 반환한다.
/// </summary>
public sealed class SkillRegistry
{
    // 스킬 맵 , 이름 -> SkillDefinition
    private readonly Dictionary<string, SkillDefinition> Skills = new(StringComparer.OrdinalIgnoreCase);

    //스킬 디렉터리를 스캔하여 Skill.md 파일을 로딩한다
    public void DiscoverSkills()
    {
        if (!Directory.Exists(AgentPaths.SkillsDir))
        {
            return;
        }

        foreach (string SubDir in Directory.GetDirectories(AgentPaths.SkillsDir))
        {
            string SkillFile = Path.Combine(SubDir, "SKILL.MD");
            SkillDefinition? Skill = SkillLoader.Load(SkillFile);
            
            if(Skill is null)
                continue;

            Skills[Skill.Name] = Skill;
        }
        
    }

    // 시스템 프롬프트에 포함할 스킬목록을 생성한다.
    // disableModelInvocation인 스킬을 제외한다.
    public string? GetSkillListingPrompt()
    {
        List<SkillDefinition> Visible = Skills.Values
            .Where(S => !S.bDisableModelInvocation)
            .ToList();

        if (Visible.Count == 0)
            return null;

        StringBuilder Sb = new();

        foreach (SkillDefinition Skill in Visible)
            Sb.AppendLine($"- {Skill.Name}: {Skill.Description}");

        return Sb.ToString().TrimEnd();
    }

    /// <summary>
    /// 사용자 호출 가능한 스킬 목록을 반환한다 (UI 표시용)  
    /// </summary>
    public IReadOnlyList<SkillDefinition> GetUserInvocableSkills()
    {
        return Skills.Values
            .Where(S => S.bUserInvocable)
            .ToList();
    }

    /// <summary>
    /// 이름으로 스킬을 조회한다
    /// </summary>
    public SkillDefinition? GetSkill(string Name) => Skills.GetValueOrDefault(Name);

    /// <summary>
    /// 슬래시 입력이 등록된 스킬인지 확인한다
    /// </summary>
    public bool HasSkillSlash(string SlashName)
    {
        string Name = SlashName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0].TrimStart('/');
        return Skills.ContainsKey(Name);
    }

    /// <summary>
    /// 슬래시 입력을 파싱하여 스킬 Instruction을 생성한다
    /// </summary>
    public string? BuildInstructionFromSlash(string SlashName)
    {
        string Name = SlashName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0].TrimStart('/');
        if (!Skills.TryGetValue(Name, out SkillDefinition? Skill))
            return null;

        return Skill.BuildInstruction();
    }












}