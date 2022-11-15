using FoolishGames.Log;
using System;

namespace DynamicCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FConsole.Write("Hello World!");
            try
            {
                string a = "";
                int c = 2 / a.Length;
                Console.WriteLine(c);
            }
            catch (Exception e)
            {
                FConsole.WriteException(e);
            }
        }
    }
}
