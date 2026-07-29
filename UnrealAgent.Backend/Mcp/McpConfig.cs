using System.Text.Json;
using System.Text.Json.Serialization;
using UnrealAgent.Backend.Core;

namespace UnrealAgent.Backend.Mcp;


//-----------------------------------------------------------------------------
// McpConfig
//-----------------------------------------------------------------------------

/// <summary>
/// settings.local.json 에서 mcpServers 섹션을 로드한다
/// </summary>
public static class McpConfig
{
    public static Dictionary<string, McpServerConfig> Load()
    {
        string SettingsPath = Path.Combine(AgentPaths.ConfigDir, "settings.local.json");

        if (!File.Exists(SettingsPath))
            return new();

        string Json = File.ReadAllText(SettingsPath);
        using JsonDocument Doc = JsonDocument.Parse(Json);

        if (!Doc.RootElement.TryGetProperty("mcpServers", out JsonElement McpElement))
            return new();

        return McpElement.Deserialize<Dictionary<string, McpServerConfig>>() ?? new();
    }
}

//-----------------------------------------------------------------------------
// McpServerConfig
//-----------------------------------------------------------------------------

/// <summary>
/// MCP 서버 하나의 설정입니다.
/// </summary>
public sealed record McpServerConfig
(
    [property: JsonPropertyName("url")]
    string Url
);