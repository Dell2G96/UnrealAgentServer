using System.Text.Json;
using UnrealAgent.Backend.Agent;

namespace UnrealAgent.Backend.Tool.Tools;

/// -----------------------------------------------------------------
/// IAgent Tool
/// -----------------------------------------------------------------

// 에이전트 도구 실행 인터페이스
// [AgentTool] 어트리뷰트와 함꼐 구현하면 ToolRegistry가 자동 스캔한다
public interface IAgentTool
{
    // 도구를 실행하고 결과를 반환한다
    Task<ToolResult> ExecuteAsync(string InputJson, AgentSession Session, CancellationToken  Ct = default);
}

//-----------------------------------------------------------------------------
// AgentTool<TInput>
//-----------------------------------------------------------------------------

/*
 * 타입 안전한 도구 기본 클래스
 * JSON 입력을 TInput 레코드로 자동 역직렬화 한다
 *
 * public sealed record Input(
 *     [property: JsonPropertyName("city")]
 *     [property: Description("날씨를 조회할 도시 이름 (예: 'Seoul', 'Busan')")]
 *     string City,
 *
 *    [property: JsonPropertyName("unit")]
 *   [property: Description("온도 단위: 'celsius' 또는 'fahrenheit' (기본값: celsius)")]
 *   string? Unit = null);
 */

public abstract class AgentTool<TInput> : IAgentTool
{
    // JSON 문자열을 TInput으로 역직렬화 하여 실행
    public Task<ToolResult> ExecuteAsync(string InputJson, AgentSession Session, CancellationToken Ct = default)
    {
        TInput Input = JsonSerializer.Deserialize<TInput>(InputJson) ?? throw new ArgumentException($"Failed to deserialize {typeof(TInput).Name}.");
        return ExecuteAsync(Input, Session, Ct);
    }
    
    // 타입 안전한 도구 실행 매서드
    protected abstract Task<ToolResult> ExecuteAsync(TInput Input, AgentSession Session, CancellationToken Ct);
    
}
