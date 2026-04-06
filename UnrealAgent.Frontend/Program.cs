using Microsoft.Extensions.DependencyInjection;
using UnrealAgent.Backend.Auth;

var Services = new ServiceCollection();


Services.AddSingleton<OAuth>();
Services.AddHttpClient("OAuth", C => C.Timeout = TimeSpan.FromSeconds(30));

var Provider = Services.BuildServiceProvider();

OAuth Auth = Provider.GetRequiredService<OAuth>();


Auth.StartFlow();

Console.Write("인증 코드를 입력하세요 : ");
string? Code = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(Code))
{
    bool bSuccess = await Auth.SubmitCodeAsync(Code);    
    Console.WriteLine(bSuccess ? " 인증성공 !" : $" 인증 실패 :  {Auth.LastError}");
    Console.WriteLine(bSuccess ? Auth.AccessToken : "");
}

