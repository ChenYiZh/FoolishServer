using FoolishGames.Log;
using FoolishServer.Config;
using FoolishServer.Log;
using FoolishServer.RPC;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FoolishServer.Runtime
{
    public static class RuntimeHost
    {
        public static bool IsRunning { get; private set; } = false;

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
        /// 启动函数
        /// </summary>
        public static void Startup()
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
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(string.Format(START_INFO,
                Assembly.GetExecutingAssembly().GetName().Version,
                GetOSBit(),
                GetRunPlatform(),
                Settings.GetVersion(),
                Settings.ServerID));
            Console.ForegroundColor = color;
            //起服
            foreach (IHostSetting setting in Settings.HostSettings)
            {
                ServerManager.Start(setting);
            }
            FConsole.WriteInfoWithCategory(Categories.FOOLISH_SERVER, "Foolish Server exit command \"Ctrl+C\" or \"Ctrl+Break\".");
        }

        public static void Shutdown()
        {
            IsRunning = false;
            ServerManager.Shutdown();
        }

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

        private static void RegistConsoleCancelKeyPress()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            try
            {
                FConsole.WriteInfoWithCategory(Categories.FOOLISH_SERVER, "RuntimeHost begin to shutdown...");
                Shutdown();
                FConsole.WriteInfoWithCategory(Categories.FOOLISH_SERVER, "RuntimeHost has closed.");
                Task.Delay(1500).Wait();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                FConsole.WriteError("OnCancelKeyPress error:{1}", ex);
            }
        }
    }
}
