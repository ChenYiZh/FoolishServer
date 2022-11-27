﻿using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using FoolishGames.Collections;
using System.Threading.Tasks;
using FoolishGames.Log;

namespace FoolishGames.Timer
{
    /// <summary>
    /// 时间任务管理
    /// </summary>
    public sealed class TimeWorker
    {
        /// <summary>
        /// 计时器
        /// </summary>
        public System.Threading.Timer Timer { get; private set; }
        /// <summary>
        /// 时间计划
        /// </summary>
        private ThreadSafeDictionary<string, TimeSchedule> schedules;
        /// <summary>
        /// 初始化
        /// </summary>
        public TimeWorker()
        {
            schedules = new ThreadSafeDictionary<string, TimeSchedule>();
        }
        /// <summary>
        /// 计时器间隔
        /// </summary>
        private const int INTERVAL = 100;
        /// <summary>
        /// 重启
        /// </summary>
        public void Restart()
        {
            Stop();
            Timer = new System.Threading.Timer(Update, null, 0, INTERVAL);
        }
        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            if (Timer != null)
            {
                Stop();
            }
            Timer = new System.Threading.Timer(Update, null, 0, INTERVAL);
        }
        /// <summary>
        /// 计时器执行的操作
        /// </summary>
        private void Update(object state)
        {
            List<string> removeCache = new List<string>();
            foreach (KeyValuePair<string, TimeSchedule> schedule in schedules)
            {
                schedule.Value.Execute();
                if (schedule.Value.IsExpired)
                {
                    removeCache.Add(schedule.Key);
                }
            };
            foreach (string key in removeCache)
            {
                schedules.Remove(key);
            }
        }
        /// <summary>
        /// 添加新的时间计划
        /// </summary>
        public void Append(TimeSchedule schedule)
        {
            if (schedule == null)
            {
                FConsole.WriteError("TimeSchedule is Null! ");
                return;
            }
            if (schedules.ContainsKey(schedule.Name))
            {
                FConsole.WriteError("There is a same name of schedule.: " + schedule.Name);
                return;
            }
            lock (schedules)
            {
                schedules.Add(schedule.Name, schedule);
            }
        }
        /// <summary>
        /// 移除时间计划
        /// </summary>
        public void Remove(TimeSchedule schedule)
        {
            if (schedule == null)
            {
                return;
            }
            string key = null;
            foreach (KeyValuePair<string, TimeSchedule> plan in schedules)
            {
                if (plan.Value == schedule)
                {
                    key = plan.Key;
                }
            }
            // WARN: 这里会出现集合修改的问题
            lock (schedules)
            {
                if (key == null && schedules.ContainsKey(schedule.Name))
                {
                    schedules.Remove(schedule.Name);
                    return;
                }
                schedules.Remove(key);
            }
        }
        /// <summary>
        /// 移除并结束所有任务
        /// </summary>
        public void Stop()
        {
            schedules.Clear();
            if (Timer != null)
            {
                Timer.Dispose();
                Timer = null;
            }
        }
    }
}
