using System.Text.Json;
using System.Text.RegularExpressions;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Core;

namespace UnrealAgent.Backend.Conversation;

/// <summary>
/// 리더 대화를 {AgentPaths.SessionsDir}/{Id}.json (사용자 설정 폴더 하위)에 저장하고 불러온다.
/// 서버는 에디터와 함께 강제 종료되므로 종료 시점이 아니라 매 턴이 끝날 때마다 저장한다.
/// </summary>
public sealed class SessionStore
{
    // /resume 목록 표시용 세션 정보
    public sealed record SessionInfo(string Id, string Title, DateTime UpdatedAt, int TurnCount);

    // 파일 저장 형태. 이미지는 저장하지 않으므로 UserInput 대신 텍스트와 "이미지가 있었는지"만 보관한다
    public sealed record SavedSession(string Id, string Title, DateTime UpdatedAt, List<SavedSpan> Spans);
    public sealed record SavedSpan(string? UserText, bool bHadImage, List<SavedAssistant> Assistants);
    public sealed record SavedAssistant(List<Block> Blocks, List<AssistantSpan.ToolExecution> ToolExecutions, long InputTokens);

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    // 이미지가 첨부됐던 턴을 복원할 때 텍스트 앞에 붙이는 안내 문구
    private const string ImageOmittedText = "[첨부 이미지는 세션에 저장되지 않음]";

    // 목록 제목으로 쓸 첫 사용자 메시지 최대 길이
    private const int MaxTitleLength = 40;

    // 현재 대화가 저장될 세션 ID (파일 이름)
    public string CurrentId { get; private set; } = NewId();

    private static string NewId() => DateTime.Now.ToString("yyyyMMdd-HHmmss");

    // 세션 ID 형식 (yyyyMMdd-HHmmss). 이 형식만 파일 경로로 쓴다
    // "..\..\x" 같은 값으로 sessions 폴더 밖에 파일을 쓰는 것(경로 조작)을 막기 위함
    private static readonly Regex IdPattern = new(@"^[0-9]{8}-[0-9]{6}$");

    private static bool IsValidId(string Id) => IdPattern.IsMatch(Id);

    private static string GetPath(string Id) =>
        IsValidId(Id)
            ? Path.Combine(AgentPaths.SessionsDir, $"{Id}.json")
            : throw new ArgumentException($"잘못된 세션 ID: {Id}", nameof(Id));

    // 새 세션 ID로 전환한다 (/clear). 기존 세션 파일은 그대로 남는다
    public void StartNew() => CurrentId = NewId();

    // 현재 대화를 CurrentId 파일에 덮어써서 저장한다. 빈 대화는 저장하지 않는다
    public void Save(Conversation Conversation)
    {
        if (Conversation.Spans.Count == 0)
            return;

        List<SavedSpan> Spans = Conversation.Spans.Select(Span => new SavedSpan(
            Span.UserInput?.Text,
            Span.UserInput?.HasImage ?? false,
            Span.AssistantSpans.Select(A => new SavedAssistant(A.AssistantBlocks.ToList(), A.ToolExecutions.ToList(), A.InputTokens)).ToList()
        )).ToList();

        SavedSession Saved = new(CurrentId, MakeTitle(Conversation), DateTime.Now, Spans);

        Directory.CreateDirectory(AgentPaths.SessionsDir);
        File.WriteAllText(GetPath(CurrentId), JsonSerializer.Serialize(Saved, JsonOptions));
    }

    // 저장된 세션 목록을 최근 수정 순으로 반환한다
    public List<SessionInfo> List()
    {
        if (!Directory.Exists(AgentPaths.SessionsDir))
            return [];

        List<SessionInfo> Result = [];
        foreach (string FilePath in Directory.GetFiles(AgentPaths.SessionsDir, "*.json"))
        {
            // ID는 파일 내용이 아니라 파일 이름에서 가져온다 (내용은 조작될 수 있음)
            string Id = Path.GetFileNameWithoutExtension(FilePath);
            if (!IsValidId(Id))
                continue;

            // UpdatedAt이 미래 시각이면 조작되었거나 신뢰할 수 없는 파일로 보고 건너뛴다
            // (서버 시작 시 자동 복원되므로, 조작된 파일이 가짜 대화를 주입하는 것을 막기 위함)
            if (ReadFile(FilePath) is { } Saved && Saved.UpdatedAt <= DateTime.Now)
                Result.Add(new SessionInfo(Id, Saved.Title, Saved.UpdatedAt, Saved.Spans.Count));
        }

        return Result.OrderByDescending(S => S.UpdatedAt).ToList();
    }

