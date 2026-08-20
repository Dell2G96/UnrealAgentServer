using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UnrealAgent.Frontend.Infrastructure;

namespace UnrealAgent.Frontend.UI.Input;

public partial class ImagePicker : JsComponentBase
{
    // 이미지 변경 시 부모에 알리는 콜백
    [Parameter] public EventCallback OnChanged { get; set; }
    
    // 현재 첨부된 이미지의 MIME 타입
    public string? ImageMediaType { get; private set; }
    
    // 현재 첨부된 이미지의 Base64 데이터
    public string? ImageBase64 { get; private set; }
    
    // 파일 다이얼로그 참조
    private ElementReference FileInputRef;

    // .net에서 JS가 호출할 수 있는 참조
    private DotNetObjectReference<ImagePicker>? DotNetRef;

    protected override Task OnModuleLoaded()
    {
        DotNetRef = DotNetObjectReference.Create(this);
        return Task.CompletedTask;
    }
    
    // 파일 다이얼로그
    private async Task Pick()
    {
        await Module.InvokeVoidAsync("pick", FileInputRef, DotNetRef);
    }
    
    // JS에서 이미지 선택 완료시 호출된다.
    [JSInvokable]
    public async Task OnImagePicked(string MediaType, string Base64)
    {
        ImageMediaType = MediaType;
        ImageBase64 = Base64;
        
        await OnChanged.InvokeAsync();
    }
    
    // 첨부된 이미지를 제거
    public async Task Clear()
    {
        ImageMediaType = null;
        ImageBase64 = null;

        await OnChanged.InvokeAsync();
    }
    
    
    
    
    
    
    
    
}
