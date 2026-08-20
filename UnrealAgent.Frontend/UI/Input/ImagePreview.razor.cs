using Microsoft.AspNetCore.Components;

namespace UnrealAgent.Frontend.UI.Input;

public partial class ImagePreview : ComponentBase
{
    // 이미지의 MIME 타입 , null 이면 미리보기를 표시하지 않는다.
    [Parameter] public string? ImageMediaType { get; set; }
    
    // 이미지의 Base64 데이터
    [Parameter] public string? ImageBase64 { get; set; }
    
    // 제거 버튼 클릭 시 콜백
    [Parameter] public EventCallback OnRemove { get; set; }
    
}