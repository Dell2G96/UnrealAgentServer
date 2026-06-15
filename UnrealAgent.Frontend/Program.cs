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

// using Anthropic.Models.Messages;
// using Microsoft.Extensions.DependencyInjection;
// using UnrealAgent.Backend.Agent;
// using UnrealAgent.Backend.Auth;
// using UnrealAgent.Backend.Conversation;
// using UnrealAgent.Backend.Core;
//
// using UnrealAgent.Backend.Prompt;
// using UnrealAgent.Backend.Tool;
// using UnrealAgent.Backend.Tool.Tools;
//
// using Block = UnrealAgent.Backend.Core.Block;
// using UnrealAgent.Backend.Llm;

#region 26.06.08 Web_Fetch 까지
// //using MessageCreateParams = Anthropic.Models.Beta.Messages.MessageCreateParams;
//
// // ServiceCollection Services = new ServiceCollection();
// var Services = new ServiceCollection();
//
//
// Services.AddHttpClient("OAuth", C => C.Timeout = TimeSpan.FromSeconds(30));
//
// // ── Auth 모듈 ──
// Services.AddSingleton<AuthConfig>();
//
// // ── Codex 모듈 ──
// Services.AddSingleton<OpenAiApiConfig>();
//
// // ── Agent 모듈 (에이전트 루프 + 세션) ──
// Services.AddSingleton<AgentSession>();
//
// // ── Runtime ──
// Services.AddSingleton<PromptBuilder>();
//
// // ── Tool 모듈 ──
// Services.AddSingleton<ToolRegistry>();
// Services.AddSingleton<ToolExecutor>();
//
// // ServiceProvider Provider = Services.BuildServiceProvider();
// // AuthConfig Auth = Provider.GetRequiredService<AuthConfig>();
// // AgentSession AgentSession = Provider.GetRequiredService<AgentSession>();
// // var PromptBuilder = Provider.GetRequiredService<PromptBuilder>();
//
// var Provider = Services.BuildServiceProvider();
//
// var Auth = Provider.GetRequiredService<AuthConfig>();
// var AgentSession = Provider.GetRequiredService<AgentSession>();
// var PromptBuilder = Provider.GetRequiredService<PromptBuilder>();
// var ToolRegistry = Provider.GetRequiredService<ToolRegistry>();
// ToolRegistry.DiscoveryTools(typeof(WebSearch).Assembly);
// var ToolExecutor = Provider.GetRequiredService<ToolExecutor>();
//
//
// //
//  // ToolRegistry.DiscoveryTools(typeof(WebSearch).Assembly);
//
//
// // 로직 시작
// Console.WriteLine("사용할 LLM 공급자를 선택하세요.");
// Console.WriteLine("1. Claude");
// Console.WriteLine("2. OpenAI");
// Console.Write("선택: ");
//
// // 클로드 , Codex 선택
// string? SelectedProvider = Console.ReadLine();
// LlmProvider ProviderType = SelectedProvider?.Trim() switch
// {
//     "2" => LlmProvider.OpenAI,
//     _ => LlmProvider.Claude
// };
//
// ILlmClient LlmClient;
//
//
// // 코덱스 선택 시 
// if (ProviderType == LlmProvider.OpenAI)
// {
//     OpenAiApiConfig OpenAiConfig = Provider.GetRequiredService<OpenAiApiConfig>();
//     OpenAiConfig.Load();
//
//     if (!OpenAiConfig.IsApiKeyConfigured())
//     {
//         Console.Write("OpenAI API Key를 입력하세요 : ");
//         string? Key = Console.ReadLine();
//
//         if (string.IsNullOrWhiteSpace(Key))
//         {
//             Console.WriteLine("OpenAI API Key가 입력되지 않았습니다.");
//             return;
//         }
//     
//         OpenAiConfig.SetApiKey(Key);
//         Console.WriteLine("OpenAI API Key 저장 완료");
//     }
//
//     string OpenAiModel = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-5.5";
//     LlmClient = new OpenAiLlmClient(OpenAiConfig.ApiKey!, OpenAiModel, ToolRegistry, AgentSession);
// }
// // 클로드 사용
// else
// {
//     Auth.Load();
//
//     if (!Auth.IsApiKeyConfigured())
//     {
//         Console.Write("Claude API Key를 입력하세요 : ");
//         string? Key = Console.ReadLine();
//
//         if (string.IsNullOrWhiteSpace(Key))
//         {
//             Console.WriteLine("Claude API Key가 입력되지 않았습니다.");
//             return;
//         }
//     
//         Auth.SetApiKey(Key);
//         Console.WriteLine("Claude API Key 저장 완료");
//     }
//
//     // 26.05.11 - 기존 기본값 "claude-opus-4-6" 대신 현재 직접 호출 경로에서 쓰던 모델명을 기본값으로 맞춤
//     string ClaudeModel = Environment.GetEnvironmentVariable("CLAUDE_MODEL") ?? "claude-opus-4-7";
//     LlmClient = new AnthropicLlmClient(Auth.Client!, ClaudeModel, ToolRegistry);
// }
//
// while (true)
// {
//     #region 기존
//     /*
//      *Console.Write("프롬프트를 입력하세요 : ");
//     string? Prompt = Console.ReadLine();
//     if (string.IsNullOrWhiteSpace(Prompt))
//     {
//         Prompt = "안녕하세요 ! 간단히 자기 소개해주세요";
//     }
//
//     try
//     {
//         if (ProviderType == LlmProvider.Claude)
//         {
//             await foreach (string Chunk in LlmClient.GenerateStreamingAsync(Prompt))
//             {
//                 Console.Write(Chunk);
//             }
//
//             Console.WriteLine();
//         }
//         else
//         {
//             string Response = await LlmClient.GenerateAsync(Prompt);
//             Console.WriteLine(Response);
//         }
//     }
//     catch (Exception Ex)
//     {
//         Console.WriteLine($"{ProviderType} 호출 실패");
//         Console.WriteLine(Ex.ToString());
//     }
//      * 
//      */
//     
//
//     #endregion
//     
//     Console.Write("\n> ");
//     string? Input = Console.ReadLine();
//
//     if (string.IsNullOrWhiteSpace(Input))
//         continue;
//
//     if (Input.Equals("exit", StringComparison.OrdinalIgnoreCase))
//         break;
//
//     // 대화 히스토리에 사용자 입력 추가
//     MessageSpan CurrentMessageSpan = AgentSession.Conversation.AddMessageSpan(Input);
//
//     if (ProviderType == LlmProvider.OpenAI)
//     {
//         try
//         {
//             AssistantSpan AssistantSpan = await LlmClient.GenerateAssistantSpanAsync(
//                 AgentSession.Conversation,
//                 Event =>
//                 {
//                     switch (Event)
//                     {
//                         case ChatEvent.Thinking Think:
//                             Console.Write(Think.Content);
//                             break;
//
//                         case ChatEvent.Text Txt:
//                             Console.Write(Txt.Content);
//                             break;
//
//                         // 26.06.03 - OpenAI 도구 호출도 Claude처럼 콘솔에 표시합니다.
//                         case ChatEvent.ToolStart Tool:
//                             Console.WriteLine($"\n-- {Tool.Name} : {Tool.Input} 도구 사용 --");
//                             break;
//
//                         // 26.06.03 - OpenAI 도구 실행 결과 수신 지점을 명시합니다.
//                         case ChatEvent.ToolEnd:
//                             break;
//                     }
//
//                     return Task.CompletedTask;
//                 });
//
//             CurrentMessageSpan.AssistantSpans.Add(AssistantSpan);
//             Console.WriteLine();
//         }
//         catch (Exception Ex)
//         {
//             Console.WriteLine($"{ProviderType} 호출 실패");
//             Console.WriteLine(Ex.ToString());
//         }
//
//         continue;
//     }
//
//     #region 26.06.01 Tool 추가 코드
//
//     // try
//     // {
//     //     AssistantSpan AssistantSpan = await LlmClient.GenerateAssistantSpanAsync(
//     //         AgentSession.Conversation,
//     //         Event =>
//     //         {
//     //             switch (Event)
//     //             {
//     //                 case ChatEvent.Thinking Think:
//     //                     Console.Write(Think.Content);
//     //                     break;
//     //
//     //                 case ChatEvent.Text Txt:
//     //                     Console.Write(Txt.Content);
//     //                     break;
//     //             }
//     //
//     //             return Task.CompletedTask;
//     //         });
//     //
//     //     CurrentMessageSpan.AssistantSpans.Add(AssistantSpan);
//     //     Console.WriteLine();
//     // }
//     // catch (Exception Ex)
//     // {
//     //     Console.WriteLine($"{ProviderType} 호출 실패");
//     //     Console.WriteLine(Ex.ToString());
//     // }
//
//     #endregion
//
//     // 에이전트 루프 : 도구 실행이 필요하면 API를 반복 호출
//     bool bContinue = true;
//
//     while (bContinue)
//     {
//         // API 요청 파라미터
//         MessageCreateParams Parameters = PromptBuilder.Build(AgentSession);
//         
//         // 스트리밍 응답 수신 및 출력
//         ApiStreamSpan ApiStreamSpan = new ApiStreamSpan();
//         await foreach (RawMessageStreamEvent Event in Auth.Client!.Messages.CreateStreaming(Parameters))
//         {
//             switch(ApiStreamSpan.Process(Event))
//             {
//                case ChatEvent.Text Txt :
//                    Console.Write(Txt.Content);
//                    break;
//             }
//         }
//         
//         // 완료된 응답을 대화 히스토리에 저장
//         switch (ApiStreamSpan.Complete())
//         {
//             case ApiStreamSpan.Result.EndSpan { CompleteSpan: { } AssistantSpan }:
//             {
//                 CurrentMessageSpan.AssistantSpans.Add(AssistantSpan);
//                 bContinue = false;
//
//                 break;
//             }
//
//             case ApiStreamSpan.Result.ExecuteTools { CompletedSpan: { } AssistantSpan, ToolCalls : { } ToolCalls }:
//             {
//                 CurrentMessageSpan.AssistantSpans.Add(AssistantSpan);
//                 
//                 // 도구 실행
//                 foreach (Block.ToolUse ToolCall in ToolCalls)
//                 {
//                     await foreach (ChatEvent Evt in ToolExecutor.ExecuteAsync(ToolCall, AssistantSpan, AgentSession))
//                     {
//                         if (Evt is ChatEvent.ToolStart Tool)
//                             Console.WriteLine($"\n-- {Tool.Name} : {Tool.Input} 도구 사용 --");
//                     }
//                 }
//
//                 // 도구 결과를 포함하여 다음 API 호출로 이어간다
//                 break;
//             }
//             
//             // 서버에 문제가 있는 경우
//             case ApiStreamSpan.Result.Continue { CompletedSpan: { } AssistantSpan }:
//             {
//                 CurrentMessageSpan.AssistantSpans.Add(AssistantSpan);
//                 
//                 // 잘린 응답을 이어서 생성
//                 break;
//             }
//         }
//     }
//     Console.WriteLine();
//     
//     #region "26.05.11 이전 provider별 직접 호출 코드 보존"
//
//     /*
//     // API 요청 파라미터 구현
//     MessageCreateParams Parameters = new MessageCreateParams
//     {
//         Model = "claude-opus-4-7",
//         MaxTokens = 1024,
//         Messages = AgentSession.Conversation.ToAnthropicMessages(),
//         Thinking = new ThinkingConfigAdaptive(),
//         OutputConfig = new OutputConfig()
//         {
//             Effort = Effort.High
//         },
//     };
//
//     ApiStreamSpan ApiStreamSpan = new ApiStreamSpan();
//     if (ProviderType == LlmProvider.Claude)
//     {
//         // 26.05.11 - ILlmClient 이벤트 호출 실험 코드 보존
//         // await foreach (ChatEvent Event in LlmClient.GenerateEventsAsync(Input))
//         // {
//         //     switch (Event)
//         //     {
//         //         case ChatEvent.Thinking Think:
//         //             Console.Write(Think.Content);
//         //             break;
//         //
//         //         case ChatEvent.Text Txt:
//         //             Console.Write(Txt.Content);
//         //             break;
//         //     }
//         // }
//
//         await foreach (RawMessageStreamEvent Event in Auth.Client!.Messages.CreateStreaming(Parameters))
//         {
//             switch (ApiStreamSpan.Process(Event))
//             {
//                 case ChatEvent.Thinking Think :
//                     Console.Write(Think.Content);
//                     break;
//                 case ChatEvent.Text Txt :
//                     Console.Write(Txt.Content);
//                     break;
//             }
//         }
//
//         switch (ApiStreamSpan.Complete())
//         {
//             case ApiStreamSpan.Result.EndSpan { CompleteSpan: { } AssistantSpan }:
//             {
//                 CurrentMessageSpan.AssistantSpans.Add(AssistantSpan);
//                 break;
//             }
//         }
//
//         Console.WriteLine();
//     }
//     else
//     {
//         string Response = await LlmClient.GenerateAsync(Input);
//         Console.WriteLine(Response);
//     }
//     */
//
//     #endregion
//
// }



