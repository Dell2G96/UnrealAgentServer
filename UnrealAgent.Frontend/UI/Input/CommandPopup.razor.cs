using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UnrealAgent.Backend.Command;
using UnrealAgent.Frontend.Infrastructure;

namespace UnrealAgent.Frontend.UI.Input;

public partial class CommandPopup : JsComponentBase
{
    // 슬래시 커맨드를 등록, 실행하는 레지스트
    [Inject] private CommandRegistry CommandRegistry { get; set; } = null!;
    
    // 등록된 슬래시 커맨드 목록
    private IReadOnlyList<CommandRegistry.CommandEntry> Commands =>
        CommandRegistry.GetAll();
    
    // 팝업 표시 여부
    public bool bShowPopup { get; private set; }

    // 현재 선택된 인덱스
    private int SelectedIndex;

    // 팝업 항목, 커맨드와 스킬을 통합 표시한다.
    private sealed record PopupItem(string Name, string Description, string Icon);

    // 필터링된 항목 목록
    private List<PopupItem> FilteredItems = [];

    // 입력 텍스트에 따라 팝업 상태를 갱신한다.
    public void Update(string RawInputText)
    {
        string Text = RawInputText;
        
        // 공백 없는 슬래시 시작 텍스트만 후보로 처리한다.
        if(!Text.StartsWith('/') || Text.Contains(' '))
        {
            bShowPopup = false;
            StateHasChanged();
            return;
        }
        
        // '/' 제거
        string Query = Text[1..];
        
        // 필터링 후 Command 목록
        List<PopupItem> CommandItems = Commands
            .Where(C => C.Name[1..].Contains(Query, StringComparison.OrdinalIgnoreCase))
            .Select(C => new PopupItem(C.Name[1..], C.Description, C.Icon))
            .ToList();

        FilteredItems = CommandItems;
        bShowPopup = FilteredItems.Count > 0;
        SelectedIndex = 0;
        
        StateHasChanged();
    }

    // 방향키로 항목을 탐색한다.
    public async Task Navigate(int Direction)
    {
        if (!bShowPopup || FilteredItems.Count == 0)
            return;

        SelectedIndex = (SelectedIndex + Direction + FilteredItems.Count)
            % FilteredItems.Count;
        StateHasChanged();

        await Module.InvokeVoidAsync("scrollToItem", "popup-item", SelectedIndex);
    }

    // 현재 선택된 항목을 적용한다. 적용된 텍스트를 반환
    public string? Select()
    {
        if (!bShowPopup || SelectedIndex < 0 || SelectedIndex >= FilteredItems.Count)
            return null;

        string Result = "/" + FilteredItems[SelectedIndex].Name + " ";
        bShowPopup = false;
        StateHasChanged();

        return Result;
    }

    // 팝업을 닫는다.
    public void Close()
    {
        bShowPopup = false;
        StateHasChanged();
    }

}