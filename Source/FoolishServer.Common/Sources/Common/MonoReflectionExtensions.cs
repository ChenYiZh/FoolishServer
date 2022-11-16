using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Common
{
    /// <summary>
    /// Mono反射的拓展
    /// </summary>
    internal static class MonoReflectionExtensions
    {
        /// <summary>
        /// 判断是否是子类
        /// </summary>
        public static bool IsChildOf(this TypeDefinition type, string classFullName)
        {
            if (type == null)
            {
                return false;
            }
            if (type.BaseType.Is(classFullName))
            {
                return true;
            }
            return type.BaseType.IsChildOf(classFullName);
        }

        /// <summary>
        /// 判断是否是子类
        /// </summary>
        public static bool Is(this TypeReference type, string classFullName)
        {
            if (type == null)
            {
                return false;
            }
            return type.FullName == classFullName;
        }

        /// <summary>
        /// 判断是否是子类
        /// </summary>
        public static bool IsChildOf(this TypeReference type, string classFullName)
        {
            if (type == null)
            {
                return false;
            }
            return type.Resolve().IsChildOf(classFullName);
        }

        /// <summary>
        /// 属性中是否包含Atrribute
        /// </summary>
        public static bool ContainsAttribute(this PropertyDefinition property, string attributeName)
        {
            if (property == null || property.CustomAttributes == null)
            {
                return false;
            }
            foreach (CustomAttribute attribute in property.CustomAttributes)
            {
                if (attribute.Constructor != null &&
                            attribute.Constructor.DeclaringType != null &&
                            attribute.Constructor.DeclaringType.Name == "EntityFieldAttribute")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
