using FoolishGames.IO;
using FoolishGames.Log;
using FoolishServer.Common;
using FoolishServer.Config;
using FoolishServer.Model;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FoolishServer.Runtime
{
    /// <summary>
    /// 代码注入器，依赖Mono
    /// </summary>
    public static class ILInjection
    {
        /// <summary>
        /// 注入监听Model发生变化的代码
        /// </summary>
        /// <returns></returns>
        public static bool InjectEntityChangeEvent()
        {
            //判断dll是否存在
            string assemblyFullname = FPath.GetFullPath(Settings.AssemblyName);
            if (!File.Exists(assemblyFullname))
            {
                throw new FileNotFoundException(string.Format("Assembly not found: {0}", assemblyFullname));
            }

            //获取pdb文件，判断是否Debug
            string pdbFullname = Path.ChangeExtension(assemblyFullname, "pdb");
            PdbReaderProvider readerProvider = null;
            PdbWriterProvider writerProvider = null;
            bool isDebug = false;
            if (File.Exists(pdbFullname))
            {
                isDebug = true;
                readerProvider = new PdbReaderProvider();
                writerProvider = new PdbWriterProvider();
            }

            //加载assembly的描述文件，使用debug在于注入代码后断点位置会发生变化
            AssemblyDefinition definition = AssemblyDefinition.ReadAssembly(assemblyFullname, new ReaderParameters()
            {
                SymbolReaderProvider = readerProvider,
                ReadSymbols = isDebug
            });

            //添加引用的搜索目录，由于保存dll时会检索引用
            BaseAssemblyResolver resolver = definition.MainModule.AssemblyResolver as BaseAssemblyResolver;
            if (resolver != null)
            {
                resolver.AddSearchDirectory(Environment.CurrentDirectory);
            }

            //判断有没有脚本发生变化
            bool hasChanged = false;

            //搜寻基类
            string entityName = typeof(Entity).FullName;
            string refDll = Path.Combine(Environment.CurrentDirectory, "FoolishServer.Common.dll");
            ModuleDefinition module = ModuleDefinition.ReadModule(refDll);
            TypeDefinition entityType = module.GetType(entityName);
            MethodDefinition notifyMethod = null;
            foreach (MethodDefinition method in entityType.Methods)
            {
                if (method.Name == "NotifyPropertyModified")
                {
                    notifyMethod = method;
                    break;
                }
            }
            FieldDefinition syncRootField = null;
            foreach (FieldDefinition field in entityType.Fields)
            {
                if (field.Name == "SyncRoot")
                {
                    syncRootField = field;
                    break;
                }
            }

            //获取所有类型进行处理
            IEnumerable<TypeDefinition> types = definition.MainModule.GetTypes();
            foreach (TypeDefinition type in types)
            {
                if (ProcessEntityType(type, entityType, syncRootField, notifyMethod))
                {
                    hasChanged = true;
                }
            }

            //保存注入后程序集
            if (hasChanged)
            {
                definition.Write(assemblyFullname + ".dll", new WriterParameters()
                {
                    SymbolWriterProvider = writerProvider,
                    WriteSymbols = isDebug
                });
            }

            return hasChanged;
        }

        /// <summary>
        /// 向Model层注入代码
        /// </summary>
        private static bool ProcessEntityType(TypeDefinition type, TypeDefinition entityType, FieldDefinition syncRootField, MethodDefinition notifyMethod)
        {
            bool hasChanged = false;
            if (type.IsChildOf(entityType.FullName))
            {
                foreach (PropertyDefinition property in type.Properties)
                {
                    //if (property.ContainsAttribute("EntityFieldAttribute"))
                    //{
                    //    if (InjectAopCode(type, property, syncRootField, notifyMethod))
                    //    {
                    //        hasChanged = true;
                    //    }
                    //}
                    if (!property.ContainsAttribute("EntityFieldAttribute"))
                    {
                        var setMethod = property.SetMethod;
                        foreach (var body in setMethod.Body.Instructions)
                        {
                            FConsole.Write(body.ToString());
                        }
                    }
                }
            }
            return hasChanged;
        }

        /// <summary>
        /// 注入修改监听代码
        /// </summary>
        private static bool InjectAopCode(TypeDefinition type, PropertyDefinition property, FieldDefinition syncRootField, MethodDefinition notifyMethod)
        {
            FConsole.Write("{0} : {1} : {2}", type.Name, property.Name, property.SetMethod.HasBody);
            MethodDefinition setMethod = property.SetMethod;
            if (setMethod.Body.Instructions.Count > 5)
            {
                return false;
                FConsole.Write("setMethod.Body.Instructions.Count > 0");
                foreach (Instruction inBody in setMethod.Body.Instructions)
                {
                    FConsole.Write(inBody.OpCode.Name);
                }
            }
            while (setMethod.Body.Instructions.Count > 1)
            {
                setMethod.Body.Instructions.RemoveAt(0);
            }

            /**
             * 注入的准备工作
             */
            //寻找私有变量
            string propertyName = property.Name;
            FieldDefinition field = null;
            if (type.HasFields)
            {
                foreach (FieldDefinition itsField in type.Fields)
                {
                    if (itsField.Name.StartsWith("<" + propertyName + ">") || string.Equals(itsField.Name, "_" + propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        field = itsField;
                        break;
                    }
                }
            }
            if (field == null)
            {
                return false;
            }

            MethodReference method = type.Module.ImportReference(notifyMethod);
            TypeReference paramType = setMethod.Parameters[0].ParameterType;
            ILProcessor worker = setMethod.Body.GetILProcessor();
            Instruction ins = setMethod.Body.Instructions[setMethod.Body.Instructions.Count - 1];
            MethodReference equalsMethod = type.Module.ImportReference(typeof(Object).GetMethod("Equals", new Type[] { typeof(object), typeof(object) }));
            FieldReference syncRoot = type.Module.ImportReference(syncRootField);
            setMethod.Body.Variables.Add(new VariableDefinition(equalsMethod.ReturnType));

            //开始注入
            worker.InsertBefore(ins, worker.Create(OpCodes.Nop));

            //Object.Equals(A,B)
            worker.InsertBefore(ins, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(ins, worker.Create(OpCodes.Ldfld, field));
            worker.InsertBefore(ins, worker.Create(OpCodes.Box, paramType));
            worker.InsertBefore(ins, worker.Create(OpCodes.Ldarg_1));
            worker.InsertBefore(ins, worker.Create(OpCodes.Box, paramType));
            worker.InsertBefore(ins, worker.Create(OpCodes.Call, equalsMethod));

            //添加判断
            worker.InsertBefore(ins, worker.Create(OpCodes.Stloc_0));
            worker.InsertBefore(ins, worker.Create(OpCodes.Ldloc_0));
            worker.InsertBefore(ins, worker.Create(OpCodes.Brtrue_S, ins));

            //不相等时赋值
            worker.InsertBefore(ins, worker.Create(OpCodes.Nop));
            worker.InsertBefore(ins, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(ins, worker.Create(OpCodes.Ldarg_1));
            worker.InsertBefore(ins, worker.Create(OpCodes.Stfld, field));

            //调用通知函数
            worker.InsertBefore(ins, worker.Create(OpCodes.Ldarg_0));
            //worker.InsertBefore(ins, worker.Create(OpCodes.Ldarg_0));
            //worker.InsertBefore(ins, worker.Create(OpCodes.Ldfld, field));
            //worker.InsertBefore(ins, worker.Create(OpCodes.Box, paramType));
            //if (notifyMethod.Parameters.Count == 2)
            //{
            worker.InsertBefore(ins, worker.Create(OpCodes.Ldstr, propertyName));
            //}
            worker.InsertBefore(ins, worker.Create(OpCodes.Call, method));
            //worker.InsertBefore(ins, worker.Create(OpCodes.Nop));
            worker.InsertBefore(ins, worker.Create(OpCodes.Nop));
            worker.InsertBefore(ins, worker.Create(OpCodes.Ret));

            return true;
        }

        private static List<Instruction> GenerateSetMethod()
        {
            return null;
        }
    }
}
