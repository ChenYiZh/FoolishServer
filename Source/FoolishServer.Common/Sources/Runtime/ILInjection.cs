using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishServer.Common;
using FoolishServer.Config;
using FoolishServer.Log;
using FoolishServer.Struct;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
                ReadSymbols = isDebug,
                InMemory = true
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
            string entityName = FType<Entity>.Type.FullName;
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

            if (notifyMethod == null || syncRootField == null)
            {
                FConsole.WriteErrorFormatWithCategory(Categories.FOOLISH_SERVER, "The class Entity was not found!");
                return false;
            }

            try
            {
                MethodTypes = new GlobalMethodTypes(definition.MainModule, syncRootField, notifyMethod);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.FOOLISH_SERVER, e);
                return false;
            }

            //获取所有类型进行处理
            IEnumerable<TypeDefinition> types = definition.MainModule.GetTypes();
            foreach (TypeDefinition type in types)
            {
                if (ProcessEntityType(type, entityType))
                {
                    hasChanged = true;
                }
            }

            //保存注入后程序集
            if (hasChanged)
            {
                definition.Write(assemblyFullname, new WriterParameters()
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
        private static bool ProcessEntityType(TypeDefinition type, TypeDefinition entityType)
        {
            bool hasChanged = false;
            if (type.IsChildOf(entityType.FullName))
            {
                foreach (PropertyDefinition property in type.Properties)
                {
                    if (property.ContainsAttribute("EntityFieldAttribute"))
                    {
                        if (InjectAopCode(type, property))
                        {
                            //FConsole.WriteWarnWithCategory(Categories.MODEL, "{0} modified.", type.FullName);
                            hasChanged = true;
                        }
                    }
                }
            }
            return hasChanged;
        }

        /// <summary>
        /// 注入修改监听代码
        /// </summary>
        private static bool InjectAopCode(TypeDefinition type, PropertyDefinition property)
        {
            MethodDefinition setMethod = property.SetMethod;
            MethodDefinition getMethod = property.GetMethod;
            if (setMethod == null || setMethod.Body.Instructions.Count > 5 || getMethod == null || getMethod.Body.Instructions.Count > 5)
            {
                return false;
            }

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

            //Set方法注入
            GenerateSetMethod(setMethod, field, propertyName);

            //Get方法注入
            GenerateGetMethod(getMethod, field, propertyName);

            return true;
        }

        /// <summary>
        /// 全局需要用的引用
        /// </summary>
        private struct GlobalMethodTypes
        {
            /// <summary>
            /// bool
            /// </summary>
            public TypeReference BoolType { get; private set; }

            /// <summary>
            /// object
            /// </summary>
            public TypeReference ObjectType { get; private set; }

            /// <summary>
            /// 同步锁的对象
            /// </summary>
            public FieldReference SyncRootField { get; private set; }

            /// <summary>
            /// 通知函数引用
            /// </summary>
            public MethodReference NotifyMethod { get; private set; }

            /// <summary>
            /// Monitor锁
            /// </summary>
            public MethodReference MonitorEnter { get; private set; }

            /// <summary>
            /// Monitor锁
            /// </summary>
            public MethodReference MonitorExit { get; private set; }

            /// <summary>
            /// Object.Equals(A,B)
            /// </summary>
            public MethodReference EqualsMethod { get; private set; }

            /// <summary>
            /// 赋值
            /// </summary>
            public GlobalMethodTypes(ModuleDefinition module, FieldDefinition syncRootField, MethodDefinition notifyMethod)
            {
                BoolType = module.ImportReference(FType<bool>.Type);
                ObjectType = module.ImportReference(FType<object>.Type);
                SyncRootField = module.ImportReference(syncRootField);
                NotifyMethod = module.ImportReference(notifyMethod);
                MethodInfo monitorEnterMethodInfo = null;
                MethodInfo[] monitorMethods = typeof(System.Threading.Monitor).GetMethods();
                foreach (MethodInfo method in monitorMethods)
                {
                    if (method.Name == "Enter" && method.GetParameters().Length == 2)
                    {
                        monitorEnterMethodInfo = method;
                        break;
                    }
                }
                if (monitorEnterMethodInfo == null)
                {
                    throw new Exception("The method: System.Threading.Monitor.Enter(object obj, ref bool lockToken); was not found!");
                }
                MonitorEnter = module.ImportReference(monitorEnterMethodInfo);
                MonitorExit = module.ImportReference(typeof(System.Threading.Monitor).GetMethod("Exit", new Type[] { typeof(object) }));
                EqualsMethod = module.ImportReference(FType<object>.Type.GetMethod("Equals", new Type[] { typeof(object), typeof(object) }));
            }
        }

        /// <summary>
        /// 全局引用
        /// </summary>
        private static GlobalMethodTypes MethodTypes;

        /// <summary>
        /// 注入Set函数
        /// </summary>
        private static void GenerateSetMethod(
            MethodDefinition setMethod,
            FieldDefinition privateField,
            string propertyName)
        {
            setMethod.Body.Variables.Clear();
            ILProcessor worker = setMethod.Body.GetILProcessor();
            TypeReference paramType = setMethod.Parameters[0].ParameterType;

            // .maxstack 4
            setMethod.Body.MaxStackSize = 4;

            // .locals init (object V_0,
            //          bool V_1,
            //          bool V_2,
            //          object V_3)
            setMethod.Body.InitLocals = true;
            VariableDefinition V_0 = new VariableDefinition(MethodTypes.ObjectType);
            VariableDefinition V_1 = new VariableDefinition(MethodTypes.BoolType);
            VariableDefinition V_2 = new VariableDefinition(MethodTypes.BoolType);
            VariableDefinition V_3 = new VariableDefinition(MethodTypes.ObjectType);
            setMethod.Body.Variables.Add(V_0);
            setMethod.Body.Variables.Add(V_1);
            setMethod.Body.Variables.Add(V_2);
            setMethod.Body.Variables.Add(V_3);

            //生成 IL代码
            Instruction IL_0000 = worker.Create(OpCodes.Nop);
            Instruction IL_0001 = worker.Create(OpCodes.Ldarg_0);
            Instruction IL_0002 = worker.Create(OpCodes.Ldfld, MethodTypes.SyncRootField);
            Instruction IL_0007 = worker.Create(OpCodes.Stloc_0);
            Instruction IL_0008 = worker.Create(OpCodes.Ldc_I4_0);
            Instruction IL_0009 = worker.Create(OpCodes.Stloc_1);

            Instruction IL_000a = worker.Create(OpCodes.Ldloc_0);
            Instruction IL_000b = worker.Create(OpCodes.Ldloca_S, V_1);
            Instruction IL_000d = worker.Create(OpCodes.Call, MethodTypes.MonitorEnter);

            Instruction IL_0012 = worker.Create(OpCodes.Nop);
            Instruction IL_0013 = worker.Create(OpCodes.Nop);
            Instruction IL_0014 = worker.Create(OpCodes.Ldarg_0);
            Instruction IL_0015 = worker.Create(OpCodes.Ldfld, privateField);
            Instruction IL_0015_1 = worker.Create(OpCodes.Box, paramType);
            Instruction IL_001a = worker.Create(OpCodes.Ldarg_1);
            Instruction IL_001a_1 = worker.Create(OpCodes.Box, paramType);
            Instruction IL_001b = worker.Create(OpCodes.Call, MethodTypes.EqualsMethod);

            Instruction IL_0020 = worker.Create(OpCodes.Ldc_I4_0);
            Instruction IL_0021 = worker.Create(OpCodes.Ceq);
            Instruction IL_0023 = worker.Create(OpCodes.Stloc_2);
            Instruction IL_0024 = worker.Create(OpCodes.Ldloc_2);


            Instruction IL_004a = worker.Create(OpCodes.Nop);

            Instruction IL_0025 = worker.Create(OpCodes.Brfalse_S, IL_004a);
            Instruction IL_0027 = worker.Create(OpCodes.Nop);
            Instruction IL_0028 = worker.Create(OpCodes.Ldarg_0);
            Instruction IL_0029 = worker.Create(OpCodes.Ldfld, privateField);
            Instruction IL_0029_1 = worker.Create(OpCodes.Box, paramType);
            Instruction IL_002e = worker.Create(OpCodes.Stloc_3);

            Instruction IL_002f = worker.Create(OpCodes.Ldarg_0);
            Instruction IL_0030 = worker.Create(OpCodes.Ldarg_1);
            Instruction IL_0031 = worker.Create(OpCodes.Stfld, privateField);

            Instruction IL_0036 = worker.Create(OpCodes.Ldarg_0);
            Instruction IL_0037 = worker.Create(OpCodes.Ldstr, propertyName);
            Instruction IL_003c = worker.Create(OpCodes.Ldloc_3);
            Instruction IL_003d = worker.Create(OpCodes.Ldarg_0);
            Instruction IL_003e = worker.Create(OpCodes.Ldfld, privateField);
            Instruction IL_003e_1 = worker.Create(OpCodes.Box, paramType);
            Instruction IL_0043 = worker.Create(OpCodes.Call, MethodTypes.NotifyMethod);


            Instruction IL_0048 = worker.Create(OpCodes.Nop);
            Instruction IL_0049 = worker.Create(OpCodes.Nop);


            Instruction IL_0058 = worker.Create(OpCodes.Ret);

            Instruction IL_004b = worker.Create(OpCodes.Leave_S, IL_0058);

            Instruction IL_004d = worker.Create(OpCodes.Ldloc_1);


            Instruction IL_0057 = worker.Create(OpCodes.Endfinally);

            Instruction IL_004e = worker.Create(OpCodes.Brfalse_S, IL_0057);
            Instruction IL_0050 = worker.Create(OpCodes.Ldloc_0);
            Instruction IL_0051 = worker.Create(OpCodes.Call, MethodTypes.MonitorExit);

            Instruction IL_0056 = worker.Create(OpCodes.Nop);

            //注入
            setMethod.Body.Instructions.Clear();
            setMethod.Body.Instructions.Add(IL_0000);
            setMethod.Body.Instructions.Add(IL_0001);
            setMethod.Body.Instructions.Add(IL_0002);
            setMethod.Body.Instructions.Add(IL_0007);
            setMethod.Body.Instructions.Add(IL_0008);
            setMethod.Body.Instructions.Add(IL_0009);
            setMethod.Body.Instructions.Add(IL_000a);
            setMethod.Body.Instructions.Add(IL_000b);
            setMethod.Body.Instructions.Add(IL_000d);
            setMethod.Body.Instructions.Add(IL_0012);
            setMethod.Body.Instructions.Add(IL_0013);
            setMethod.Body.Instructions.Add(IL_0014);
            setMethod.Body.Instructions.Add(IL_0015);
            setMethod.Body.Instructions.Add(IL_0015_1);
            setMethod.Body.Instructions.Add(IL_001a);
            setMethod.Body.Instructions.Add(IL_001a_1);
            setMethod.Body.Instructions.Add(IL_001b);
            setMethod.Body.Instructions.Add(IL_0020);
            setMethod.Body.Instructions.Add(IL_0021);
            setMethod.Body.Instructions.Add(IL_0023);
            setMethod.Body.Instructions.Add(IL_0024);
            setMethod.Body.Instructions.Add(IL_0025);
            setMethod.Body.Instructions.Add(IL_0027);
            setMethod.Body.Instructions.Add(IL_0028);
            setMethod.Body.Instructions.Add(IL_0029);
            setMethod.Body.Instructions.Add(IL_0029_1);
            setMethod.Body.Instructions.Add(IL_002e);
            setMethod.Body.Instructions.Add(IL_002f);
            setMethod.Body.Instructions.Add(IL_0030);
            setMethod.Body.Instructions.Add(IL_0031);
            setMethod.Body.Instructions.Add(IL_0036);
            setMethod.Body.Instructions.Add(IL_0037);
            setMethod.Body.Instructions.Add(IL_003c);
            setMethod.Body.Instructions.Add(IL_003d);
            setMethod.Body.Instructions.Add(IL_003e);
            setMethod.Body.Instructions.Add(IL_003e_1);
            setMethod.Body.Instructions.Add(IL_0043);
            setMethod.Body.Instructions.Add(IL_0048);
            setMethod.Body.Instructions.Add(IL_0049);
            setMethod.Body.Instructions.Add(IL_004a);
            setMethod.Body.Instructions.Add(IL_004b);
            setMethod.Body.Instructions.Add(IL_004d);
            setMethod.Body.Instructions.Add(IL_004e);
            setMethod.Body.Instructions.Add(IL_0050);
            setMethod.Body.Instructions.Add(IL_0051);
            setMethod.Body.Instructions.Add(IL_0056);
            setMethod.Body.Instructions.Add(IL_0057);
            setMethod.Body.Instructions.Add(IL_0058);

            //try-catch注入
            ExceptionHandler exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Finally);
            exceptionHandler.TryStart = IL_000a;
            exceptionHandler.TryEnd = IL_004d;
            exceptionHandler.HandlerStart = IL_004d;
            exceptionHandler.HandlerEnd = IL_0058;

            setMethod.Body.ExceptionHandlers.Clear();
            setMethod.Body.ExceptionHandlers.Add(exceptionHandler);
        }

        /// <summary>
        /// 注入Get函数
        /// </summary>
        private static void GenerateGetMethod(
            MethodDefinition getMethod,
            FieldDefinition privateField,
            string propertyName)
        {
            getMethod.Body.Variables.Clear();
            ILProcessor worker = getMethod.Body.GetILProcessor();

            // .maxstack 2
            getMethod.Body.MaxStackSize = 2;

            // .locals init (object V_0,
            //          bool V_1,
            //          retType V_2)
            getMethod.Body.InitLocals = true;
            VariableDefinition V_0 = new VariableDefinition(MethodTypes.ObjectType);
            VariableDefinition V_1 = new VariableDefinition(MethodTypes.BoolType);
            VariableDefinition V_2 = new VariableDefinition(getMethod.ReturnType);
            getMethod.Body.Variables.Add(V_0);
            getMethod.Body.Variables.Add(V_1);
            getMethod.Body.Variables.Add(V_2);

            //生成 IL代码
            Instruction IL_0000 = worker.Create(OpCodes.Nop);
            Instruction IL_0001 = worker.Create(OpCodes.Ldarg_0);
            Instruction IL_0002 = worker.Create(OpCodes.Ldfld, MethodTypes.SyncRootField);
            Instruction IL_0007 = worker.Create(OpCodes.Stloc_0);
            Instruction IL_0008 = worker.Create(OpCodes.Ldc_I4_0);
            Instruction IL_0009 = worker.Create(OpCodes.Stloc_1);


            Instruction IL_000a = worker.Create(OpCodes.Ldloc_0);
            Instruction IL_000b = worker.Create(OpCodes.Ldloca_S, V_1);
            Instruction IL_000d = worker.Create(OpCodes.Call, MethodTypes.MonitorEnter);

            Instruction IL_0012 = worker.Create(OpCodes.Nop);
            Instruction IL_0013 = worker.Create(OpCodes.Nop);
            Instruction IL_0014 = worker.Create(OpCodes.Ldarg_0);
            Instruction IL_0015 = worker.Create(OpCodes.Ldfld, privateField);
            Instruction IL_001a = worker.Create(OpCodes.Stloc_2);

            Instruction IL_0028 = worker.Create(OpCodes.Ldloc_2);

            Instruction IL_001b = worker.Create(OpCodes.Leave_S, IL_0028);


            Instruction IL_001d = worker.Create(OpCodes.Ldloc_1);


            Instruction IL_0027 = worker.Create(OpCodes.Endfinally);

            Instruction IL_001e = worker.Create(OpCodes.Brfalse_S, IL_0027);

            Instruction IL_0020 = worker.Create(OpCodes.Ldloc_0);
            Instruction IL_0021 = worker.Create(OpCodes.Call, MethodTypes.MonitorExit);
            Instruction IL_0026 = worker.Create(OpCodes.Nop);

            Instruction IL_0029 = worker.Create(OpCodes.Ret);


            //注入
            getMethod.Body.Instructions.Clear();
            getMethod.Body.Instructions.Add(IL_0000);
            getMethod.Body.Instructions.Add(IL_0001);
            getMethod.Body.Instructions.Add(IL_0002);
            getMethod.Body.Instructions.Add(IL_0007);
            getMethod.Body.Instructions.Add(IL_0008);
            getMethod.Body.Instructions.Add(IL_0009);
            getMethod.Body.Instructions.Add(IL_000a);
            getMethod.Body.Instructions.Add(IL_000b);
            getMethod.Body.Instructions.Add(IL_000d);
            getMethod.Body.Instructions.Add(IL_0012);
            getMethod.Body.Instructions.Add(IL_0013);
            getMethod.Body.Instructions.Add(IL_0014);
            getMethod.Body.Instructions.Add(IL_0015);
            getMethod.Body.Instructions.Add(IL_001a);
            getMethod.Body.Instructions.Add(IL_001b);
            getMethod.Body.Instructions.Add(IL_001d);
            getMethod.Body.Instructions.Add(IL_001e);
            getMethod.Body.Instructions.Add(IL_0020);
            getMethod.Body.Instructions.Add(IL_0021);
            getMethod.Body.Instructions.Add(IL_0026);
            getMethod.Body.Instructions.Add(IL_0027);
            getMethod.Body.Instructions.Add(IL_0028);
            getMethod.Body.Instructions.Add(IL_0029);


            //try-catch注入
            ExceptionHandler exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Finally);
            exceptionHandler.TryStart = IL_000a;
            exceptionHandler.TryEnd = IL_001d;
            exceptionHandler.HandlerStart = IL_001d;
            exceptionHandler.HandlerEnd = IL_0028;

            getMethod.Body.ExceptionHandlers.Clear();
            getMethod.Body.ExceptionHandlers.Add(exceptionHandler);
        }
    }
}
