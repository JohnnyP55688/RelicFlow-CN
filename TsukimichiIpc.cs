using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace RelicFlowCN;

/// <summary>Read-only bridge to Tsukimichi. Every call degrades to an empty result when absent.</summary>
public sealed class TsukimichiIpc
{
    private readonly ICallGateSubscriber<int> apiVersion;
    private readonly ICallGateSubscriber<bool> isReady;
    private readonly ICallGateSubscriber<(uint RowId, string Name, string Step)> getUpNext;
    private readonly ICallGateSubscriber<uint, string[]> getBlockers;

    public TsukimichiIpc(IDalamudPluginInterface pluginInterface)
    {
        apiVersion = pluginInterface.GetIpcSubscriber<int>("Tsukimichi.ApiVersion");
        isReady = pluginInterface.GetIpcSubscriber<bool>("Tsukimichi.IsReady");
        getUpNext = pluginInterface.GetIpcSubscriber<(uint, string, string)>("Tsukimichi.GetUpNext");
        getBlockers = pluginInterface.GetIpcSubscriber<uint, string[]>("Tsukimichi.GetBlockers");
    }

    public bool Available
    {
        get
        {
            try { return apiVersion.InvokeFunc() == 1 && isReady.InvokeFunc(); }
            catch { return false; }
        }
    }

    public (uint RowId, string Name, string Step) UpNext()
    {
        try { return Available ? getUpNext.InvokeFunc() : default; }
        catch { return default; }
    }

    public string[] Blockers(uint questId)
    {
        try { return Available && questId != 0 ? getBlockers.InvokeFunc(questId) : []; }
        catch { return []; }
    }

}
