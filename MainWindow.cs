using System.Numerics;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace RelicFlowCN;

public sealed class MainWindow(Plugin plugin) : Window("肝武助手 RelicFlow###RelicFlowMain")
{
    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(620, 420),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        var config = plugin.Configuration;
        var selected = RelicCatalog.Find(config.SelectedSeriesId);

        ImGui.TextColored(new Vector4(0.35f, 0.85f, 0.65f, 1f), "免费核心测试版 0.6.1");
        ImGui.SameLine();
        ImGui.TextDisabled("当前版本只管理进度，不会自动操作角色");
        ImGui.Separator();

        ImGui.Text("任务进度读取");
        if (plugin.Tsukimichi.Available)
        {
            var upNext = plugin.Tsukimichi.UpNext();
            ImGui.TextColored(new Vector4(0.35f, 0.85f, 0.65f, 1f), "月道已连接");
            if (upNext.RowId != 0)
            {
                ImGui.SameLine();
                ImGui.TextWrapped($"下一任务：{upNext.Name} · {upNext.Step}");
                foreach (var line in plugin.Tsukimichi.Blockers(upNext.RowId))
                    ImGui.TextDisabled(line);
                ImGui.TextDisabled("任务判断结果已直接集成在 RelicFlow 界面中。中文攻略链接将在遗物任务数据完成映射后显示。");
            }
            else
            {
                ImGui.TextDisabled("月道已就绪，但当前没有返回下一任务。");
            }
        }
        else
        {
            ImGui.TextDisabled("任务读取模块暂不可用，继续使用 RelicFlow 手动清单。");
        }
        ImGui.Separator();

        ImGui.Text("插件联动状态");
        DrawPluginStatus("FATE Autopilot", plugin.ExternalPlugins.Fate());
        DrawPluginStatus("BozjaBuddyReborn-CN", plugin.ExternalPlugins.Bozja());
        DrawPluginStatus("AutoDuty", plugin.ExternalPlugins.AutoDuty());
        DrawPluginStatus("Questionable", plugin.ExternalPlugins.Questionable());
        ImGui.TextDisabled("公开核心仅显示状态，不包含启动、停止、兑换或循环控制。");
        ImGui.Separator();

        ImGui.Text("追踪分类");
        ImGui.SetNextItemWidth(220);
        if (ImGui.BeginCombo("##category", RelicCatalog.CategoryName(config.SelectedCategory)))
        {
            foreach (var category in System.Enum.GetValues<RelicCategory>())
            {
                if (ImGui.Selectable(RelicCatalog.CategoryName(category), category == config.SelectedCategory))
                {
                    config.SelectedCategory = category;
                    var first = RelicCatalog.ForCategory(category).First();
                    config.SelectedSeriesId = first.Id;
                    config.Save();
                    selected = first;
                }
            }
            ImGui.EndCombo();
        }

        if (selected.Category != config.SelectedCategory)
        {
            selected = RelicCatalog.ForCategory(config.SelectedCategory).First();
            config.SelectedSeriesId = selected.Id;
            config.Save();
        }

        ImGui.SameLine();
        ImGui.Text("路线");
        ImGui.SetNextItemWidth(280);
        if (ImGui.BeginCombo("##series", $"{selected.Name} · {selected.Version}"))
        {
            foreach (var series in RelicCatalog.ForCategory(config.SelectedCategory))
            {
                if (ImGui.Selectable($"{series.Name} · {series.Version}", series.Id == selected.Id))
                {
                    config.SelectedSeriesId = series.Id;
                    config.Save();
                }
            }
            ImGui.EndCombo();
        }

        var completed = selected.Steps.Count(x => config.CompletedSteps.Contains(x.Id));
        var fraction = selected.Steps.Count == 0 ? 0 : completed / (float)selected.Steps.Count;
        ImGui.ProgressBar(fraction, new Vector2(-1, 24), $"{completed} / {selected.Steps.Count} 个阶段");

        var next = selected.Steps.FirstOrDefault(x => !config.CompletedSteps.Contains(x.Id));
        ImGui.Spacing();
        ImGui.TextColored(new Vector4(1f, 0.82f, 0.3f, 1f), "推荐下一步");
        if (next is null)
            ImGui.TextWrapped("这条武器线已经全部手动标记完成。可切换其他武器线继续记录。");
        else
        {
            ImGui.TextWrapped($"{next.Title}  ·  {next.Group}");
            ImGui.TextDisabled(next.Detail);
            if (!string.IsNullOrWhiteSpace(next.WikiPage))
                ImGui.TextDisabled($"资料来源：FF14 灰机 Wiki · {next.WikiPage}");
        }

        ImGui.Separator();
        ImGui.Text("步骤清单");
        if (ImGui.BeginChild("steps", new Vector2(0, -42), true))
        {
            foreach (var step in selected.Steps)
            {
                var done = config.CompletedSteps.Contains(step.Id);
                if (ImGui.Checkbox($"##{step.Id}", ref done))
                {
                    if (done) config.CompletedSteps.Add(step.Id);
                    else config.CompletedSteps.Remove(step.Id);
                    config.Save();
                }
                ImGui.SameLine();
                ImGui.TextUnformatted($"[{step.Group}] {step.Title}");
                ImGui.Indent(28);
                ImGui.TextDisabled(step.Detail);
                if (!string.IsNullOrWhiteSpace(step.WikiPage))
                    ImGui.TextDisabled($"FF14 Wiki：{step.WikiPage}");
                ImGui.Unindent(28);
                ImGui.Spacing();
            }
            ImGui.EndChild();
        }

        if (ImGui.Button("全部取消标记"))
        {
            foreach (var step in selected.Steps) config.CompletedSteps.Remove(step.Id);
            config.Save();
        }
        ImGui.SameLine();
        ImGui.TextDisabled("命令：/relicflow 或 /肝武助手");
    }

    private static void DrawPluginStatus(string name, PluginStatus status)
    {
        if (!status.Available)
        {
            ImGui.TextDisabled($"{name}：未连接（需要支持 RelicFlow 接口的新版）");
            return;
        }

        var color = status.Running
            ? new Vector4(0.35f, 0.85f, 0.65f, 1f)
            : new Vector4(0.7f, 0.7f, 0.7f, 1f);
        ImGui.TextColored(color, $"{name}：{(status.Running ? "运行中" : "已连接／空闲")} · {status.Phase}");
        if (!string.IsNullOrWhiteSpace(status.Detail))
            ImGui.TextDisabled(status.Detail);
    }
}
