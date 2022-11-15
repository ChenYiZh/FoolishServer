using FoolishGames.Compiler.CSharp;
using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Execution
{
    class Program
    {
        static void Main(string[] args)
        {
            FConsole.RegistLogger(new Logger());
            try
            {
                Assembly assembly = ScriptEngine.Compile("../Scripts", true);
                Type typeClass = assembly.GetType("DynamicCode.Program");
                MethodInfo method = typeClass.GetMethod("Main");
                method.Invoke(null, new object[] { null });
            }
            catch (Exception e)
            {
                FConsole.WriteException(e);
            }
            Console.ReadLine();
        }
    }
}
