using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace RelicFlowCN;

public sealed class ExternalPluginIpc
{
    private readonly ICallGateSubscriber<int> fateApi;
    private readonly ICallGateSubscriber<bool> fateRunning;
    private readonly ICallGateSubscriber<string> fateStatus;
    private readonly ICallGateSubscriber<string> fatePhase;
    private readonly ICallGateSubscriber<int> bozjaApi;
    private readonly ICallGateSubscriber<bool> bozjaRunning;
    private readonly ICallGateSubscriber<string> bozjaStatus;
    private readonly ICallGateSubscriber<string> bozjaState;
    private readonly ICallGateSubscriber<bool> autoDutyNavigating;
    private readonly ICallGateSubscriber<bool> autoDutyLooping;
    private readonly ICallGateSubscriber<bool> autoDutyStopped;
    private readonly ICallGateSubscriber<bool> questionableRunning;
    private readonly ICallGateSubscriber<string?> questionableQuestId;
    private readonly ICallGateSubscriber<string, bool> questionableStartSingleQuest;
    private readonly ICallGateSubscriber<string, bool> questionableStop;
    private readonly ICallGateSubscriber<object> autoDutyStop;

    public ExternalPluginIpc(IDalamudPluginInterface pi)
    {
        fateApi = pi.GetIpcSubscriber<int>("FateAutopilot.ApiVersion");
        fateRunning = pi.GetIpcSubscriber<bool>("FateAutopilot.IsRunning");
        fateStatus = pi.GetIpcSubscriber<string>("FateAutopilot.GetStatus");
        fatePhase = pi.GetIpcSubscriber<string>("FateAutopilot.GetPhase");
        bozjaApi = pi.GetIpcSubscriber<int>("BozjaBuddyReborn.ApiVersion");
        bozjaRunning = pi.GetIpcSubscriber<bool>("BozjaBuddyReborn.IsRunning");
        bozjaStatus = pi.GetIpcSubscriber<string>("BozjaBuddyReborn.GetStatus");
        bozjaState = pi.GetIpcSubscriber<string>("BozjaBuddyReborn.GetState");
        autoDutyNavigating = pi.GetIpcSubscriber<bool>("AutoDuty.IsNavigating");
        autoDutyLooping = pi.GetIpcSubscriber<bool>("AutoDuty.IsLooping");
        autoDutyStopped = pi.GetIpcSubscriber<bool>("AutoDuty.IsStopped");
        autoDutyStop = pi.GetIpcSubscriber<object>("AutoDuty.Stop");
        questionableRunning = pi.GetIpcSubscriber<bool>("Questionable.IsRunning");
        questionableQuestId = pi.GetIpcSubscriber<string?>("Questionable.GetCurrentQuestId");
        questionableStartSingleQuest = pi.GetIpcSubscriber<string, bool>("Questionable.StartSingleQuest");
        questionableStop = pi.GetIpcSubscriber<string, bool>("Questionable.Stop");
    }

    public PluginStatus Fate() => Read(fateApi, fateRunning, fateStatus, fatePhase);
    public PluginStatus Bozja() => Read(bozjaApi, bozjaRunning, bozjaStatus, bozjaState);
    public PluginStatus AutoDuty()
    {
        try
        {
            var navigating = autoDutyNavigating.InvokeFunc();
            var looping = autoDutyLooping.InvokeFunc();
            var stopped = autoDutyStopped.InvokeFunc();
            var phase = navigating ? "副本路径运行中" : looping ? "循环排队中" : stopped ? "已停止" : "准备中";
            var detail = navigating
                ? "AutoDuty 正在执行当前副本路径。"
                : looping
                    ? "AutoDuty 已启用循环流程，正在等待或切换副本。"
                    : stopped ? "AutoDuty 已连接，目前没有运行任务。" : "AutoDuty 已连接，正在准备任务。";
            return new PluginStatus(true, navigating || looping, phase, detail);
        }
        catch
        {
            return PluginStatus.Unavailable;
        }
    }

    public PluginStatus Questionable()
    {
        try
        {
            var running = questionableRunning.InvokeFunc();
            var questId = questionableQuestId.InvokeFunc();
            var detail = string.IsNullOrWhiteSpace(questId)
                ? "Questionable 已连接，目前没有选中的任务。"
                : $"当前任务：{questId}";
            return new PluginStatus(true, running, running ? "任务执行中" : "已连接／空闲", detail);
        }
        catch
        {
            return PluginStatus.Unavailable;
        }
    }

    public bool StartQuestionableQuest(string questId)
    {
        try { return questionableStartSingleQuest.InvokeFunc(questId); }
        catch { return false; }
    }

    public void StopAutomation()
    {
        try { questionableStop.InvokeFunc("RelicFlow emergency stop"); } catch { }
        try { autoDutyStop.InvokeAction(); } catch { }
    }

    private static PluginStatus Read(
        ICallGateSubscriber<int> api,
        ICallGateSubscriber<bool> running,
        ICallGateSubscriber<string> status,
        ICallGateSubscriber<string> phase)
    {
        try
        {
            return api.InvokeFunc() < 1
                ? PluginStatus.Unavailable
                : new PluginStatus(true, running.InvokeFunc(), phase.InvokeFunc(), status.InvokeFunc());
        }
        catch
        {
            return PluginStatus.Unavailable;
        }
    }
}

public readonly record struct PluginStatus(bool Available, bool Running, string Phase, string Detail)
{
    public static PluginStatus Unavailable => new(false, false, string.Empty, string.Empty);
}