#endregion

using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Auth;
using UnrealAgent.Backend.Model;
using UnrealAgent.Backend.Model.Models;
using UnrealAgent.Backend.Prompt;
using UnrealAgent.Backend.Tool;
using UnrealAgent.Backend.Tool.Tools;
using UnrealAgent.Frontend.Infrastructure;

WebApplicationBuilder Builder = WebApplication.CreateBuilder(args);
// ── WebApplicationBuilder (서비스 등록 + 앱 설정을 담는 빌더) 생성 ──

// ── Kestrel (요청을 받아서 넘겨주는 서버 엔진) 포트 설정 ──
Builder.WebHost.UseUrls("http://localhost:55558");

// ── 정적 웹 자산 강제 로드 ──
Builder.WebHost.UseStaticWebAssets();

// ── Blazor 서비스 등록 (Razor 컴포넌트 + 서버 측 인터랙티브 모드) ──
Builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// ── HTTP 클라이언트 등록 (외부 API 호출용) ──
Builder.Services.AddHttpClient("OAuth", C => C.Timeout = TimeSpan.FromSeconds(30));
Builder.Services.AddHttpClient("WebFetch");

// ── Auth 모듈 ──
Builder.Services.AddSingleton<AuthConfig>();

// ── Agent 모듈 (에이전트 루프 + 세션) ──
Builder.Services.AddSingleton<AgentSession>();
Builder.Services.AddSingleton<AgentLoop>();

