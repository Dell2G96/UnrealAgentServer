using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UnrealAgent.Backend.Agent;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Command.Attributes;

namespace UnrealAgent.Backend.Command;

/// <summary>
/// [AgentCommand] 어트리뷰트를 스캔하여 슬래시 커맨드를 등록하고 실행한다
/// 커맨드 인스턴스는 Discovery 시 한 번 생성되어 재사용 된다
/// </summary>
public sealed class CommandRegistry(IServiceProvider ServiceProvider)
{
    
    /// 등록된 커맨드 정보 
    public sealed record CommandEntry(string Name, string Description, string Icon, IAgentCommand Command);

    // 커맨드 이름 -> CommandEntry 매핑
    // 대소문자를 구분하지 않는다
    private readonly Dictionary<string, CommandEntry> Commands = new(StringComparer.OrdinalIgnoreCase);
    
    // 지정된 어셈블리에서 [AgentCommand] + IAgentCommand 클래스를 스캔하여 등록한다
    // 인스턴스는 DI로 한 번 생성되어 재사용된다
    public void DiscoverCommands(params Assembly[] Assemblies)
    {
        foreach (Assembly Asm in Assemblies)
        {
            foreach (Type Type in Asm.GetTypes())
            {
                AgentCommandAttribute? Attr = Type.GetCustomAttribute<AgentCommandAttribute>();

                if (Attr is null)
                    continue;
                
                if(!typeof(IAgentCommand).IsAssignableFrom(Type))
                    continue;
                
                if(ActivatorUtilities.CreateInstance(ServiceProvider, Type) is not IAgentCommand Instance)
                    continue;

                Commands[Attr.Name] = new CommandEntry(Attr.Name, Attr.Description, Attr.Icon, Instance);
            }
        }
    }
    
    // 등록된 모든 커맨드 정보를 반환
    public IReadOnlyList<CommandEntry> GetAll() => Commands.Values.ToList();
    
    // 슬래시 입력이 등록된 커맨드인지 확인한다
    // "/clear arg1" -> "/clear"로 매칭
    public bool HasCommand(string Input)
    {
        string Name = Input.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        return Commands.ContainsKey(Name);
    }

    // 슬래시 커맨드를 파싱하고 실행
    // Claude에게 전달하지 않고 서버가 직접 처리
    public async IAsyncEnumerable<ChatEvent> ExecuteAsync(string Input, AgentSession Session)
    {
        string[] Parts = Input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string Name = Parts[0];
        string[] Args = Parts.Length > 1 ? Parts[1..] : [];

        if (!Commands.TryGetValue(Name, out CommandEntry? Entry))
        {
            yield return new ChatEvent.System($"알 수 없는 커맨드 :  {Name}");
            yield break;
        }

        await foreach (ChatEvent Evt in Entry.Command.ExecuteAsync(Args, Session))
            yield return Evt;
    }


}