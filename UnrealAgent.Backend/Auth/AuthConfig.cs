using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic;

namespace UnrealAgent.Backend.Auth;

/// <summary>
/// API KEY 기반 인증 시스템
/// AuthConfig.Json 파일에서 키를 로드하고, AnthropicClient 를 생성
/// </summary>
public class AuthConfig
{
    /// 설정 파일 경로 /// </summary>
    private readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".unrealagent", "Authconfig.json");
    
    // JSON 직렬화 옵션
    private static readonly JsonSerializerOptions JsonOptions = new()
        { WriteIndented = true };
    
    // 저장된 API KEY
    public string? ApiKey { get; private set; }

    // 현재 인증 정보로 구성된 안트로픽 클라이언트
    public AnthropicClient? Client { get; private set; } 
    
    /// API Key 가 설정되었는지 확인
    public bool IsApiKeyConfigured() => !string.IsNullOrWhiteSpace(ApiKey);
    
    // API Key를 설정하고 파일에 저장
    public void SetApiKey(string Key)
    {
        ApiKey = Key;
        Save();
    }
    
    // 현재 설정을 파일에 저장. 디렉토리가 없으면 생성
    private void Save()
    {
        string Dir = Path.GetDirectoryName(ConfigPath)!;
        if (!Directory.Exists(Dir))
            Directory.CreateDirectory(Dir);
        
        JsonObject Root = new() {["api_key"] = ApiKey};
        File.WriteAllText(ConfigPath, Root.ToJsonString(JsonOptions));
        
        UpdateClient();
    }

    /// <summary>
    ///  설정 파일을 로드
    /// </summary>
    public void Load()
    {
        if(!File.Exists(ConfigPath))
            return;
        
        string Json = File.ReadAllText(ConfigPath);
        JsonNode? Root = JsonNode.Parse(Json);
        if (Root is null)
            return;
        
        ApiKey = Root["api_key"]?.GetValue<string>();
        UpdateClient();

    }

    /// API KEy 로 AntrhopicClient 생성
    private void UpdateClient()
    {
        Client = ApiKey is not null
            ? new AnthropicClient {ApiKey = ApiKey}
            : null;
    }
}