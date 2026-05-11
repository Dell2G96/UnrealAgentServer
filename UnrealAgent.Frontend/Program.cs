#region 기존 OAuth 방식


// using Microsoft.Extensions.DependencyInjection;
// using UnrealAgent.Backend.Auth;
//
// var Services = new ServiceCollection();
//
//
// Services.AddSingleton<OAuth>();
// Services.AddHttpClient("OAuth", C => C.Timeout = TimeSpan.FromSeconds(30));
//
// var Provider = Services.BuildServiceProvider();
//
// OAuth Auth = Provider.GetRequiredService<OAuth>();
//
//
// Auth.StartFlow();
//
// Console.Write("인증 코드를 입력하세요 : ");
// string? Code = Console.ReadLine();
//
// if (!string.IsNullOrWhiteSpace(Code))
// {
//     bool bSuccess = await Auth.SubmitCodeAsync(Code);    
//     Console.WriteLine(bSuccess ? " 인증성공 !" : $" 인증 실패 :  {Auth.LastError}");
//     Console.WriteLine(bSuccess ? Auth.AccessToken : "");
// }
//


#endregion

using System.Diagnostics;
using Anthropic.Models.Messages;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Auth;
using UnrealAgent.Backend.Conversation;
using UnrealAgent.Backend.Core;

using UnrealAgent.Backend.Llm;
using Block = UnrealAgent.Backend.Core.Block;

//using MessageCreateParams = Anthropic.Models.Beta.Messages.MessageCreateParams;

ServiceCollection Services = new ServiceCollection();
Services.AddSingleton<AuthConfig>();
Services.AddSingleton<OpenAiApiConfig>();
Services.AddSingleton<AgentSession>();

ServiceProvider Provider = Services.BuildServiceProvider();
AuthConfig Auth = Provider.GetRequiredService<AuthConfig>();
AgentSession AgentSession = Provider.GetRequiredService<AgentSession>();

Auth.Load();

if (!Auth.IsApiKeyConfigured())
{
    Console.Write("API Key를 입력하세요 : ");
    string? Key = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(Key))
    {
        Console.WriteLine("API Key 가 입력되지 않았습니다");
        return;
    }
    
    Auth.SetApiKey(Key);
    Console.WriteLine("Api Key 저장 완료 !");
}

Console.WriteLine("사용할 LLM 공급자를 선택하세요.");
Console.WriteLine("1. Claude");
Console.WriteLine("2. OpenAI");
Console.Write("선택: ");

string? SelectedProvider = Console.ReadLine();
LlmProvider ProviderType = SelectedProvider?.Trim() switch
{
    "2" => LlmProvider.OpenAI,
    _ => LlmProvider.Claude
};

ILlmClient LlmClient;


if (ProviderType == LlmProvider.OpenAI)
{
    OpenAiApiConfig OpenAiConfig = Provider.GetRequiredService<OpenAiApiConfig>();
    OpenAiConfig.Load();

    if (!OpenAiConfig.IsApiKeyConfigured())
    {
        Console.Write("OpenAI API Key를 입력하세요 : ");
        string? Key = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(Key))
        {
            Console.WriteLine("OpenAI API Key가 입력되지 않았습니다.");
            return;
        }
    
        OpenAiConfig.SetApiKey(Key);
        Console.WriteLine("OpenAI API Key 저장 완료");
    }

    string OpenAiModel = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-5.5";
    LlmClient = new OpenAiLlmClient(OpenAiConfig.ApiKey!, OpenAiModel);
}
// 클로드 사용
else
{

    if (!Auth.IsApiKeyConfigured())
    {
        Console.Write("Claude API Key를 입력하세요 : ");
        string? Key = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(Key))
        {
            Console.WriteLine("Claude API Key가 입력되지 않았습니다.");
            return;
        }
    
        Auth.SetApiKey(Key);
        Console.WriteLine("Claude API Key 저장 완료");
    }

    string ClaudeModel = Environment.GetEnvironmentVariable("CLAUDE_MODEL") ?? "claude-opus-4-6";
    LlmClient = new AnthropicLlmClient(Auth.Client!, ClaudeModel);
}

