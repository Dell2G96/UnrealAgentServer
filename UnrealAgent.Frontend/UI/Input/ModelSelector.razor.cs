using Microsoft.AspNetCore.Components;
using UnrealAgent.Backend.Model;

namespace UnrealAgent.Frontend.UI.Input;

public partial class ModelSelector
{
    // 모델 설정 서비스
    [Inject] private ModelSettings Settings { get; set; } = null!;
    
    // 모델 레지스트리 서비스
    [Inject] private ModelRegistry Registry { get; set; } = null!;
    
    // 드롭다운 열림 상태
    private bool bIsOpen;

    // 드롭다운 열거나 닫는다
    private void ToggleDropdown() => bIsOpen = !bIsOpen;

    // 현재 모델의 아이콘 글자
    private string ModelIcon => Settings.DisplayName.Length > 0
        ? Settings.DisplayName[0].ToString()
        : "U";

    // 모델을 선택하고 드롭다운을 닫는다
    private void SelectModel(IModel Model)
    {
        Settings.Select(Model);
        bIsOpen = false;
    }
    
    // 모델별 아이콘 배경색입니다
    private static string GetIconBg(IModel Model) => Model.DisplayName[0] switch
    {
        'O' => "bg-[#444]",
        'S' => "bg-[#333]",
        _ => "bg-[#2a2a2a]"
    };
    
    // 모델별 아이콘 글자색
    private static string GetIconColor(IModel Model) => Model.DisplayName[0]
    switch
    {
        'O' => "text-[#e0e0e0]",
        'S' => "text-[#aaa]",
        _ => "text-[#888]"
    };

}