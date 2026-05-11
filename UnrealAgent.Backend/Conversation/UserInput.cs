namespace UnrealAgent.Backend.Conversation;
/// <summary>
///  사용자 입력 메세지. 텍스트와 첨부 이미지를 포함한다
/// </summary>
public sealed record UserInput(string Text)
{
    public static implicit operator UserInput(string Text) => new(Text);
}