    // 세션 파일을 읽어 대화 내역을 교체하고, 이후 저장 대상을 그 세션으로 바꾼다
    public bool Load(string Id, Conversation Conversation)
    {
        if (!IsValidId(Id) || ReadFile(GetPath(Id)) is not { } Saved)
            return false;

        Conversation.Restore(Saved.Spans.Select(ToMessageSpan));
        CurrentId = Id;
        return true;
    }

    // 가장 최근 세션을 불러온다 (서버 시작 시 자동 이어하기)
    public bool LoadLatest(Conversation Conversation)
    {
        SessionInfo? Latest = List().FirstOrDefault();
        return Latest is not null && Load(Latest.Id, Conversation);
    }

    // 대화 내역을 채팅 화면용 메시지 목록으로 변환한다 (ChatStore 복원용)
    public static List<ChatUIMessage> BuildUIMessages(Conversation Conversation)
    {
        List<ChatUIMessage> Messages = [];

        // 원래 시각은 저장하지 않으므로 복원 시각으로 표시 (비워두면 0001-01-01 → "12:00 AM")
        DateTime RestoredAt = DateTime.Now;

        foreach (MessageSpan Span in Conversation.Spans)
        {
            if (Span.UserInput is not null)
                Messages.Add(new ChatUIMessage.User(Span.UserInput.Text));

            foreach (AssistantSpan Assistant in Span.AssistantSpans)
            {
                foreach (Block Item in Assistant.AssistantBlocks)
                {
                    switch (Item)
                    {
                        case Block.Thinking Thinking:
                            Messages.Add(new ChatUIMessage.Thinking(Thinking.Content) { StartTime = RestoredAt, bIsCompleted = true });
                            break;

                        case Block.Text Text:
                            Messages.Add(new ChatUIMessage.Assistant(Text.Content));
                            break;

                        case Block.ToolUse ToolUse:
                            string Result = Assistant.ToolExecutions.FirstOrDefault(E => E.ToolUseId == ToolUse.Id)?.OutPut ?? "";
                            Messages.Add(new ChatUIMessage.Tool(ToolUse.Name, Result)
                            {
                                ToolUseId = ToolUse.Id,
                                Input = ToolUse.InputJson,
                                StartTime = RestoredAt,
                                bIsCompleted = true
                            });
                            break;
                    }
                }
            }
        }

        return Messages;
    }

    // 파일을 읽어 역직렬화한다. 손상된 파일은 null (목록에서 제외)
    private static SavedSession? ReadFile(string FilePath)
    {
        try
        {
            return JsonSerializer.Deserialize<SavedSession>(File.ReadAllText(FilePath), JsonOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // 저장 형태 → 도메인 MessageSpan 변환. 이미지가 있던 턴은 안내 문구를 붙인다
    private static MessageSpan ToMessageSpan(SavedSpan Saved)
    {
        UserInput? Input = Saved.UserText is null
            ? null
            : new UserInput(Saved.bHadImage ? $"{ImageOmittedText}\n{Saved.UserText}".Trim() : Saved.UserText);

        MessageSpan Span = new() { UserInput = Input };

        foreach (SavedAssistant SavedA in Saved.Assistants)
        {
            AssistantSpan Assistant = new() { AssistantBlocks = SavedA.Blocks, InputTokens = SavedA.InputTokens };
            Assistant.ToolExecutions.AddRange(SavedA.ToolExecutions);
            Span.AssistantSpans.Add(Assistant);
        }

        return Span;
    }

    // 첫 사용자 메시지로 목록 제목을 만든다. 요약(/compact)된 대화는 사용자 메시지가 없을 수 있다
    private static string MakeTitle(Conversation Conversation)
    {
        string? FirstText = Conversation.Spans.Select(S => S.UserInput?.Text).FirstOrDefault(T => !string.IsNullOrWhiteSpace(T));
        if (FirstText is null)
            return "(요약된 대화)";

        string OneLine = FirstText.ReplaceLineEndings(" ").Trim();
        return OneLine.Length > MaxTitleLength ? OneLine[..MaxTitleLength] + "…" : OneLine;
    }
}
