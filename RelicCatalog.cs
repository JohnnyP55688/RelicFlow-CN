using System.Collections.Generic;
using System.Linq;

namespace RelicFlowCN;

public sealed record RelicStep(string Id, string Group, string Title, string Detail, bool Automated = false);
public sealed record RelicSeries(string Id, string Name, string Expansion, IReadOnlyList<RelicStep> Steps);

public static class RelicCatalog
{
    public static IReadOnlyList<RelicSeries> Series { get; } =
    [
        new("zodiac", "古武 / 黄道武器", "重生之境",
        [
            new("zodiac-base", "任务", "完成基础古武任务", "确认已取得对应职业的基础古武。"),
            new("zodiac-zenith", "强化", "升级为天极武器", "准备萨维奈灵药并完成强化。"),
            new("zodiac-atma", "FATE", "收集十二颗魂晶", "在指定旧地图完成 FATE；第一版由你手动记录完成情况。"),
            new("zodiac-animus", "天火文书", "完成九本天火文书", "逐本完成指定怪物、FATE、理符和副本。"),
            new("zodiac-novus", "强化", "完成星盘与魔晶石阶段", "记录星光变石及属性灌注进度。"),
            new("zodiac-nexus", "刷光", "完成灵魂定着", "通过副本等内容累计光。"),
            new("zodiac-zeta", "最终阶段", "完成黄道武器·觉醒", "完成十二种玛哈特玛及最终任务。")
        ]),
        new("anima", "魂武 / 元灵武器", "苍穹之禁城",
        [
            new("anima-start", "任务", "开启元灵武器任务线", "确认职业与初始武器。"),
            new("anima-crystals", "FATE", "收集星极与灵极水晶", "在苍穹地图完成 FATE。"),
            new("anima-dungeons", "副本", "完成指定副本流程", "按任务要求依次完成副本。"),
            new("anima-materials", "材料", "准备强化材料", "记录诗学与兑换材料。"),
            new("anima-light", "刷光", "完成魂武刷光", "记录凝聚度进度。"),
            new("anima-finish", "最终阶段", "完成元灵武器", "完成最终任务与战斗。")
        ]),
        new("eureka", "优雷卡武器", "红莲之狂潮",
        [
            new("eureka-anemos", "优雷卡", "风岛阶段", "记录常风晶簇与武器强化。"),
            new("eureka-pagos", "优雷卡", "冰岛阶段", "记录恒冰晶簇、结晶化与武器强化。"),
            new("eureka-pyros", "优雷卡", "火岛阶段", "记录涌火晶簇与文理技能相关进度。"),
            new("eureka-hydatos", "优雷卡", "水岛阶段", "记录丰水晶簇和最终强化。")
        ]),
        new("resistance", "义军武器", "暗影之逆焰",
        [
            new("resistance-start", "任务", "开启义军武器任务线", "完成前置任务并取得初始武器。"),
            new("resistance-memories", "材料", "收集三色记忆", "可在博兹雅或对应旧地图获得。"),
            new("resistance-bitter", "材料", "收集苦涩的记忆", "记录任务材料数量。"),
            new("resistance-reginae", "博兹雅", "女王古殿与后续材料", "第一版提供清单；后续接入 BBR 国服版。"),
            new("resistance-zadnor", "扎杜诺尔", "收集区域与副本材料", "记录扎杜诺尔及相关任务进度。"),
            new("resistance-finish", "最终阶段", "完成义军武器最终强化", "完成最终材料与属性步骤。")
        ])
    ];

    public static RelicSeries Find(string id) => Series.FirstOrDefault(x => x.Id == id) ?? Series[0];
}
