namespace UnrealAgent.Backend.Chat;

/// <summary>
/// UI에 표시되는 채팅 메시지입니다.
/// </summary>
public abstract record class ChatUIMessage
{
    // 메세지 본문
    public abstract string Content { get; init; }
    
    // 메세지 생성 시간
    public DateTime Timestamp { get; init; } = DateTime.Now;

    // Content 뒤에 텍스트를 이어붙인 새 인스턴스를 반환한다
    public ChatUIMessage Append(string Text) => this with { Content = Content + Text };
    
    //-----------------------------------------------------------------------------
    // User
    //-----------------------------------------------------------------------------

    // 사용자 메세지
    public sealed record User(string Content) : ChatUIMessage
    {
        
    }
    
    
    
    //-----------------------------------------------------------------------------
    // Assistant
    //-----------------------------------------------------------------------------
    
    // 어시스트 AI 응답
    public sealed record Assistant(string Content) : ChatUIMessage;


    //-----------------------------------------------------------------------------
    // Thinking
    //-----------------------------------------------------------------------------
    
    // 사고 과정 (Extended Thinking) 메시지
    public sealed record Thinking(string Content) : ChatUIMessage
    {
        // 사고 직전 시간 UI에서 실시간 경과 시간을 계산
        public DateTime StartTime { get; init; }
        
        // 사고 과정에 소요된 최종 시간(초) 완료 후 확정
        public double ElapsedSeconds { get; init; }
        
        // 완료 여부
        public bool bIsCompleted { get; init; }
    }
    
    //-----------------------------------------------------------------------------
    // System
    // -----------------------------------------------------------------------------
    
    // 시스템 메세지
    public sealed record System(string Content) : ChatUIMessage;
}