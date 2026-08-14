using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UnrealAgent.Backend.Skill;

/// <summary>
/// Skill.md 파일을 파싱하여 SkillDefinition 을 생성한다.
/// YAML 프론트패머을 YamlDotNet으로 파싱하고 마크다운 본문을 분리한다 
/// </summary>
public static partial class SkillLoader
{
    // YAML 프론트매터를 추출하는 정규식
    [GeneratedRegex(@"^---\s*\n([\s\S]*?)---\s*\n?", RegexOptions.None)]
    private static partial Regex FrontmatterRegex();
    
    // YAML 디시리얼라이저 , kebab-case 키를 지원
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Skill.md 파일 경로에서 SkillDefinition을 파싱 
    /// </summary>
    public static SkillDefinition? Load(string FilePath)
    {
        if (!File.Exists(FilePath))
            return null;

        string RawContent = File.ReadAllText(FilePath);
        string SkillRoot = Path.GetDirectoryName(FilePath);
        
        // 프론트매터와 본문을 분리
        SkillFrontmatter Frontmatter = Parsefrontmatter(RawContent, out string Body);

        string Name = Frontmatter.Name!;
        string Description = Frontmatter.Description!;

        SkillDefinition Definition = new()
        {
            Name = Name,
            Description = Description,
            bDisableModelInvocation = Frontmatter.DisableModelInvocation,
            bUserInvocable = Frontmatter.UserInvocable,
            Content = Body.Trim(),
            SkillRoot = SkillRoot!,
        };

        return Definition;
    }

    // YAML 프론트매터를 파싱하고 본문을 분리하고
    private static SkillFrontmatter Parsefrontmatter(string RawContent, out string Body)
    {
        Match Match = FrontmatterRegex().Match(RawContent);
        if (!Match.Success)
        {
            Body = RawContent;
            return new SkillFrontmatter();
        }

        Body = RawContent[Match.Length..];
        string Yaml = Match.Groups[1].Value;

        try
        {
            return YamlDeserializer.Deserialize<SkillFrontmatter>(Yaml);
        }
        catch
        {
            return new SkillFrontmatter();
        }
    }
}


//-----------------------------------------------------------------------------
// SkillFrontmatter
//-----------------------------------------------------------------------------

/// <summary>
/// SKILL.md YAML 프론트매터의 역직렬화 대상 클래스입니다.
/// YamlDotNet의 HyphenatedNamingConvention으로 kebab-case 키를 자동 매핑합니다.
/// </summary>

internal sealed class SkillFrontmatter
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool DisableModelInvocation { get; set; }
    public bool UserInvocable { get; set; } = true;
}