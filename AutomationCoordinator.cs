using System;
using System.Collections.Generic;
using System.Linq;

namespace RelicFlowCN;

public sealed class AutomationCoordinator(Plugin plugin)
{
    private readonly List<string> queue = [];
    private int index;
    private bool sawQuestionableRunning;
    private bool startRequested;
    private DateTime deadline;

    public bool Running { get; private set; }
    public string Status { get; private set; } = "未启动";

    public bool Start()
    {
        queue.Clear();
        queue.AddRange(plugin.Configuration.QuestAutomationQueue
            .Split([',', ';', '\r', '\n', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase));
        if (queue.Count == 0)
        {
            Status = "请先填写 Questionable 任务 ID。";
            return false;
        }

        if (!plugin.ExternalPlugins.Questionable().Available)
        {
            Status = "Questionable 未连接。";
            return false;
        }

        Running = true;
        index = 0;
        sawQuestionableRunning = false;
        startRequested = false;
        deadline = DateTime.UtcNow.AddSeconds(30);
        Status = "等待其他自动插件空闲";
        return true;
    }

    public void Stop()
    {
        Running = false;
        plugin.ExternalPlugins.StopAutomation();
        Status = "已停止，并向 Questionable 与 AutoDuty 发送停止请求。";
    }

    public void Tick()
    {
        if (!Running) return;

        var fate = plugin.ExternalPlugins.Fate();
        var bozja = plugin.ExternalPlugins.Bozja();
        if ((fate.Available && fate.Running) || (bozja.Available && bozja.Running))
        {
            Status = "等待 FATE／博兹雅自动化停止";
            return;
        }

        var autoDuty = plugin.ExternalPlugins.AutoDuty();
        var questionable = plugin.ExternalPlugins.Questionable();
        if (!questionable.Available)
        {
            Fail("Questionable 连接中断");
            return;
        }

        if (questionable.Running || (autoDuty.Available && autoDuty.Running))
        {
            sawQuestionableRunning |= questionable.Running;
            deadline = DateTime.UtcNow.AddMinutes(10);
            Status = autoDuty.Running
                ? $"任务 {queue[index]}：Questionable 已交给 AutoDuty 执行副本"
                : $"任务 {queue[index]}：Questionable 执行中";
            return;
        }

        if (sawQuestionableRunning)
        {
            index++;
            sawQuestionableRunning = false;
            startRequested = false;
            if (index >= queue.Count)
            {
                if (plugin.Configuration.RepeatAutomationQueue) index = 0;
                else
                {
                    Running = false;
                    Status = "任务队列已完成";
                    return;
                }
            }
            deadline = DateTime.UtcNow.AddSeconds(30);
        }

        if (DateTime.UtcNow > deadline)
        {
            Fail($"启动任务 {queue[index]} 超时");
            return;
        }

        if (startRequested)
        {
            Status = $"等待 Questionable 启动任务 {queue[index]}";
            return;
        }

        if (plugin.ExternalPlugins.StartQuestionableQuest(queue[index]))
        {
            startRequested = true;
            Status = $"已请求 Questionable 启动任务 {queue[index]}";
            deadline = DateTime.UtcNow.AddSeconds(30);
        }
        else
        {
            Fail($"Questionable 拒绝或不支持任务 {queue[index]}");
        }
    }

    private void Fail(string message)
    {
        Running = false;
        Status = message;
    }
}
