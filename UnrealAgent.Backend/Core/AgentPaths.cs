namespace UnrealAgent.Backend.Core;

// 프로젝트 경로를 제공하는 정적 클래스
public static class AgentPaths
{
    public static readonly string UserConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".unrealagent");
    
    // 프로젝트 경로 ------------------------------------------------------

    /// UE 프로젝트 루트 경로
    public static string RootPath { get; } = string.Empty;
    
    // .uproject 파일 경로
    public static string UProjectPath { get; } = string.Empty;
    
    // 프로젝트 레벨 설정 디렉터리 경로
    public static string ConfigDir => Path.Combine(RootPath, ".unrealagent");
    
    
    
    // 초기화---------------------------------------------------------------------
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