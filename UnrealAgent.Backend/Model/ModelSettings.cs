using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic.Models.Messages;
using UnrealAgent.Backend.Core;

namespace UnrealAgent.Backend.Model;

/// <summary>
/// API 런타임 변경 싱글톤
/// 모델 변경 시 이 객체를 업데이트 하면 즉시 반영
/// 설정은 ~/.unrealagent/ModelSettings.Json에 자동 저장
/// </summary>

public sealed class ModelSettings(ModelRegistry Registry)
{
    // 설정 파일 경로
    private readonly string ConfigPath = Path.Combine(AgentPaths.UserConfigDir, "ModelSettings.json");

    // JSON 직렬화 옵션
    private static readonly JsonSerializerOptions JsonOptions = new() {WriteIndented = true};

    // 현재 선택된 모델 정의
    private IModel CurrentModel = new Models.Opus48();

    // 확장된 사고 활성화 여부 백킹 필드
    private bool ThinkingEnabled = true;

    // 사고 싶이 백킹필드
    private Effort currentEffort = Effort.High;

    // API 모델 ID 
    public string Model => CurrentModel.Id;

    // UI 표시 이름
    public string DisplayName => CurrentModel.DisplayName;
    
    // 모델 설명
    public string Description => CurrentModel.Description;

    // 최대 출력 토큰 수
    public int MaxTokens => CurrentModel.MaxOutputTokens;

    // 컨텍스트 윈도우 크기
    public int ContextWindow => CurrentModel.ContextWindow;

    // 현재 설정에 맞는 ThinkingConfigParam를 반환
    public ThinkingConfigParam GetThinking() => bThinkingEnabled? new ThinkingConfigAdaptive() : new ThinkingConfigDisabled();

    
    // 현재 설정에 맞는 Effort의 OutputCOnfig를 반환
    public OutputConfig GetEffort() => new() { Effort = Effort };
    
    // 모델을 변경
    public void Select(IModel ClaudeModel)
    {
        CurrentModel = ClaudeModel;
        Save();
    }
    
    // 확장된 사고 활성화 여부 
    public bool bThinkingEnabled
    {
        get => ThinkingEnabled;
        set { ThinkingEnabled = value; Save(); }
    }

    // Claude의 사고 싶이 thinking 과 독립적으로 동작
    public Effort Effort
    {
        get => currentEffort;
        set { currentEffort = value; Save(); }
    }
    
    
    // 현재 설정을 파일에 저장 
    private void Save()
    {
        string Dir = Path.GetDirectoryName(ConfigPath)!;
        if (!Directory.Exists(Dir))
            Directory.CreateDirectory(Dir);

        JsonObject Root = new()
        {
            ["model"] = Model,
            ["thinking_enabled"] = ThinkingEnabled,
            ["effort"] = currentEffort.ToString().ToLowerInvariant()
        };
        
        File.WriteAllText(ConfigPath, Root.ToJsonString(JsonOptions));
    }
    
    // 설정 파일에서 로드한다
    // ModelRegistry가 초기화된 후 호출해야한다

    public void Load()
    {
        if (!File.Exists(ConfigPath))
            return;

        string Json = File.ReadAllText(ConfigPath);
        JsonNode? Root = JsonNode.Parse(Json);
        if (Root is null)
            return;

        if (Root["model"]?.GetValue<string>() is { } ModelId && Registry.FindById(ModelId) is { } Found)
            CurrentModel = Found;

        if (Root["thinking_enabled"] is not null)
            ThinkingEnabled = Root["thinking_enabled"]!.GetValue<bool>();

        if (Root["effort"]?.GetValue<string>() is { } EffortStr &&
            Enum.TryParse<Effort>(EffortStr, true, out Effort ParsedEffort))
            currentEffort = ParsedEffort;
    }
}