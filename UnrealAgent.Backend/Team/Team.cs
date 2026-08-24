using System.Diagnostics;
using System.Net.Sockets;
using UnrealAgent.Backend.Core;

namespace UnrealAgent.Backend.Team;

// 팀원의 프로세스와 포트정보
public record TeammateInfo(Process Process, int Port);


/// <summary>
/// 팀 생명 주기, 프로세스 관리, 메시징 담당
/// </summary>
public sealed class Team : IAsyncDisposable
{
    // 현재 팀 이름 , 팀 이 없으면 null
    public string? TeamName { get; set; }
    
    // 에이전트의 이름, 리더는 : "leader | 팀원은 : 고유 이름
    public string AgentName { get; set; } = "leader";

    // 부모 프로세스 ID, 팀원일 때 부모 생존을 감시한다.
    public int? ParentPid { get; set; }

    // 팀원 목록
    private Dictionary<string, TeammateInfo> Teammates { get; } = new();

    // 팀원 목록을 읽기 전용으로 반환
    public IReadOnlyDictionary<string, TeammateInfo> Members => Teammates;

    // 팀 상배 변경 시 발생하는 이벤트
    public event Action? OnTeamChanged;

    // 팀 리소스를 정리
    public async ValueTask DisposeAsync() => await DeleteTeamAsync();
    
    ///////////////////////////////////////////////////////////////////////////
    /// 팀 생명주기
    ///////////////////////////////////////////////////////////////////////////

    public void CreateTeam(string Name)
    {
        if (TeamName is not null)
            throw new InvalidOperationException($"Team '{TeamName}' already exists.");

        if (Directory.Exists(AgentPaths.GetTeamDir(Name)))
            throw new InvalidOperationException($"Team dircetory '{Name}' already exists, Use a different name.");

        Directory.CreateDirectory(AgentPaths.GetMailboxDir(Name));

        TeamName = Name;
        OnTeamChanged?.Invoke();
    }
    
    // 팀 전체를 삭제한다.
    public async Task DeleteTeamAsync()
    {
        if (TeamName is null)
            return;

        foreach (string Name in Teammates.Keys.ToList())
            await ShutdownTeammateAsync(Name);

        string TeamDir = AgentPaths.GetTeamDir(TeamName);
        if(Directory.Exists(TeamDir))
            Directory.Delete(TeamDir, recursive: true);

        TeamName = null;
        OnTeamChanged?.Invoke();
    }


    /// ///////////////////////////////////////////////////////////////////////
    /// 팀원 관리
    /// ///////////////////////////////////////////////////////////////////////

    // 팀원 프로세스를 스폰
    public async Task SpawnTeammateAsync(string Name, string? Prompt)
    {
        if (TeamName is null)
            throw new InvalidOperationException("No active Team.");

        if (Teammates.ContainsKey(Name))
            throw new InvalidOperationException($"Teammate '{Name}' already exists.");

        int Port = FindAvailablePort();
        Process Proc = SpawnProcess(TeamName, Name, Port, Environment.ProcessId);
        
        // 프로세스가 즉시 크래쉬 되었는지 확인
        await Task.Delay(1000);
        if (Proc.HasExited)
        {
            int ExitCode = Proc.ExitCode;
            Proc.Dispose();

            throw new InvalidOperationException($"Teammate  '{Name}' crashed on startup (exit code : {ExitCode}).");
        }

        Teammates[Name] = new TeammateInfo(Proc, Port);
        OnTeamChanged?.Invoke();

        if (!string.IsNullOrEmpty(Prompt))
            await SendMessageAsync(Name, MessageType.Chat, Prompt);

    }
    
    // 팀원 프로세스를 종료
    public async Task ShutdownTeammateAsync(string Name)
    {
        if (TeamName is null)
            return;

        if (Teammates.Remove(Name, out TeammateInfo? Info))
        {
            // 프로세스가 정상적으로 존재할 때만 종료 메세지 전송
            if (!Info.Process.HasExited)
            {
                await SendMessageAsync(Name, MessageType.Command, "shutdown");

                try
                {
                    // 5초후 종료되도록 설정 하고 대기한다
                    using CancellationTokenSource Cts = new(5000);
                    await Info.Process.WaitForExitAsync(Cts.Token);

                }
                catch (OperationCanceledException)
                {
                    Info.Process.Kill(entireProcessTree: true);
                }
            }
                
            Info.Process.Dispose();
        }
        OnTeamChanged?.Invoke();
    }
    
    ///////////////////////////////////////////////////////////////////////////
    /// 메시징
    ///////////////////////////////////////////////////////////////////////////

    // 특정 팀원에게 메세지를 보냄
    public async Task SendMessageAsync(string To, MessageType Type, string Content)
    {
        if (TeamName is null)
            throw new InvalidOperationException("No active Team.");

        string? MailboxDir = AgentPaths.GetMailboxDir(TeamName);
        TeamMessage Message = new(AgentName, Type, Content, DateTime.UtcNow);

        await Mailbox.SendAsync(MailboxDir, To, Message);
    }
    
    // 모든 팀원에게 메세지를 브로드캐스팅 한다
    public async Task BroadcastAsync(string Content)
    {
        foreach (string Name in Teammates.Keys)
            await SendMessageAsync(Name, MessageType.Chat, Content);
    }
    
    
    ///////////////////////////////////////////////////////////////////////////
    /// 프로세스
    ///////////////////////////////////////////////////////////////////////////

    //팀원 프로세스를 생성
    private static Process SpawnProcess(string TeamName, string Name, int Port, int ParentPid)
    {
        string ExePath = Environment.ProcessPath ??
                         throw new InvalidOperationException("Cannot determine process path");
        
        return Process.Start(new ProcessStartInfo
        {
          FileName  = ExePath,
          Arguments = $"--team-name \"{TeamName}\" --agent-name \"{Name}\" --port {Port} --parent-pid {ParentPid}",
          UseShellExecute = false,
          CreateNoWindow = true
        }) ?? throw new InvalidOperationException($"Failed to start process '{Name}'.");
    }
    
    
    // 커맨드라인 인자에서 팀정보를 파싱한다.
    public void ParseArgs(string[] Args)
    {
        for (int i = 0; i < Args.Length - 1; i++)
        {
            switch (Args[i])
            {
                case "--team-name":
                    TeamName = Args[++i];
                    break;
                case "--agent-name":
                    AgentName = Args[++i];
                    break;
                case "--parent-pid":
                    ParentPid = int.Parse(Args[++i]);
                    break;
            }
        }
    }
    
    // 사용 가능한 포트를 찾는다
    private int FindAvailablePort(int StartPort = 59000)
    {
        HashSet<int> UsedPorts = Teammates.Values.Select(t => t.Port).ToHashSet();

        for (int Port = StartPort; Port < StartPort + 1000; Port++)
        {
            if(UsedPorts.Contains(Port))
                continue;

            try
            {
                using TcpListener Listener = new(System.Net.IPAddress.Loopback, Port);
                Listener.Start();
                Listener.Stop();
             
                return Port;
            }
            catch (SocketException)
            {
                
            }
        }

        throw new InvalidOperationException("No available port found.");
    }
}