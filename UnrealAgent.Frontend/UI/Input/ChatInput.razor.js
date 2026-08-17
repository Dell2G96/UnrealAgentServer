
// textarea에 키 바인딩을 설정합니다.
// 멘션 팝업 열림 시: ArrowUp/Down → 탐색, Enter → 선택, Tab → 드릴다운, ArrowLeft → 상위, Escape → 닫기.
// 커맨드 팝업 열림 시: ArrowUp/Down → 탐색, Enter/Tab → 선택, Escape → 닫기.
export function setupKeyBindings(textarea, dotNetRef) 
{
    textarea.addEventListener("keydown", function (e)
    {

        // 멘션 팝업이 열려있을 때
        if (document.querySelector(".mention-popup"))
        {
            switch (e.key)
            {
                case "ArrowUp":
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync("MentionNavigate", -1);
                    return;
                case "ArrowDown":
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync("MentionNavigate", 1);
                    return;
                case "Enter":
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync("MentionSelect");
                    return;
                case "Tab":
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync("MentionTab");
                    return;
                case "ArrowLeft":
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync("MentionGoBack");
                    return;
                case "Escape":
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync("MentionClose");
                    return;
            }
        }
        
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

// 입력 내용에 맞춰 textarea 높이를 늘린다 (카카오톡처럼 한 줄로 시작해 점점 커짐)
// 높이 상한은 CSS의 max-h-[160px]가 잡아 주고, 넘치면 내부 스크롤이 생긴다.
export function setupAutoGrow(textarea)
{
    const grow = function ()
    {
        // 먼저 auto로 되돌려야 scrollHeight가 '현재 높이'가 아닌 '내용에 필요한 높이'를 준다
        textarea.style.height = "auto";
        textarea.style.height = textarea.scrollHeight + "px";
    };

    textarea.addEventListener("input", grow);
    grow();
}

// 전송 후 높이를 한 줄로 되돌린다
export function resetHeight(textarea)
{
    textarea.style.height = "auto";
}