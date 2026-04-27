using System.Text.Json;
using System.Text.Json.Nodes;

namespace UnrealAgent.Backend.Llm;

public sealed class OpenAiApiConfig
{
    private readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".unrealagent",
        "OpenAIAuthconfig.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string? ApiKey { get; private set; }

    public bool IsApiKeyConfigured() => !string.IsNullOrWhiteSpace(ApiKey);

    public void SetApiKey(string key)
    {
        ApiKey = key;
        Save();
    }

    public void Load()
    {
        if (!File.Exists(ConfigPath))
            return;

        string json = File.ReadAllText(ConfigPath);
        JsonNode? root = JsonNode.Parse(json);
        if (root is null)
            return;

        ApiKey = root["api_key"]?.GetValue<string>();
    }

    private void Save()
    {
        string dir = Path.GetDirectoryName(ConfigPath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        JsonObject root = new() { ["api_key"] = ApiKey };
        File.WriteAllText(ConfigPath, root.ToJsonString(JsonOptions));
    }
}
