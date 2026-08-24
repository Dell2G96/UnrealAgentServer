using System.IO.Pipelines;
using UnrealAgent.Backend.Agent.Middleware;
using UnrealAgent.Backend.Conversation;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Mode;
using UnrealAgent.Backend.Security;

namespace UnrealAgent.Backend.Agent;

/// <summary>
/// 에이전트 세션
/// 프로세스 아이덴티티, 대화 상태, 미들웨어 파이프라인을 통합한다
/// </summary>
public sealed class AgentSession
{
    // 세션의 대화 히스토리
    public Conversation.Conversation Conversation { get; } = new();
    
    // 팀 정보
    public Team.Team Team { get; } = new();
    
    // 이 세션의 도구 실행 권한 엔진
    public PermissionEngine PermissionEngine { get; }
    
    // 현재 에지전트 모드
    public AgentMode Mode { get; set; } = AgentMode.Normal;
    
    // 미들웨어 체인을통해 메세지를 처리하는 파이프라인
    private readonly AgentPipeline Pipeline;

    public AgentSession(AgentLoop Loop, SlashCommandMiddleware SlashCommandMiddleware)
    {
        PermissionEngine = new PermissionEngine(Team);

        Pipeline = new AgentPipeline()
            .Use(SlashCommandMiddleware)
            .Run(Loop.RunAsync);
    }
    
    // 사용자 메세지를 처리한다
    public IAsyncEnumerable<ChatEvent> ProcessMessage(UserInput Input, CancellationToken Ct = default)
        => Pipeline.RunAsync(Input, this, Ct);
    

}