namespace UnrealAgent.Frontend.Page;

public partial class Chat
{
    // 설정 패널 표시 여부
    private bool bShowSettings;

    // 설정 패널을 토클한다
    private void ToggleSettings( ) => bShowSettings = !bShowSettings;
}