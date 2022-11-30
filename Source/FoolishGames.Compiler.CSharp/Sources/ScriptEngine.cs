/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2025 ChenYiZh
https://space.bilibili.com/9308172

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
****************************************************************************/
using FoolishGames.IO;
using FoolishGames.Log;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
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
        /// <summary>
        /// 动态编译
        /// </summary>
        public static bool Compile(string scriptsPath, bool isDebug = false, string outputFile = null, string librariesPath = null)
        {
            return CompileByRoslyn(scriptsPath, isDebug, outputFile, librariesPath);
        }

        #region mono
        /// <summary>
        /// 使用Mono动态编译
        /// <para>https://www.cnblogs.com/zhongzf/archive/2011/11/27/2264955.html</para>
        /// <para>https://www.mono-project.com/docs/about-mono/languages/csharp/</para>
        /// </summary>
        [Obsolete("不兼容.NetStandard")]
        public static Assembly CompileByMono(string scriptsPath, bool isDebug = false, string outputFile = null, string librariesPath = null)
        {
            throw new PlatformNotSupportedException("Only on .Net Framework.");
        }
        #endregion

        #region roslyn
        /// <summary>
        /// 使用Roslyn动态编译
        /// <para>https://blog.csdn.net/Crazy2910/article/details/106918516</para>
        /// </summary>
        public static bool CompileByRoslyn(string scriptsPath, bool isDebug = false, string outputFile = null, string librariesPath = null)
        {
            //路径检查
            outputFile = CheckOutputFilePath(outputFile);
            string assemblyName = Path.GetFileNameWithoutExtension(outputFile);
            string pdbFilePath = outputFile.Substring(0, outputFile.Length - 4) + ".pdb";
            librariesPath = FPath.GetFullPath(librariesPath);

            //获取文件
            string[] scripts = Directory.GetFiles(FPath.GetFullPath(scriptsPath), "*.cs", SearchOption.AllDirectories);
            string[] dlls = Directory.GetFiles(FPath.GetFullPath(librariesPath), "*.dll", SearchOption.AllDirectories);

            //读取需要忽略的dll
            List<string> ignoreDlls = GetIgnoreReferences();
            ignoreDlls.Add(Path.GetFileName(outputFile));

            //源码读取
            LinkedList<SyntaxTree> synctaxTrees = new LinkedList<SyntaxTree>();
            foreach (string file in scripts)
            {
                string script = File.ReadAllText(file, Encoding.UTF8);
                SyntaxTree synctaxTree = CSharpSyntaxTree.ParseText(script, CSharpParseOptions.Default, file, Encoding.UTF8);
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
                //FConsole.Write(Path.GetFileName(file));
                MetadataReference reference = MetadataReference.CreateFromFile(file);
                references.Add(reference);
            }

            //编译设置
            CSharpCompilationOptions options = new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: (isDebug ? OptimizationLevel.Debug : OptimizationLevel.Release),
                platform: Platform.AnyCpu,
                moduleName: Path.GetFileNameWithoutExtension(outputFile)
            //publicSign: true
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
                FConsole.WriteErrorFormatWithCategory(Categories.COMPILER, "Failed to compile C# scripts.");
                foreach (Diagnostic diagnostic in result.Diagnostics)
                {
                    if (diagnostic.WarningLevel == 0)
                    {
                        FConsole.WriteErrorFormatWithCategory(Categories.COMPILER, diagnostic.GetMessage());
                    }
                }
                return false;
            }
            FConsole.WriteInfoFormatWithCategory(Categories.COMPILER, "C# compiled successfully.");
            return true;
        }
        #endregion


        #region .net framework
        /// <summary>
        /// 编译cs文件
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/microsoft.csharp.csharpcodeprovider</para>
        /// </summary>
        [Obsolete("不兼容NetStandard")]
        public static Assembly CompileByCodeDom(string scriptsPath, bool isDebug = false, string outputFile = null, string librariesPath = null)
        {
            throw new PlatformNotSupportedException("Only on .Net Framework.");
            //CodeDomProvider provider = new CSharpCodeProvider();
            //if (provider == null)
            //{
            //    FConsole.WriteErrorWithCategory(Categories.COMPILER, "Failed to generate CSharpCodeProvider.");
            //    return null;
            //}

            //if (string.IsNullOrEmpty(outputFile))
            //{
            //    outputFile = "FoolishGames.Runtime.dll";
            //}
            //if (!outputFile.EndsWith(".dll"))
            //{
            //    outputFile += ".dll";
            //}
            //CompilerParameters options = new CompilerParameters();
            //options.GenerateExecutable = false;
            //options.GenerateInMemory = false;
            //options.TreatWarningsAsErrors = false;
            //options.OutputAssembly = outputFile;
            //if (string.IsNullOrEmpty(librariesPath))
            //{
            //    librariesPath = Environment.CurrentDirectory;
            //}
            //else
            //{
            //    librariesPath = Path.Combine(Environment.CurrentDirectory, librariesPath);
            //}

            //string[] dlls = Directory.GetFiles(librariesPath, "*.dll");
            //foreach (string dll in dlls)
            //{
            //    options.ReferencedAssemblies.Add(Path.Combine(librariesPath, Path.GetFileName(dll)));
            //}

            //CompilerResults result = provider.CompileAssemblyFromSource(options, scriptsPath);
            //if (result.Errors.Count > 0)
            //{
            //    FConsole.WriteErrorWithCategory(Categories.COMPILER, "Failed to compile C# scripts.");
            //    foreach (CompilerError error in result.Errors)
            //    {
            //        FConsole.WriteErrorWithCategory(Categories.COMPILER, "  {0}", error.ToString());
            //    }
            //    return null;
            //}
            //FConsole.WriteInfoWithCategory(Categories.COMPILER, "C# compiled successfully.");
            //return result.CompiledAssembly;
        }
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
