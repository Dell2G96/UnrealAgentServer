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

using Microsoft.Extensions.DependencyInjection;
using UnrealAgent.Backend.Auth;
using UnrealAgent.Backend.Llm;

ServiceCollection Services = new ServiceCollection();
Services.AddSingleton<AuthConfig>();
Services.AddSingleton<OpenAiApiConfig>();

ServiceProvider Provider = Services.BuildServiceProvider(); 

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
        string? Key = Console.ReadLine():;

        if (string.IsNullOrWhiteSpace(Key))
        {
            Console.WriteLine("OpenAI API Key가 입력되지 않았습니다.");
            return;
        }
    
        OpenAiConfig.SetApiKey(Key);
        Console.WriteLine("OpenAI API Key 저장 완료");
    }

    string OpenAiModel = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-5.1";
    LlmClient = new OpenAiLlmClient(OpenAiConfig.ApiKey!, OpenAiModel);
}
else
{
    AuthConfig Auth = Provider.GetRequiredService<AuthConfig>();
    Auth.Load();

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

Console.Write("프롬프트를 입력하세요 : ");
string? Prompt = Console.ReadLine();
if (string.IsNullOrWhiteSpace(Prompt))
{
    Prompt = "안녕하세요 ! 간단히 자기 소개해주세요";
}

try
{
    string Response = await LlmClient.GenerateAsync(Prompt);
    Console.WriteLine(Response);
}
catch (Exception Ex)
{
    Console.WriteLine($"{ProviderType} 호출 실패");
    Console.WriteLine(Ex.ToString());
}
