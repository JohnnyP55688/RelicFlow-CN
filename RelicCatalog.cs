using System.Collections.Generic;
using System.Linq;

namespace RelicFlowCN;

public enum RelicCategory { Combat, DeepDungeon, CrafterGatherer }

public sealed record RelicStep(string Id, string Group, string Title, string Detail, string? WikiPage = null);
public sealed record RelicSeries(string Id, string Name, string Version, RelicCategory Category, IReadOnlyList<RelicStep> Steps);

public static class RelicCatalog
{
    public static string CategoryName(RelicCategory category) => category switch
    {
        RelicCategory.Combat => "战斗肝武",
        RelicCategory.DeepDungeon => "深层迷宫武器",
        RelicCategory.CrafterGatherer => "制作采集工具",
        _ => "其他"
    };

    public static IReadOnlyList<RelicSeries> Series { get; } =
    [
        Combat("zodiac", "上古／黄道武器", "2.0", "黄道武器",
            ("上古", "取得基础上古武器", "完成对应职业的上古武器任务并取得成品。"),
            ("天极", "强化为天极武器", "准备萨维奈灵药并完成强化。"),
            ("魂晶", "收集十二颗魂晶", "在指定旧地图完成危命任务，集齐十二种魂晶。"),
            ("天火文书", "完成九本天火文书", "逐本完成指定怪物、危命任务、理符和副本。"),
            ("新星", "完成星盘与属性灌注", "准备星光变石及魔晶石，完成属性灌注。"),
            ("镇魂", "完成灵魂定着", "通过指定战斗内容累计光。"),
            ("觉醒", "完成黄道武器·觉醒", "完成十二种玛哈特玛及最终任务。")),
        Combat("anima", "元灵武器", "3.0", "元灵武器",
            ("启程", "开启元灵武器任务线", "确认职业与初始武器，完成前置任务。"),
            ("水晶", "收集星极与灵极水晶", "在苍穹地图完成危命任务并收集水晶。"),
            ("副本", "依次完成指定副本", "携带任务武器按任务要求完成副本。"),
            ("材料", "准备神秘系列材料", "核对神秘骨片、贝壳、原石、种子及对应兑换品。"),
            ("培养", "完成灵魂凝聚与属性培养", "按任务要求准备材料并分配属性。"),
            ("刷光", "完成魂武刷光", "通过指定战斗内容累计凝聚度。"),
            ("最终", "完成元灵武器", "完成最后的讨伐与任务交付。")),
        Combat("eureka", "禁地兵装", "4.0", "禁地兵装",
            ("常风之地", "完成风岛强化", "提升元素等级，收集常风晶簇并强化武器。"),
            ("恒冰之地", "完成冰岛强化", "收集恒冰晶簇并完成结晶化相关步骤。"),
            ("涌火之地", "完成火岛强化", "收集涌火晶簇，推进文理技能与武器强化。"),
            ("丰水之地", "完成水岛最终强化", "收集丰水晶簇并完成最终阶段。")),
        Combat("resistance", "义军武器", "5.0", "义军武器",
            ("启程", "取得初始义军武器", "完成前置任务并取得对应职业武器。"),
            ("记忆", "收集三色记忆晶块", "烦恼、悲伤、恐惧三种记忆晶块各收集二十个。"),
            ("记忆", "收集苦涩的记忆晶块", "通过博兹雅或指定内容取得任务材料。"),
            ("女王古殿", "完成女王古殿阶段", "完成任务所需副本和后续材料收集。"),
            ("扎杜诺尔", "完成扎杜诺尔材料阶段", "收集区域遭遇战与指定副本材料。"),
            ("最终", "完成义军武器最终强化", "完成最终材料收集与属性分配。")),
        Combat("manderville", "曼德维尔武器", "6.0", "曼德维尔武器",
            ("前置", "完成希尔迪布兰德系列前置", "推进相关支线至当前武器阶段所需进度。"),
            ("强化", "取得曼德维尔武器", "接受任务并准备对应阶段的强化材料。"),
            ("强化", "完成惊奇曼德维尔阶段", "准备材料并完成第二阶段任务。"),
            ("强化", "完成华丽曼德维尔阶段", "准备材料并完成第三阶段任务。"),
            ("最终", "完成最终强化", "完成最终任务并确认成品。")),
        Combat("phantom", "幻境武器", "7.0", "幻境武器",
            ("前置", "开启幻境武器任务线", "确认版本前置和对应职业条件。"),
            ("当前阶段", "推进当前开放的强化步骤", "根据国服当前版本核对任务、材料与交付地点。")),

        Deep("potd", "角尊／闪熠装备", "3.0", "死者宫殿"),
        Deep("hoh", "天之武器", "4.0", "天之御柱"),
        Deep("orthos", "正统／高尚武器", "6.0", "正统优雷卡"),
        Deep("pilgrim", "光耀／圣礼武器", "7.0", "朝圣交错路"),

        Tool("luminary", "华光工具", "2.0", "华光工具"),
        Tool("blessed1", "赐福工具（3.0）", "3.0", "制作采集特殊工具"),
        Tool("blessed2", "赐福工具（4.0）", "4.0", "制作采集特殊工具"),
        Tool("skysteel", "天钢工具", "5.0", "天钢工具"),
        Tool("resplendent", "辉煌工具", "5.0", "制作采集特殊工具"),
        Tool("splendorous", "莫雯卓越工具", "6.0", "莫雯卓越工具"),
        Tool("cosmic", "宇宙工具", "7.0", "宇宙工具")
    ];

    private static RelicSeries Combat(string id, string name, string version, string wiki,
        params (string Group, string Title, string Detail)[] steps) =>
        new(id, name, version, RelicCategory.Combat,
            steps.Select((x, i) => new RelicStep($"{id}-{i + 1}", x.Group, x.Title, x.Detail, wiki)).ToArray());

    private static RelicSeries Deep(string id, string name, string version, string wiki) =>
        new(id, name, version, RelicCategory.DeepDungeon,
        [
            new($"{id}-aetherpool", wiki, "强化聚魔装备", $"在{wiki}内提升聚魔武器与防具强化值。", wiki),
            new($"{id}-exchange", "带出", $"交换{name}", "达到要求后在迷宫外兑换并带出对应职业武器。", wiki)
        ]);

    private static RelicSeries Tool(string id, string name, string version, string wiki) =>
        new(id, name, version, RelicCategory.CrafterGatherer,
        [
            new($"{id}-unlock", "前置", $"解锁{name}", "确认职业等级、主线与支线前置条件。", wiki),
            new($"{id}-progress", "制作／采集", "完成阶段目标", "按当前阶段完成制作、采集、鉴定或兑换要求。", wiki),
            new($"{id}-finish", "完成", $"取得{name}", "交付阶段材料并确认工具已强化完成。", wiki)
        ]);

    public static IReadOnlyList<RelicSeries> ForCategory(RelicCategory category) =>
        Series.Where(x => x.Category == category).ToArray();

    public static RelicSeries Find(string id) => Series.FirstOrDefault(x => x.Id == id) ?? Series[0];
}
