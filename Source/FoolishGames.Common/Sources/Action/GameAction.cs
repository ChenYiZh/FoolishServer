using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FoolishGames.IO;
using FoolishGames.Log;

namespace FoolishGames.Action
{
    /// <summary>
    /// 消息基类
    /// </summary>
    public abstract class GameAction : IAction
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public long MsgId { get; private set; }

        /// <summary>
        /// ActionId
        /// </summary>
        public int ActionId { get; private set; }

        /// <summary>
        /// 是否统计时间
        /// </summary>
        public bool AnalysisTime { get; protected set; } = true;

        /// <summary>
        /// 警告超时时间毫秒，0为全部统计
        /// </summary>
        public int AlertTimeout { get; protected set; } = 1000;

        /// <summary>
        /// 计时器
        /// </summary>
        private Stopwatch watch = null;

        /// <summary>
        /// 是否在计时
        /// </summary>
        private bool IsTiming { get { return watch != null && watch.IsRunning; } }

        /// <summary>
        /// 接收到的消息
        /// </summary>
        private IMessageReader Reader { get; set; }

        /// <summary>
        /// 刚创建时处理，所有参数都还没有赋值
        /// </summary>
        public virtual void Awake() { }

        /// <summary>
        /// 类名
        /// </summary>
        private string typeName = null;

        /// <summary>
        /// 类名
        /// </summary>
        protected string TypeName
        {
            get
            {
                if (typeName == null)
                {
                    typeName = GetType().Name;
                }
                return typeName;
            }
        }

        /// <summary>
        /// 工作函数
        /// </summary>
        /// <param name="reader"></param>
        internal void Work(int actionId, IMessageReader reader)
        {
            MsgId = reader.MsgId;
            ActionId = actionId;
            Reader = reader;
            AnalysisTime = true;
            AlertTimeout = 1000;
            try
            {
                Awake();
                if (AnalysisTime)
                {
                    if (watch == null) { watch = new Stopwatch(); }
                    watch.Restart();
                }
                SetReader(reader);
                if (Check())
                {
                    TakeAction(reader);
                }
                if (IsTiming)
                {
                    watch.Stop();
                    TimeSpan deltaTime = watch.Elapsed;
                    if (deltaTime.TotalMilliseconds > AlertTimeout)
                    {
                        FConsole.WriteWarnWithCategory("Action", "{0} is timeout, {1}ms.", TypeName, (int)deltaTime.TotalMilliseconds);
                    }
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory("Action", "An error occurred on process action", e);
            }
            if (watch != null)
            {
                watch.Reset();
            }
        }

        /// <summary>
        /// 判断有效性
        /// </summary>
        public virtual bool Check()
        {
            return true;
        }

        /// <summary>
        /// 消息预处理
        /// </summary>
        public virtual void SetReader(IMessageReader reader) { }

        public abstract void TakeAction(IMessageReader reader);
    }
}
