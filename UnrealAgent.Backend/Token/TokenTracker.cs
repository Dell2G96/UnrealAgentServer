using Anthropic.Models.Messages;
using UnrealAgent.Backend.Auth;
using UnrealAgent.Backend.Model;
using UnrealAgent.Backend.Prompt;
using UnrealAgent.Backend.Tool.Tools;

namespace UnrealAgent.Backend.Token;


/// <summary>
/// 시스템 프롬프트와 도구 정의 고정 토큰을 측정
/// 모델별로 캐싱
/// </summary>
public class TokenTracker(AuthConfig Auth, PromptBuilder PromptBuilder, ToolRegistry ToolRegistry, ModelSettings ModelSettings)
{
    // 고정 토큰 측정 값
    public sealed record FixedTokens(long SystemPrompt, long UnrealAgentMd, long Skills, long Tools);

    // 현재 고정 토큰 측정 값
    public FixedTokens? Fixed { get; private set; }

    // 카테고리별 컨텍스트 사용량을 계산
    public TokenUsage GetTokenUsage(long ContextTokens)
    {
        long SystemTokens = Fixed?.SystemPrompt ?? 0;
        long UnrealAgentMdTokens = Fixed?.UnrealAgentMd ?? 0;
        long SkillTokens = Fixed?.Skills ?? 0;
        long Tools = Fixed?.Tools ?? 0;
        long Messages = Math.Max(0, ContextTokens - SystemTokens - UnrealAgentMdTokens - SkillTokens - Tools);

        return new TokenUsage(SystemTokens, UnrealAgentMdTokens, SkillTokens, Tools, Messages, ModelSettings.ContextWindow);
    }
    
    // Count Tokens API 로 고정 토큰을 측정한다
    public async Task MeasureAsync()
    {
        if (Auth.Client is null || Fixed is not null)
            return;

        List<MessageParam> DummyMessages =
        [
            new() { Role = Role.User, Content = "." }
        ];
        
        // 1 - 기준선 : 더미 메시지만 포함
        MessageTokensCount Baseline = await Auth.Client.Messages.CountTokens(new MessageCountTokensParams
        {
            Model = ModelSettings.Model,
            Messages = DummyMessages
        });
        
        // 2 - 시스템 프롬프트만
        MessageTokensCount SystemOnly = await Auth.Client.Messages.CountTokens(new MessageCountTokensParams
        {
            Model = ModelSettings.Model,
            Messages = DummyMessages,
            System = PromptBuilder.BuildWithout(PromptBuilder.Section.UnrealAgentMd | PromptBuilder.Section.Skills)
        });

        // 3- UnrealAgent.md 만
        MessageTokensCount MdOnly = await Auth.Client.Messages.CountTokens(new MessageCountTokensParams
        {
            Model = ModelSettings.Model,
            Messages = DummyMessages,
            System = PromptBuilder.BuildOnly(PromptBuilder.Section.UnrealAgentMd)
        });
           
        // 4 - Skills 만
        MessageTokensCount SkillsOnly = await Auth.Client.Messages.CountTokens(new MessageCountTokensParams
        {
            Model = ModelSettings.Model,
            Messages = DummyMessages,
            System = PromptBuilder.BuildOnly(PromptBuilder.Section.Skills)
        });
        
        // 5 - 도구만
        MessageTokensCount ToolsOnly = await Auth.Client.Messages.CountTokens(new MessageCountTokensParams
        {
            Model = ModelSettings.Model,
            Messages = DummyMessages,
            Tools = ToolRegistry.GetAllSchemas().Select(S => (MessageCountTokensTool)S).ToList()
        });
        
        Fixed = new FixedTokens(
            SystemPrompt: SystemOnly.InputTokens - Baseline.InputTokens,
            UnrealAgentMd: MdOnly.InputTokens - Baseline.InputTokens,
            Skills: SkillsOnly.InputTokens - Baseline.InputTokens,
            Tools: ToolsOnly.InputTokens - Baseline.InputTokens);
    }
}