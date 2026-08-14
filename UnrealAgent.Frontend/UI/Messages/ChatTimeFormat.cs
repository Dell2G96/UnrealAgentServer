using System.Globalization;

namespace UnrealAgent.Frontend.UI.Messages;

/// <summary>
/// 말풍선 옆에 붙는 시각 문자열을 카카오톡과 같은 "오전/오후 h:mm" 형태로 만든다.
/// C# 참고: static 클래스는 인스턴스를 만들 수 없고 static 멤버만 담는 상자다.
///          C++의 네임스페이스 안 자유 함수 모음과 같은 역할이다.
/// </summary>
public static class ChatTimeFormat
{
    // 서버 로케일이 영어여도 "오전/오후"가 나오도록 한국어 문화권을 고정해 사용한다.
    private static readonly CultureInfo Korean = new("ko-KR");

    // 예: 오후 2:11
    public static string ToKakaoTime(DateTime Time) => Time.ToString("tt h:mm", Korean);
}
