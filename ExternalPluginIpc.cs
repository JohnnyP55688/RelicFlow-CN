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
    }

    public PluginStatus Fate() => Read(fateApi, fateRunning, fateStatus, fatePhase);
    public PluginStatus Bozja() => Read(bozjaApi, bozjaRunning, bozjaStatus, bozjaState);

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
