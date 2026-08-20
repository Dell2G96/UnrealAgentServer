using System.Text.Json;
using Anthropic.Models.Messages;
using Block = UnrealAgent.Backend.Core.Block;

namespace UnrealAgent.Backend.Conversation;

/// <summary>
/// Claude API 대화 히스토리를 전달
/// MessageSpan 기반으로 사용자 턴과 API 호출 결과를 구조화하여 저장합니다.
/// </summary>
public sealed class Conversation
{

    // 메세지 구간(사용자 1턴) 목록
    private readonly List<MessageSpan> MessageSpans = [];
    
    // 마지막 AssistantSpan의 입력 토큰 수 
    // 현재 컨텍스트 윈도우 사용량을 나타낸다.
    public long ContextTokens => MessageSpans.SelectMany(E => E.AssistantSpans).LastOrDefault()?.InputTokens ?? 0;

    // 26.05.11 - Claude/OpenAI 양쪽에서 동일한 대화 히스토리를 변환할 수 있도록 읽기 전용 노출
    public IReadOnlyList<MessageSpan> Spans => MessageSpans;

    // 첫번쨰 사용자 메세지 텍스트를 반환한다.
    // 빌링 헤더 생성에 사용됨
    public string GetFirstUserText() => MessageSpans.FirstOrDefault()?.UserInput?.Text ?? "";

    // MessageSpan를 추가하고 반환한다
    public MessageSpan AddMessageSpan(UserInput Input)
    {
        MessageSpan MessageSpan = new() { UserInput = Input };
        MessageSpans.Add(MessageSpan);

        return MessageSpan;
    }

    // 도메인 모델을 안트로픽 API 메세지 형식으로 반환
    // API는 user <-> assistant 교대를 요구하며, tool_result는 user role 로 전송
    // ex) [user] -> [assistant: text + tool_use] -> [user: tool_result] -> [assistant : text] 
    public List<MessageParam> ToAnthropicMessages()
    {
        List<MessageParam> Messages = [];

        foreach (MessageSpan MessageSpan in MessageSpans)
        {
            // user 메세지
            if (MessageSpan.UserInput is not null)
                Messages.Add(ConvertUserInput(MessageSpan.UserInput));

            // assistant 메시지
            foreach (AssistantSpan Span in MessageSpan.AssistantSpans)
            {
                // Assistant 대답
                Messages.Add(ConvertAssistantBlocks(Span.AssistantBlocks));
                
                // Assistant 도구 실행 결과
                if(Span.ToolExecutions.Count > 0)
                    Messages.Add(ConvertToolResults(Span.ToolExecutions));
            }
        }

        return Messages;
    }

    
    /*
     * UserInput 을 안트로픽 API 메세지로 변환
     * 이미지가 없으면 텍스트, 있으면 이미지 + 텍스트 블록으로 구성
     */
    private static MessageParam ConvertUserInput(UserInput Input)
    {
        List<ContentBlockParam> Blocks = [];

        // 이미지 블록을 먼저 추가 , Claude가 이미지를 먼저 인식하도록
        if (Input.HasImage)
        {
            Blocks.Add(new ImageBlockParam
            {
                Source = new Base64ImageSource
                {
                    MediaType = Input.ImageMediaType!,
                    Data = Input.ImageBase64!
                }
            });
        }

        // Anthropic API는 빈 텍스트 블록을 허용하지 않는다.
        if (!string.IsNullOrWhiteSpace(Input.Text))
        {
            Blocks.Add(new TextBlockParam
            {
                Text = Input.Text
            });
        }

        // UI에서 차단하더라도 잘못된 입력이 API까지 전달되지 않도록 방어한다.
        if (Blocks.Count == 0)
            throw new InvalidOperationException("사용자 메시지에는 텍스트 또는 이미지가 필요합니다.");

        return new MessageParam
        {
            Role = Role.User,
            Content = Blocks
        };
    }

    // 도메인 Block 목록을 안트로픽 API 어시스턴트 메시지로 반환
    private static MessageParam ConvertAssistantBlocks(IReadOnlyList<Block> Blocks)
    {
        List<ContentBlockParam> ContentBlocks = new List<ContentBlockParam>();

        foreach (Block Block in Blocks)
        {
            switch (Block)
            {
                case Block.Text { Content : { } Content }:
                {
                    ContentBlocks.Add(new TextBlockParam { Text = Content });
                    break;
                }

                case Block.Thinking { Content : { } Content, Signature: { } Signature }:
                {
                    ContentBlocks.Add(new ThinkingBlockParam { Thinking = Content, Signature = Signature });
                    break;
                }

                case Block.ToolUse { Id : { } Id, Name : { } Name, InputJson: { } InputJson }:
                {
                    Dictionary<string, JsonElement> ParsedInput = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(InputJson) ?? new Dictionary<string, JsonElement>();
                    
                    ContentBlocks.Add(new ToolUseBlockParam {ID = Id, Name = Name, Input = ParsedInput });
                    break;
                }
            }
        }
        return new MessageParam { Role = Role.Assistant, Content = ContentBlocks };
    }
    // 도구 실행 결과를 안트로픽 API user 메세지(ToolResult) 로 변환
    private MessageParam ConvertToolResults(IReadOnlyList<AssistantSpan.ToolExecution> Executions)
    {
        List<ContentBlockParam> ResultBlocks = Executions.Select(E => (ContentBlockParam)new ToolResultBlockParam
        {
            ToolUseID = E.ToolUseId,
            Content = E.OutPut,
            IsError = E.bIsError ? true : null
        }).ToList();

        return new MessageParam { Role = Role.User, Content = ResultBlocks };
    }
    
    // 대화 내역 초기화
    public void Clear()
    {
        MessageSpans.Clear();
    }

    // 대화 히스토리를 요약 텍스트로 교체
    // 기존 MessageSpan을 모두 지우고, 요약을 Assistant 메시지로 담은 MessageSpan 하나를 추가한다.
    public void Compact(string Summary)
    {
        MessageSpans.Clear();
        
        MessageSpans.Add(new MessageSpan
        {
            AssistantSpans = { new AssistantSpan { AssistantBlocks = [new Block.Text(Summary)]} }
        });
    }

}
