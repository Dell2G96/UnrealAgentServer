
// textarea에 키 바인딩을 설정
// Enter → 전송, Shift+Tab → 모드 순환.
// Shift+Enter는 줄바꿈을 유지
export function setupKeyBindings(textarea, dotNetRef) 
{
    textarea.addEventListener("keydown", function (e)
    {
        // 팝업이 열려져있을 때 키 처리
        if(document.querySelector(".command-popup"))
        {
            switch (e.key)
            {
                case "ArrowUp":
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync("PopupNavigate", -1);
                    return;
                case "ArrowDown":
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync("PopupNavigate", 1);
                    return;
                case "Tab":
                case "Enter":
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync("PopupSelect");
                    return;
                case "Escape":
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync("PopupClose");
                    return;
            }
        }
        
        // 기본 키 바인딩
        if(e.key === "Enter" && !e.shiftKey)
        {
            e.preventDefault();
            textarea.closest("form").requestSubmit();
        }
        else if(e.key === "Tab" && e.shiftKey)
        {
            e.preventDefault();
            dotNetRef.invokeMethodAsync("CycleMode");
        }
    });
}