using Microsoft.Extensions.Hosting;
using UnrealAgent.Backend.Chat;
using UnrealAgent.Backend.Conversation;

namespace UnrealAgent.Backend.Agent;

/// <summary>
/// 메시지 큐 + 에이전트 호출 + ChatStore 관리를 담당하는 서비스
/// 리더와 팀원 모두 동일한 서비스를 사용
/// Store 수정은 직접하지 않고 OnChatEvent을 통해 UI 스레드에서 실행
/// </summary>
public sealed class AgentRunner(AgentSession Session) : BackgroundService
{
    // 반응형 상태 관리자
    public ChatStore Store { get; } = new();

    // 사용자 입력을 순서대로 보관하는 메시지 큐
    private readonly Queue<UserInput> MessageQueue = new();

    // 큐에 메세지가 도착하면 BackGroundService 루프를 꺠우는 시그널
    private readonly SemaphoreSlim Signal = new(0);

    // ChatEvent 발생 시 UI 스레드에서 처리할 이벤트
    public event Func<ChatEvent, Task>? OnChatEvent;

    protected override async Task ExecuteAsync(CancellationToken Ct)
    {
        while (!Ct.IsCancellationRequested)
        {
            // 시그널 대기 - EnqueueMessage 메세지 도착 시 해제
            await Signal.WaitAsync(Ct);

            // 메시지 큐를 순차 처리
            await DrainQueue();
        }
    }

    // 메세지 큐를 추가하고 BackgroundService 루프를 꺠운다
    public async void EnqueueMessage(UserInput Input)
    {
        // 사용자 메시지 UI를 위해 추가
        await DispatchEventAsync(new ChatEvent.User(Input.Text)); 
        
        MessageQueue.Enqueue(Input);
        Signal.Release();
    }
    
    // 큐에서 메시지를 하나씩 꺼내 순차적으러 처리한다
    private async Task DrainQueue()
    {
        while (MessageQueue.TryDequeue(out UserInput? Input))
        {
            try
            {
                await foreach (ChatEvent Evt in Session.ProcessMessage(Input))
                {
                    await DispatchEventAsync(Evt);
                }
            }
            catch (Exception e)
            {
                await DispatchEventAsync(new ChatEvent.System(e.Message));
            }
        }
    }

    /// <summary>
    /// Chat Event를 UI 스레드로 디스패치한다
    /// </summary>
    private Task DispatchEventAsync(ChatEvent Evt)
    {
        if (OnChatEvent is { } Handler)
            return Handler(Evt);
        
        // 구독자가 없으면 (UI미로드 상태) 직접 Store에 누적
        Store.Process(Evt);
        return Task.CompletedTask;
    }
}