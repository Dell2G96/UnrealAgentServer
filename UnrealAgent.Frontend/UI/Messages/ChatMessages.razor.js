let controller;
let observer;

// 바닥에서 이 거리(px) 안이면 "바닥에 있다"고 본다
const BottomThreshold = 40;

/**
 * 채팅 자동 스크롤을 설정합니다.
 * - 바닥에 있을 때 내용이 늘어나면(스트리밍 포함) 자동으로 바닥을 따라간다.
 * - 사용자가 위로 스크롤하면 자동 스크롤을 끄고 ↓ 버튼을 보여준다.
 * - 다시 바닥까지 내려오거나 ↓ 버튼을 누르면 자동 스크롤이 다시 켜진다.
 */
export function setup(container, button)
{
    // 이전 리스너 정리
    controller?.abort();
    observer?.disconnect();
    controller = new AbortController();

    // 자동 스크롤 활성 여부
    let bAutoScroll = true;

    // 직전 scrollTop. 값이 줄어들면 사용자가 위로 올린 것으로 판단한다
    let lastScrollTop = container.scrollTop;

    const isAtBottom = () =>
        container.scrollHeight - container.scrollTop - container.clientHeight <= BottomThreshold;

    const setAutoScroll = (bEnabled) =>
    {
        bAutoScroll = bEnabled;
        button.classList.toggle("hidden", bEnabled);
    };

    const jumpToBottom = () =>
    {
        container.scrollTop = container.scrollHeight;
        lastScrollTop = container.scrollTop;
    };

    container.addEventListener("scroll", () =>
    {
        if (isAtBottom())
            setAutoScroll(true);
        else if (container.scrollTop < lastScrollTop - 1)
            setAutoScroll(false);

        lastScrollTop = container.scrollTop;
    }, { signal: controller.signal });

    // ↓ 버튼 : 바닥으로 부드럽게 이동하고 자동 스크롤을 다시 켠다
    // (내려가는 동안은 scrollTop 이 커지기만 하므로 위 scroll 이벤트가 자동 스크롤을 끄지 않는다)
    button.addEventListener("click", () =>
    {
        setAutoScroll(true);
        container.scrollTo({ top: container.scrollHeight, behavior: "smooth" });
    }, { signal: controller.signal });

    // 메시지 추가 · 스트리밍 글자 추가 · 블록 펼침 등 DOM 변화가 생기면 바닥을 따라간다
    observer = new MutationObserver(() =>
    {
        if (bAutoScroll)
            jumpToBottom();
    });
    observer.observe(container, { childList: true, subtree: true, characterData: true });

    // 처음 열었을 때(복원된 대화 포함) 바닥에서 시작
    jumpToBottom();
}

export function dispose()
{
    controller?.abort();
    observer?.disconnect();
}
