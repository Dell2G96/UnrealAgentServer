namespace UnrealAgent.Backend.Conversation;
/// <summary>
///  사용자 입력 메세지. 텍스트와 첨부 이미지를 포함한다
/// </summary>
public sealed record UserInput(string Text, string? ImageMediaType = null, string? ImageBase64 = null)
{
    public static implicit operator UserInput(string Text) => new(Text);

    // 첨부 이미지가 있는지 여부
    public bool HasImage =>
        !string.IsNullOrWhiteSpace(ImageMediaType) &&
        !string.IsNullOrWhiteSpace(ImageBase64);
}
