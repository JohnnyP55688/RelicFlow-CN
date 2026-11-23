using System.Collections.Generic;
using Dalamud.Configuration;

namespace RelicFlowCN;

public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public RelicCategory SelectedCategory { get; set; } = RelicCategory.Combat;
    public string SelectedSeriesId { get; set; } = "zodiac";
    public HashSet<string> CompletedSteps { get; set; } = [];

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}
