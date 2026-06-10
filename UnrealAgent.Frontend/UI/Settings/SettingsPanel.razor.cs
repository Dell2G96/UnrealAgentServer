using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Auth;

namespace UnrealAgent.Frontend.UI.Settings;

public partial class SettingsPanel
{
    // 인증 설정
    [Inject] private AuthConfig Auth { get; set; } = null!;
    
    // 패널 표시 여부
    [Parameter] public bool bIsVisible { get; set; }
    
    // 패널 닫기
    [Parameter] public EventCallback OnClose { get; set; }
    
    // API 키 입력
    private string ApiKeyInput = "";

    // 상태 메시지
    private string StatusMessage = "";


    // 상태 메시지 CSS 클래스
    private string StatusCss = "";

    /// API 키 저장
    private void SaveApiKey()
    {
        if (string.IsNullOrWhiteSpace(ApiKeyInput))
        {
            StatusMessage = "Api 키를 입력하라";
            StatusCss = "text-[#e05e5e]";
            return;
        }
        
        Auth.SetApiKey(ApiKeyInput.Trim());
        ApiKeyInput = "";
        StatusMessage = "Api 키가 저장되었다";
        StatusCss = "text-[#4ba96c]";
    }
}