using FoolishGames.Log;
using FoolishGames.Reflection;
using FoolishServer.Config;
using FoolishServer.Data;
using FoolishServer.Log;
using FoolishServer.RPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FoolishServer.Runtime
{
    /// <summary>
    /// 运行时
    /// </summary>
    public static class RuntimeHost
    {
        /// <summary>
        /// 是否正在运行
        /// </summary>
        public static bool IsRunning { get; private set; } = false;

        /// <summary>
        /// 启动字符串
        /// </summary>
        const string START_INFO =
@"////////////////////////////////////////////////////////////////////////////////////////////////

  !###################################@:         
!##$`                                ;@#@;
#@:                                    |#@
#$`     `$#####$`         .%#$|%@@:    ;##
#$`   .%#|:::::|#%.      '&%:::::!&!   ;##
#$`  .%&!:::::::;&%.     |$;::::`      ;##
#$`  ;@|'::::::'.        |$::::`       ;##
#$`  |&!:::::`           :&|'::::;!'   ;##
#$`  |@!:::::'.           '&&;':%@|    ;##
#$`  :@%::::::::'            ';:.      ;##
#$`   !#|':::::'|@|                    ;##          Foolish Server {0} ({1} bit)
#$`    ;#&;':';&#;        '|&####@:    ;##          Running in {2} platform
#$`      '$###$'      !###########$`   ;##          Game: {3}   Server: {4}
@#;                    .';!|!;:.      .%#&    
:@#@|`..............................'$##$'                      http://www.chenyizh.cn
  '$##################################|.              

";

        /// <summary>
        /// 输出启动信息
        /// </summary>
        internal static void PrintStartInfo()
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(string.Format(START_INFO,
                Assembly.GetExecutingAssembly().GetName().Version,
                GetOSBit(),
                GetRunPlatform(),
                Settings.GetVersion(),
                Settings.ServerID));
            Console.ForegroundColor = color;
        }

        /// <summary>
        /// 自定义运行时
        /// </summary>
        public static IRuntime CustomRuntime { get; private set; }

        /// <summary>
        /// 启动函数
        /// </summary>
        public static void Startup()
        {
            try
            {
                if (IsRunning)
                {
                    return;
                }
                IsRunning = true;
                //注册关闭事件
                RegistConsoleCancelKeyPress();
                //注册日志
                FConsole.RegistLogger(new NLogger());
                //初始化配置
                Configeration.Initialize();

                //创建自定义脚本
                if (!string.IsNullOrEmpty(Settings.MainClass))
                {
                    CustomRuntime = ObjectFactory.Create<IRuntime>(Settings.MainClass);
                    if (CustomRuntime == null)
                    {
                        throw new Exception(Settings.MainClass + " is not exists.");
                    }
                }

                //自定义脚本初始启动
                if (CustomRuntime != null)
                {
                    CustomRuntime.OnStartup();
                }

                //数据库信息初始化
                DataContext.Initialize();
                //自定义脚本初始启动
                if (CustomRuntime != null)
                {
                    CustomRuntime.OnDatebaseInitialized();
                }
                //开始加载数据
                DataContext.Start();
                //准备起服
                if (CustomRuntime != null)
                {
                    CustomRuntime.ReadyToStartServers();
                }
                //起服
                foreach (IHostSetting setting in Settings.HostSettings)
                {
                    ServerManager.Start(setting);
                }
                FConsole.WriteWarnFormatWithCategory(Categories.FOOLISH_SERVER, "Foolish Server exit command \"Ctrl+C\" or \"Ctrl+Break\".");

                if (CustomRuntime != null)
                {
                    CustomRuntime.OnBegun();
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.FOOLISH_SERVER, "Failed to start the server.", e);
            }
        }

        /// <summary>
        /// 关闭操作
        /// </summary>
        public static void Shutdown()
        {
            if (CustomRuntime != null)
            {
                CustomRuntime.OnShutdown();
            }
            IsRunning = false;
            FConsole.WriteWarnFormatWithCategory(Categories.FOOLISH_SERVER, "RuntimeHost begin to shutdown...");
            //先关闭所有玩家连接
            ServerManager.Shutdown();
            //后关闭数据连接，因为需要保存
            DataContext.Shutdown();
            FConsole.WriteWithCategory(Categories.FOOLISH_SERVER, "RuntimeHost has closed.");
            Task.Delay(1500).Wait();
        }

        /// <summary>
        /// 系统框架
        /// </summary>
        /// <returns></returns>
        private static int GetOSBit()
        {
            try
            {
                return Environment.Is64BitProcess ? 64 : 32;
            }
            catch (Exception)
            {
                return 32;
            }
        }

        /// <summary>
        /// 运行平台
        /// </summary>
        /// <returns></returns>
        private static string GetRunPlatform()
        {
            try
            {
                return Environment.OSVersion.Platform.ToString();
            }
            catch (Exception)
            {
                return "Unknow";
            }
        }

        /// <summary>
        /// 注册退出指令
        /// </summary>
        private static void RegistConsoleCancelKeyPress()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);
        }

        /// <summary>
        /// 输入退出指令时执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            try
            {
                Shutdown();
                if (CustomRuntime != null)
                {
                    CustomRuntime.OnKilled();
                }
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                FConsole.WriteErrorFormat("OnCancelKeyPress error:{1}", ex);
            }
        }

        /// <summary>
        /// 重启应用
        /// </summary>
        internal static void Reboot()
        {
            FConsole.WriteWarnFormatWithCategory(Categories.FOOLISH_SERVER, "begin to reboot...");
            Shutdown();
            if (CustomRuntime != null)
            {
                CustomRuntime.OnKilled();
            }
            Process.Start(Process.GetCurrentProcess().ProcessName, Environment.CommandLine);
            Process.GetCurrentProcess().Kill();
        }
    }
}