// ── AgentRunner (메세지 큐 + 에이전트 루프 서비스)  ──
Builder.Services.AddSingleton<AgentRunner>();
Builder.Services.AddHostedService(Sp => Sp.GetRequiredService<AgentRunner>());

// ── Runtime 모듈 ──
Builder.Services.AddSingleton<PromptBuilder>();

// ── Tool 모듈 ──
Builder.Services.AddSingleton<ToolRegistry>();
Builder.Services.AddSingleton<ToolExecutor>();

// ── Claude 모델 레지스트리 & 런타임 설정 ──
Builder.Services.AddSingleton<ModelRegistry>();
Builder.Services.AddSingleton<ModelSettings>();


// 여기까지 서비스 등록 단계. Build() 이후는 미들웨어/라우팅 설정 단계입니다.
WebApplication App = Builder.Build();

// ── 어트리뷰트 기반 자동 스캔 ──
App.Services.GetRequiredService<ToolRegistry>().DiscoveryTools(typeof(WebSearch).Assembly);
// 06.12 추가 (Model 설정)
App.Services.GetRequiredService<ModelRegistry>().DiscoverModels(typeof(Opus48).Assembly);

// ── Auth 설정 로드 ──
App.Services.GetRequiredService<AuthConfig>().Load();
App.Services.GetRequiredService<ModelSettings>().Load();

// ── 미들웨어 파이프라인 ──
App.UseStaticFiles();
App.UseAntiforgery();

// ── Blazor 엔드포인트 (Razor 컴포넌트 라우팅 + 서버 렌더 모드 적용) ──
App.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// ── 서버 실행 (요청 수신 대기 시작) - http://localhost:55558/ ──
App.Run();

