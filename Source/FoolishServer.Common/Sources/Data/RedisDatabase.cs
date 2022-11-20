using FoolishGames.Log;
using FoolishServer.Config;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace FoolishServer.Data
{
    /// <summary>
    /// Redis 链接池
    /// </summary>
    public class RedisDatabase : IDatabase
    {
        /// <summary>
        /// Redis连接池，这是一个池子
        /// </summary>
        public ConnectionMultiplexer Redis { get; private set; } = null;
        /// <summary>
        /// 配置信息
        /// </summary>
        public IRedisSetting Setting { get; private set; }
        /// <summary>
        /// 是否还连接着
        /// </summary>
        public bool Connected { get { return Redis != null && Redis.IsConnected; } }

        /// <summary>
        /// 心跳检测
        /// </summary>
        private Timer HeartbeatTime = null;
        /// <summary>
        /// Redis的连接服务器
        /// </summary>
        private IServer RedisServer { get; set; }
        /// <summary>
        /// 连接中的数据库
        /// </summary>
        private StackExchange.Redis.IDatabase Database { get; set; }

        public RedisDatabase(IRedisSetting setting)
        {
            Setting = setting;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            FConsole.WriteWithCategory(Categories.REDIS, "Redis closing...");
            if (HeartbeatTime != null)
            {
                try
                {
                    HeartbeatTime.Dispose();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.REDIS, e);
                }
                HeartbeatTime = null;
            }
            if (Redis != null)
            {
                if (RedisServer != null)
                {
                    try
                    {
                        //阻塞当前线程，强制Redis落地
                        RedisServer.Save(SaveType.ForegroundSave);
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Categories.REDIS, e);
                    }
                }
                try
                {
                    Redis.Close();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.REDIS, e);
                }
                Redis = null;
                Redis = null;
                Database = null;
            }
        }
        /// <summary>
        /// 连接函数
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            FConsole.WriteInfoWithCategory(Categories.REDIS, "Redis connecting");
            ConfigurationOptions options = new ConfigurationOptions();
            options.get_EndPoints().Add(new IPEndPoint(IPAddress.Parse(Setting.Host), Setting.Port));
            options.Password = Setting.Password;
            options.ConnectTimeout = Setting.Timeout;
            options.DefaultDatabase = Setting.DbIndex;
            //退出时强制读写需要用到
            options.AllowAdmin = true;
            if (Redis == null)
            {
                try
                {
                    Redis = ConnectionMultiplexer.Connect(options);
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.REDIS, e);
                }
            }
            else if (!Connected)
            {
                try
                {
                    Redis.Close();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.REDIS, e);
                }
                try
                {
                    Redis = ConnectionMultiplexer.Connect(options);
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.REDIS, e);
                }
            }
            if (Connected && HeartbeatTime == null)
            {
                HeartbeatTime = new Timer(Ping, this, 60000, 60000);
            }
            if (Connected)
            {
                RedisServer = Redis.GetServer(Setting.Host, Setting.Port);
                Database = Redis.GetDatabase(Setting.DbIndex);
                FConsole.WriteInfoFormatWithCategory(Categories.REDIS, "Redis[{0}] connected.", Setting.DbIndex);
            }
            return Connected;
        }
        /// <summary>
        /// 心跳检测
        /// </summary>
        /// <param name="state"></param>
        private void Ping(object state)
        {
            try
            {
                RedisServer.Ping();
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.REDIS, e);
            }
        }

        /// <summary>
        /// 操作一堆数据
        /// </summary>
        public bool CommitModifiedEntitys(IEnumerable<DbCommition> commitions)
        {
            foreach (DbCommition commition in commitions)
            {
                FConsole.Write(commition.Key + ": " + commition.ModifyType);
            }
            return false;
        }
    }
}
