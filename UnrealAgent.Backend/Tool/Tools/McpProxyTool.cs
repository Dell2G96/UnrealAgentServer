using System.Text.Json;
using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Mcp;

namespace UnrealAgent.Backend.Tool.Tools;

/// <summary>
/// MCP 서버의 도구를 IAgentTool 로 래핑하는 프록시
/// 실행 시 McpClient를 통해 MCP 서버에 tools/call을 전달한다
/// </summary>
public sealed class McpProxyTool(McpClient Client, string OriginalName) : IAgentTool
{
    public async Task<ToolResult> ExecuteAsync(string InputJson, AgentSession Session, CancellationToken Ct = default)
    {
        // JSon 문자열 -> JsonElement 로 변환
        JsonElement? Arguments = string.IsNullOrWhiteSpace(InputJson)
            ? null
            : JsonDocument.Parse(InputJson).RootElement;
        
        // mCP 서버에 도구 실행 요청
        ToolCallResult Result = await Client.CallToolAsync(OriginalName, Arguments, Ct);
        
        // MCP 응답에서 텍스트 추출
        // text 가 아닌 블록(image 등)은 타입만 남겨서 내용이 통째로 사라지지 않게 한다
        string Text = string.Join("\n", Result.Content.Select(C => C is { Type: "text", Text: not null }
            ? C.Text
            : $"[{C.Type}]")
        );

        // 안트로픽 API 는 tool_result 의 content 가 비어 있으면 요청을 거부한다
        // (is_error 가 true 인 경우 특히 400 BadRequest)
        if (string.IsNullOrWhiteSpace(Text))
        {
            Text = Result.IsError
                ? $"MCP tool '{OriginalName}' failed without an error message."
                : $"MCP tool '{OriginalName}' returned no output.";
        }

        return Result.IsError
            ? ToolResult.Error(Text)
            : ToolResult.Success(Text);
    }
    
}
