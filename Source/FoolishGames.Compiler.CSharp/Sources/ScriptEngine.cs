using FoolishGames.IO;
using FoolishGames.Log;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace FoolishGames.Compiler.CSharp
{
    /// <summary>
    /// C#编译工具
    /// </summary>
    public static class ScriptEngine
    {
        #region roslyn
        /// <summary>
        /// 使用Roslyn动态编译
        /// <para>https://blog.csdn.net/Crazy2910/article/details/106918516</para>
        /// </summary>
        public static Assembly Compile(string scriptsPath, bool isDebug = false, string outputFile = null, string librariesPath = null)
        {
            //路径检查
            outputFile = CheckOutputFilePath(outputFile);
            string assemblyName = Path.GetFileNameWithoutExtension(outputFile);
            string pdbFilePath = outputFile.Substring(0, outputFile.Length - 4) + ".pdb";
            librariesPath = FPath.GetFullPath(librariesPath);

            //获取文件
            string[] scripts = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, scriptsPath), "*.cs", SearchOption.AllDirectories);
            string[] dlls = Directory.GetFiles(librariesPath, "*.dll", SearchOption.AllDirectories);

            //读取需要忽略的dll
            List<string> ignoreDlls = GetIgnoreReferences();
            ignoreDlls.Add(Path.GetFileName(outputFile));

            //源码读取
            LinkedList<SyntaxTree> synctaxTrees = new LinkedList<SyntaxTree>();
            foreach (string file in scripts)
            {
                string script = File.ReadAllText(file, Encoding.UTF8);
                SyntaxTree synctaxTree = CSharpSyntaxTree.ParseText(script);
                synctaxTrees.AddLast(synctaxTree);
            }
            //synctaxTrees.AddFirst(CSharpSyntaxTree.ParseText(GenerateAssemblyInfo(assemblyName)));

            //添加用于编译的引用
            List<MetadataReference> references = new List<MetadataReference>(dlls.Length);
            foreach (string file in dlls)
            {
                if (ignoreDlls.Contains(Path.GetFileName(file)))
                {
                    continue;
                }
                MetadataReference reference = MetadataReference.CreateFromFile(file);
                references.Add(reference);
            }

            //编译设置
            CSharpCompilationOptions options = new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: (isDebug ? OptimizationLevel.Debug : OptimizationLevel.Release),
                platform: Platform.AnyCpu
            );

            //创建编译器
            Compilation compiler = CSharpCompilation.Create(assemblyName, synctaxTrees, references, options);

            //编译
            EmitResult result = compiler.Emit(outputFile, isDebug ? pdbFilePath : null);
            //EmitResult result = null;
            //using (FileStream dll = new FileStream(outputFile, FileMode.Create))
            //{
            //    FileStream pdb = null;
            //    if (isDebug)
            //    {
            //        pdb = new FileStream(pdbFilePath, FileMode.Create);
            //    }
            //    using (Stream res = compiler.CreateDefaultWin32Resources(true, false, null, null))
            //    {
            //        result = compiler.Emit(dll, pdb, res);
            //    }
            //    if (pdb != null)
            //    {
            //        pdb.Dispose();
            //    }
            //}

            //判断
            if (!result.Success)
            {
                FConsole.WriteErrorWithCategory(Categories.COMPILER, "Failed to compile C# scripts.");
                foreach (Diagnostic diagnostic in result.Diagnostics)
                {
                    if (diagnostic.WarningLevel == 0)
                    {
                        FConsole.WriteErrorWithCategory(Categories.COMPILER, diagnostic.GetMessage());
                    }
                }
                return null;
            }
            FConsole.WriteInfoWithCategory(Categories.COMPILER, "C# compiled successfully.");
            return Assembly.LoadFrom(outputFile);
        }
        #endregion


        #region .net 4.6+
        ///// <summary>
        ///// 编译cs文件
        ///// <para>https://learn.microsoft.com/zh-cn/dotnet/api/microsoft.csharp.csharpcodeprovider</para>
        ///// </summary>
        //public static Assembly Compile(string scriptsPath, bool isDebug = false, string outputFile = null, string librariesPath = null)
        //{
        //    CodeDomProvider provider = new CSharpCodeProvider();
        //    if (provider == null)
        //    {
        //        FConsole.WriteErrorWithCategory(Categories.COMPILER, "Failed to generate CSharpCodeProvider.");
        //        return null;
        //    }

        //    if (string.IsNullOrEmpty(outputFile))
        //    {
        //        outputFile = "FoolishGames.Runtime.dll";
        //    }
        //    if (!outputFile.EndsWith(".dll"))
        //    {
        //        outputFile += ".dll";
        //    }
        //    CompilerParameters options = new CompilerParameters();
        //    options.GenerateExecutable = false;
        //    options.GenerateInMemory = false;
        //    options.TreatWarningsAsErrors = false;
        //    options.OutputAssembly = outputFile;
        //    if (string.IsNullOrEmpty(librariesPath))
        //    {
        //        librariesPath = Environment.CurrentDirectory;
        //    }
        //    else
        //    {
        //        librariesPath = Path.Combine(Environment.CurrentDirectory, librariesPath);
        //    }

        //    string[] dlls = Directory.GetFiles(librariesPath, "*.dll");
        //    foreach (string dll in dlls)
        //    {
        //        options.ReferencedAssemblies.Add(Path.Combine(librariesPath, Path.GetFileName(dll)));
        //    }

        //    CompilerResults result = provider.CompileAssemblyFromSource(options, scriptsPath);
        //    if (result.Errors.Count > 0)
        //    {
        //        FConsole.WriteErrorWithCategory(Categories.COMPILER, "Failed to compile C# scripts.");
        //        foreach (CompilerError error in result.Errors)
        //        {
        //            FConsole.WriteErrorWithCategory(Categories.COMPILER, "  {0}", error.ToString());
        //        }
        //        return null;
        //    }
        //    FConsole.WriteInfoWithCategory(Categories.COMPILER, "C# compiled successfully.");
        //    return result.CompiledAssembly;
        //}
        #endregion

        /// <summary>
        /// 输出路径检查
        /// </summary>
        private static string CheckOutputFilePath(string outputFile)
        {
            if (string.IsNullOrEmpty(outputFile))
            {
                outputFile = "FoolishGames.Runtime.dll";
            }
            if (!outputFile.EndsWith(".dll"))
            {
                outputFile += ".dll";
            }
            return FPath.GetFullPath(outputFile);
        }

        /// <summary>
        /// 生成版本字符串
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        private static string GenerateAssemblyInfo(string assemblyName)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("using System.Reflection;");
            builder.AppendLine("using System.Runtime.CompilerServices;");
            builder.AppendLine("using System.Runtime.InteropServices;");

            builder.AppendLine($"[assembly: AssemblyTitle(\"{assemblyName}\")]");
            builder.AppendLine($"[assembly: AssemblyProduct(\"{assemblyName}\")]");
            builder.AppendLine($"[assembly: AssemblyCopyright(\"Copyright © FoolishGames.\")]");
            builder.AppendLine($"[assembly: ComVisible(false)]");
            builder.AppendLine($"[assembly: Guid(\"{Guid.NewGuid().ToString()}\")]");
            builder.AppendLine($"[assembly: AssemblyVersion(\"1.0.0.1\")]");
            return builder.ToString();
        }

        private static List<string> GetIgnoreReferences()
        {
            string xmlFileName = "FoolishGames.CSharp.Ignored.References.config";
            if (!File.Exists(xmlFileName))
            {
                return new List<string>();
            }
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlDocument xml = new XmlDocument();
            using (XmlReader reader = XmlReader.Create(xmlFileName, settings))
            {
                xml.Load(reader);
            }
            XmlNode node = xml.SelectSingleNode("references");
            List<string> dlls = new List<string>(node.ChildNodes.Count + 1);
            foreach (XmlNode child in node)
            {
                dlls.Add(child.InnerText);
            }
            return dlls;
        }
    }
}
