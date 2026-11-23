using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace RelicFlowCN;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    internal Configuration Configuration { get; }
    internal TsukimichiIpc Tsukimichi { get; }
    internal ExternalPluginIpc ExternalPlugins { get; }
    private WindowSystem WindowSystem { get; } = new("RelicFlowCN");
    private MainWindow MainWindow { get; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Tsukimichi = new TsukimichiIpc(PluginInterface);
        ExternalPlugins = new ExternalPluginIpc(PluginInterface);
        MainWindow = new MainWindow(this);
        WindowSystem.AddWindow(MainWindow);
        CommandManager.AddHandler("/relicflow", new CommandInfo(OnCommand) { HelpMessage = "打开肝武助手" });
        CommandManager.AddHandler("/肝武助手", new CommandInfo(OnCommand) { HelpMessage = "打开肝武助手" });
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi += Toggle;
        PluginInterface.UiBuilder.OpenConfigUi += Toggle;
        Log.Information("RelicFlow CN 0.1.0 loaded.");
    }

    private void OnCommand(string command, string arguments) => Toggle();
    private void Toggle() => MainWindow.Toggle();

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi -= Toggle;
        PluginInterface.UiBuilder.OpenConfigUi -= Toggle;
        CommandManager.RemoveHandler("/relicflow");
        CommandManager.RemoveHandler("/肝武助手");
        WindowSystem.RemoveAllWindows();
    }
}