while (true)
{
    #region 기존
    /*
     *Console.Write("프롬프트를 입력하세요 : ");
    string? Prompt = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(Prompt))
    {
        Prompt = "안녕하세요 ! 간단히 자기 소개해주세요";
    }

    try
    {
        if (ProviderType == LlmProvider.Claude)
        {
            await foreach (string Chunk in LlmClient.GenerateStreamingAsync(Prompt))
            {
                Console.Write(Chunk);
            }

            Console.WriteLine();
        }
        else
        {
            string Response = await LlmClient.GenerateAsync(Prompt);
            Console.WriteLine(Response);
        }
    }
    catch (Exception Ex)
    {
        Console.WriteLine($"{ProviderType} 호출 실패");
        Console.WriteLine(Ex.ToString());
    }
     * 
     */
    

    #endregion
    
    Console.Write("\n> ");
    string? Input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(Input))
        continue;

    if (Input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    // 대화 히스토리에 사용자 입력 추가
    MessageSpan CurrentMessageSpan = AgentSession.Conversation.AddMessageSpan(Input);
    
    // API 요청 파라미터 구현
    MessageCreateParams Parameters = new MessageCreateParams
    {
        Model = "claude-opus-4-7",
        MaxTokens = 1024,
        Messages = AgentSession.Conversation.ToAnthropicMessages(),
        Thinking = new ThinkingConfigAdaptive(),
        OutputConfig = new OutputConfig()
        {
            Effort = Effort.High
        },
    };

    ApiStreamSpan ApiStreamSpan = new ApiStreamSpan();
    if (ProviderType == LlmProvider.Claude)
    {
        // await foreach (ChatEvent Event in LlmClient.GenerateEventsAsync(Input))
        // {
        //     switch (Event)
        //     {
        //         case ChatEvent.Thinking Think:
        //             Console.Write(Think.Content);
        //             break;
        //
        //         case ChatEvent.Text Txt:
        //             Console.Write(Txt.Content);
        //             break;
        //     }
        // }

        await foreach (RawMessageStreamEvent Event in Auth.Client!.Messages.CreateStreaming(Parameters))
        {
            switch (ApiStreamSpan.Process(Event))
            {
                case ChatEvent.Thinking Think :
                    Console.Write(Think.Content);
                    break;
                case ChatEvent.Text Txt :
                    Console.Write(Txt.Content);
                    break;
            }
        }

        switch (ApiStreamSpan.Complete())
        {
            case ApiStreamSpan.Result.EndSpan { CompleteSpan: { } AssistantSpan }:
            {
                CurrentMessageSpan.AssistantSpans.Add(AssistantSpan);
                break;
            }
        }


        Console.WriteLine();
    }
    else
    {
        string Response = await LlmClient.GenerateAsync(Input);
        Console.WriteLine(Response);
    }

    #region "기존 코드 Claude SDK 사용"
    /*
    MessageCreateParams Parameters = new MessageCreateParams
    {
        Model = "claude-opus-4-7",
        MaxTokens = 1024,
        Messages = [new() { Role = Role.User, Content = Input }],
        Thinking = new ThinkingConfigAdaptive(),
        OutputConfig = new OutputConfig()
        {
            Effort = Effort.High
        }
    };

    ApiSteamSpan Span = new ApiSteamSpan();

    await foreach (RawMessageStreamEvent Event in Auth.Client!.Messages.CreateStreaming(Parameters))
    {
        switch (Span.Process(Event))
        {
            case ChatEvent.Thinking Think:
                Console.Write(Think.Content);
                break;

            case ChatEvent.Text Txt:
                Console.Write(Txt.Content);
                break;
        }
    }

    Console.WriteLine();

    Console.WriteLine("\n--- 완료된 블록 ---");
    foreach (Block B in Span.Blocks)
    {
        switch (B)
        {
            case Block.Thinking T:
                Console.WriteLine($"Thinking : {T.Content} {T.Signature}");
                break;

            case Block.Text T:
                Console.WriteLine($"Text : {T.Content}");
                break;
        }
    }
    Console.WriteLine(Span.FinalStopReason);
    */
    #endregion
}
