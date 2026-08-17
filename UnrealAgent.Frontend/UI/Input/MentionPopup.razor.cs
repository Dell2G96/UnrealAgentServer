
using Microsoft.JSInterop;
using UnrealAgent.Backend.Mention;
using UnrealAgent.Frontend.Infrastructure;

namespace UnrealAgent.Frontend.UI.Input;

public partial class MentionPopup : JsComponentBase
{
    
    // 팝업 표시 여부
    public bool bShowPopup { get; private set; }
    
    //현재 선택된 인덱스
    private int SelectedIndex;

    // 멘션 항목 목록
    private List<MentionItem> Items = [];

    // 현재 탐색중인 경로, 빈 문자열이면 프로젝트 루트
    private string BasePath = "";

    // 전체 입력 텍스트로 팝업을 갱신,
    // 마지막 @을 찾아 멘션 쿼리를 추출
    public void Update(string Text)
    {
        // 마지막 @를 찾는다
        int AtIndex = Text.LastIndexOf('@');

        if (AtIndex < 0)
        {
            if (bShowPopup)
                Close();

            return;
        }

        string Query = Text[(AtIndex + 1)..];
        
        // 쿼리에 공백이 있으면 완료된 멘션
        if (Query.Contains(' '))
        {
            if (bShowPopup)
                Close();

            return;
        }
        
        // @ 뒤 쿼리로 항목을 갱신
        int LastSlash = Query.LastIndexOf('/');
        
        // @Source/My : Source 폴더 안에서 My 필터
        if (LastSlash >= 0)
        {
            BasePath = Query[..LastSlash];
            string Filter = Query[(LastSlash + 1)..];
            Items = MentionProvider.ListItems(BasePath, Filter);
        }
        
        // @So : 프로젝트 전체 재귀 검색
        else if (Query.Length >= 2)
        {
            BasePath = "";
            Items = MentionProvider.SearchItem(Query);
        }
        // @ : 루트 폴더 목록 표시
        else
        {
            BasePath = "";
            Items = MentionProvider.ListItems("", Query);
        }

        bShowPopup = Items.Count > 0 || !string.IsNullOrEmpty(BasePath);
        SelectedIndex = 0;
        StateHasChanged();
    }
    
    // 방향키로 항목을 탐색
    public async Task Navigate(int Direction)
    {
        if (!bShowPopup || Items.Count == 0)
            return;
        SelectedIndex = (SelectedIndex + Direction + Items.Count) % Items.Count;
        await Module.InvokeVoidAsync("scrollToItem", "mention-item", SelectedIndex);
        
        StateHasChanged();
    }
    
    // Enter 키로 최종 선택
    // 폴더/파일 경로르 반환하고 팝업을 닫는다.
    public string? Select()
    {
        if (!bShowPopup || SelectedIndex < 0 || SelectedIndex >= Items.Count)
            return null;

        string Path = Items[SelectedIndex].RelativePath;
        Close();

        return Path;
    }
    
    /// Tab 키로 폴더면 드릴다운, 파일이면 선택합니다.
    /// 드릴다운 시 "경로/"를 반환합니다.
    public string? Tab()
    {
        if (!bShowPopup || SelectedIndex < 0 || SelectedIndex >= Items.Count)
            return null;

        MentionItem Selected = Items[SelectedIndex];

        if (Selected.Kind == MentionItemKind.Folder)
            return Selected.RelativePath + "/";

        Close();
        return Selected.RelativePath;
    }
        
    /// ← 키로 상위 폴더 쿼리를 반환합니다.
    /// BasePath가 없으면 null을 반환합니다.
    public string? GoBack()
    {
        if (string.IsNullOrEmpty(BasePath))
            return null;

        int LastSlash = BasePath.LastIndexOf('/');
        return LastSlash >= 0 ? BasePath[..LastSlash] + "/" : "";
    }
    
    
    /// <summary>
    ///  팝업을 닫고, 상태를 초기화
    /// </summary>
    public void Close()
    {
        bShowPopup = false;
        BasePath = "";
        Items = [];
        SelectedIndex = 0;
        
        StateHasChanged();
    }
}