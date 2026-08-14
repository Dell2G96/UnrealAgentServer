using System.ComponentModel;
using System.Text.Json.Serialization;
using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Skill;
using UnrealAgent.Backend.Tool.Attributes;

namespace UnrealAgent.Backend.Tool.Tools;

[AgentTool("skill", """
                    Execute a skill within the main conversation.

                    When users ask you to perform tasks, check if any of the available skills match.
                    Skills provide specialized capabilities and domain knowledge.

                    How to invoke:
                    - Use this tool with the skill name and optional arguments
                    - Examples:
                      - skill: "build" - invoke the build skill

                    Important:
                    - When a skill matches the user's request, this is a BLOCKING REQUIREMENT:
                      invoke the relevant skill BEFORE generating any other response about the task
                    - NEVER mention a skill without actually calling this tool
                    - Do not invoke a skill that is already running
                    - Follow the returned instructions exactly
                    """)]
public class SkillTool(SkillRegistry SkillRegistry) : AgentTool<SkillTool.Input>
{
    public sealed record Input(
        [property: JsonPropertyName("skill")]
        [property: Description("The skill name")]
        string Skill);

    protected override Task<ToolResult> ExecuteAsync(Input Args, AgentSession Session, CancellationToken Ct)
    {
        SkillDefinition? Skill = SkillRegistry.GetSkill(Args.Skill);

        if (Skill is null)
            return Task.FromResult(ToolResult.Error($"Unknown skill: '{Args.Skill} '."));
        
        // 모델 자동 호출 차단 체크
        if (Skill.bDisableModelInvocation)
            return Task.FromResult(ToolResult.Error(
                $"Skill '{Args.Skill}' cannnot be invoked by the model. User must invoke it manually with / {Args.Skill}."));

        return Task.FromResult(ToolResult.Success(Skill.BuildInstruction()));

    }
}