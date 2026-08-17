namespace UnrealAgent.Backend.Mention;


/// <summary>
/// 멘션 항목의 종류
/// </summary>
public enum MentionItemKind
{
    Folder,
    File
}

/// <summary>
/// @ 멘션 팝업에 표시되는 개별 항목
/// </summary>
/// <param name="Name"> 파일 또는 폴더 이름</param>
/// <param name="RelativePath"> 프로젝트 루트 기준 상대 경로</param>
/// <param name="Kind"> 폴더 또는 파일인지 구분</param>
public record MentionItem(string Name, string RelativePath, MentionItemKind Kind);