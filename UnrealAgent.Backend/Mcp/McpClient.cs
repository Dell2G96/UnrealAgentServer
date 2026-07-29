using System.Net.Http.Json;
using System.Text.Json;

namespace UnrealAgent.Backend.Mcp;

/// <summary>
/// HTTP를 통해 MCP 서버와 통신하는 클라이언트
/// initialize -> tools/list -> tools/call 흐름을 처리한다
/// </summary>
public class McpClient(HttpClient Http, string ServerName, string Url)
{
    // JSON-RPC 요청 ID 카운터
    private int NextId;
    
    // 이니셜라이즈 결과, 연결 후 서버 정보를 담고 있다
    public InitializeResult? ServerResult { get; private set; }
    
    // 서버가 도구를 지원하는지 여부
    public bool HasTools => ServerResult?.Capabilities.HasTools ?? false;
    
    //-------------------------------------------------------------------------
    // initialize 핸드셰이크
    //-------------------------------------------------------------------------

    
    /// <summary>
    /// MCP 서버에 Initialize 요청을 보내고 서버 정보를 받는다 
    /// </summary>
    public async Task<InitializeResult> InitializeAsync(CancellationToken Ct = default)
    {
        InitializeResult Result = await SendAsync<InitializeParams, InitializeResult>(
            "initialize",
            new InitializeParams(),
            Ct
        );

        ServerResult = Result;

        return Result;
    }
    
    //-------------------------------------------------------------------------
    // tools/list
    //-------------------------------------------------------------------------

    /// <summary>
    /// 서버에서 사용 가능한 도구 목록을 가져온다
    /// </summary>
    public async Task<List<McpToolDefinition>> ListToolsAsync(CancellationToken Ct = default)
    {
        ToolsListResult Result = await SendAsync<object, ToolsListResult>(
            "tools/list",
            new { },
            Ct
        );

        return Result.Tools;
    }
    
    //-------------------------------------------------------------------------
    // tools/call
    //-------------------------------------------------------------------------
    public async Task<ToolCallResult> CallToolAsync(string ToolName, JsonElement? Arguments,
        CancellationToken Ct = default)
    {
        return await SendAsync<ToolCallParams, ToolCallResult>(
            "tools/call",
            new ToolCallParams { Name = ToolName, Arguments = Arguments },
            Ct
        );
    }
    
    //-------------------------------------------------------------------------
    // 내부 통신
    //-------------------------------------------------------------------------
    private async Task<TResult> SendAsync<TParams, TResult>(string Method, TParams Params, CancellationToken Ct)
    {
        JsonRpcRequest Request = new()
        {
            Id = Interlocked.Increment(ref NextId),
            Method = Method,
            Params = Params
        };
        
        //언리얼 HTTP는 Content-Length를 필수적으로 요구하기 떄문에 
        // Content 를 사용해야 한다
        string JsonBody = JsonSerializer.Serialize(Request);
        using StringContent Content = new(JsonBody, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage HttpResponse = await Http.PostAsync(Url, Content, Ct);
        HttpResponse.EnsureSuccessStatusCode();

        JsonRpcRespone? RpcResponse = await HttpResponse.Content.ReadFromJsonAsync<JsonRpcRespone>(Ct);

        if (RpcResponse is null)
            throw new InvalidOperationException($"[{ServerName}] 빈 응답을 받았다");
        
        if (!RpcResponse.IsSuccess)
            throw new InvalidOperationException($"[{ServerName}] {RpcResponse.Error!.Message} (code: {RpcResponse.Error.Code})");

        return RpcResponse.Result!.Value.Deserialize<TResult>()
               ?? throw new InvalidOperationException($"[{ServerName}] result 역직렬화에 실패했습니다.");
    }
}


















