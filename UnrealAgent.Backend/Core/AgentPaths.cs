using System.Security.Cryptography;
using System.Text;

namespace UnrealAgent.Backend.Core;

// 프로젝트 경로를 제공하는 정적 클래스
public static class AgentPaths
{
    public static readonly string UserConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".unrealagent");
    
    ///////////////////////////////////////////////////////////////////////////
    // 프로젝트 경로 
    ///////////////////////////////////////////////////////////////////////////

    /// UE 프로젝트 루트 경로
    public static string RootPath { get; } = string.Empty;
    
    // .uproject 파일 경로
    public static string UProjectPath { get; } = string.Empty;
    
    // 프로젝트 레벨 설정 디렉터리 경로
    public static string ConfigDir => Path.Combine(RootPath, ".unrealagent");

    // 스킬 디렉터리 경로 
    // ({ConfigDir}/skills)
    public static string SkillsDir => Path.Combine(ConfigDir, "skills");

    // 대화 세션 저장 디렉터리 경로.
    // 대화 내용(민감한 정보 포함 가능)이 UE 프로젝트 트리 안에 남아 git/zip 공유 시
    // 함께 유출되지 않도록, 프로젝트 폴더가 아니라 사용자 설정 폴더 아래에 저장한다
    // ({UserConfigDir}/sessions/{프로젝트 경로 해시})
    public static string SessionsDir => Path.Combine(UserConfigDir, "sessions", ProjectKey);

    // RootPath를 식별하는 짧은 해시. 같은 사용자 설정 폴더 아래에서 UE 프로젝트별로
    // 세션 저장 위치를 분리하기 위함 (RootPath가 없으면 "default" 폴더를 쓴다)
    private static string ProjectKey =>
        string.IsNullOrEmpty(RootPath)
            ? "default"
            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(RootPath)))[..16];
    
    ///////////////////////////////////////////////////////////////////////////
    /// 팀 경로
    ///////////////////////////////////////////////////////////////////////////

    // 전체 팀 루트 디렉토리
    public static string TeamsRoot => Path.Combine(ConfigDir, "teams");
    
    // 특정 팀의 디렉토리 경로를 반환한다.
    public static string GetTeamDir(string TeamName) => Path.Combine(TeamsRoot, TeamName);
    
    // 팀 메일박스 디렉토리 경로르 반환한다.
    public static string GetMailboxDir(string TeamName) => Path.Combine(GetTeamDir(TeamName), "mailbox");
    
    
    ///////////////////////////////////////////////////////////////////////////
    // 초기화
    ///////////////////////////////////////////////////////////////////////////
    static AgentPaths()
    {
        // UE가 실행 시 --project-dir 인자로 프로젝트 경로를 넘겨준다.
        // (Agent Server가 프로젝트 트리 밖의 별도 빌드 경로에서 실행되므로
        // exe 위치 기준 디렉토리 탐색으로는 프로젝트를 찾을 수 없다)
        string[] Args = Environment.GetCommandLineArgs();
        string? ProjectDirArg = null;

        for (int i = 0; i < Args.Length - 1; i++)
        {
            if (Args[i] == "--project-dir")
            {
                ProjectDirArg = Args[i + 1];
                break;
            }
        }

        DirectoryInfo? Dir = ProjectDirArg is not null
            ? new DirectoryInfo(ProjectDirArg)
            : new DirectoryInfo(AppContext.BaseDirectory);

        while (Dir is not null)
        {
            FileInfo? UProject = Dir.GetFiles("*.uproject").FirstOrDefault();

            if (UProject is not null)
            {
                RootPath = Dir.FullName;
                UProjectPath = UProject.FullName;

                return;
            }

            Dir = Dir.Parent;
        }
    }


}