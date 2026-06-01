namespace UnrealAgent.Backend.Tool;

// Result returned after an agent tool execution.
public sealed class ToolResult(bool isSuccess, string content)
{
    public bool IsSuccess { get; } = isSuccess;

    public string Content { get; } = content;

    public static ToolResult Success(string content) => new(true, content);

    public static ToolResult Error(string error) => new(false, error);
}
