using System.Text.Json;

namespace UnrealAgent.Backend.Team;

/// <summary>
/// 파일 기반 메일박스
/// 메세지 1개 : 파일 1개 방식으로 동작한다
/// 보내기 : 상대 폴더에 JSON 형식의 파일 생성
/// 받기 : 내 폴더의 JSON 형식의 파일을 읽고 삭제한다.
/// </summary>
public static class Mailbox
{
    // 메세지를 받을 팀원의 메일박스에 전송한다
    public static async Task SendAsync(string MailboxDir, string To, TeamMessage Message)
    {
        string Dir = Path.Combine(MailboxDir, To);
        Directory.CreateDirectory(Dir);

        string FilePath = Path.Combine(Dir, $"{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}.json");
        string Json = JsonSerializer.Serialize(Message);

        await File.WriteAllTextAsync(FilePath, Json);
    }
    
    // 내 메일박스의 모든 메세지를 가져오고 해당 파일을 삭제한다
    public static async Task<List<TeamMessage>> TakeAllAsync(string MailboxDir, string Me)
    {
        string Dir = Path.Combine(MailboxDir, Me);
        
        if (!Directory.Exists(Dir))
            return [];

        string[] Files = Directory.GetFiles(Dir, "*.json");
        if (Files.Length == 0)
            return [];
        
        // 시간 순으로 정렬한다
        Array.Sort(Files);

        List<TeamMessage> Messages = new(Files.Length);
        foreach (string File_ in Files)
        {
            try
            {
                string Json = await File.ReadAllTextAsync(File_);
                TeamMessage? Msg = JsonSerializer.Deserialize<TeamMessage>(Json);
                
                if(Msg is not null)
                    Messages.Add(Msg);

            }
            catch (Exception)
            {
                // 파싱 실패하면 무시하고 계속 진행
            }
            
            // 파일 삭제
            File.Delete(File_);
        }

        return Messages;
    }
}