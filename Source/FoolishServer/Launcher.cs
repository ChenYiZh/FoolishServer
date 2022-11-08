using FoolishServer.Config;
using FoolishServer.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FoolishServer
{
    /// <summary>
    /// 启动类
    /// </summary>
    public class Launcher
    {
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
            GConsole.RegistLogger(new NLogger());
            Configeration.Initialize();
            GConsole.WriteLine(LogLevel.INFO, string.Format(START_INFO,
                Assembly.GetExecutingAssembly().GetName().Version,
                GetOSBit(),
                GetRunPlatform(),
                Settings.GetVersion(),
                Settings.ServerID));
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
    }
}
