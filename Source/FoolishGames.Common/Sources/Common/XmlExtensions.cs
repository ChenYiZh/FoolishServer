using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace FoolishGames.Common
{
    public static class XmlExtensions
    {
        public static string GetValue(this XmlNode node, string def)
        {
            if (node == null) { return def; }
            return string.IsNullOrEmpty(node.InnerText) ? def : node.InnerText;
        }

        public static bool GetValue(this XmlNode node, bool def)
        {
            if (node == null) { return def; }
            bool result;
            return bool.TryParse(node.InnerText, out result) ? result : def;
        }

        public static short GetValue(this XmlNode node, short def)
        {
            if (node == null) { return def; }
            short result;
            return short.TryParse(node.InnerText, out result) ? result : def;
        }

        public static int GetValue(this XmlNode node, int def)
        {
            if (node == null) { return def; }
            int result;
            return int.TryParse(node.InnerText, out result) ? result : def;
        }

        public static long GetValue(this XmlNode node, long def)
        {
            if (node == null) { return def; }
            long result;
            return long.TryParse(node.InnerText, out result) ? result : def;
        }

        public static ushort GetValue(this XmlNode node, ushort def)
        {
            if (node == null) { return def; }
            ushort result;
            return ushort.TryParse(node.InnerText, out result) ? result : def;
        }

        public static uint GetValue(this XmlNode node, uint def)
        {
            if (node == null) { return def; }
            uint result;
            return uint.TryParse(node.InnerText, out result) ? result : def;
        }

        public static ulong GetValue(this XmlNode node, ulong def)
        {
            if (node == null) { return def; }
            ulong result;
            return ulong.TryParse(node.InnerText, out result) ? result : def;
        }

        public static string GetValue(this XmlNode node, string attribute, string def)
        {
            if (node == null) { return def; }
            return node.Attributes.GetNamedItem(attribute).GetValue(def);
        }

        public static bool GetValue(this XmlNode node, string attribute, bool def)
        {
            if (node == null) { return def; }
            return node.Attributes.GetNamedItem(attribute).GetValue(def);
        }

        public static short GetValue(this XmlNode node, string attribute, short def)
        {
            if (node == null) { return def; }
            return node.Attributes.GetNamedItem(attribute).GetValue(def);
        }

        public static int GetValue(this XmlNode node, string attribute, int def)
        {
            if (node == null) { return def; }
            return node.Attributes.GetNamedItem(attribute).GetValue(def);
        }

        public static long GetValue(this XmlNode node, string attribute, long def)
        {
            if (node == null) { return def; }
            return node.Attributes.GetNamedItem(attribute).GetValue(def);
        }

        public static ushort GetValue(this XmlNode node, string attribute, ushort def)
        {
            if (node == null) { return def; }
            return node.Attributes.GetNamedItem(attribute).GetValue(def);
        }

        public static uint GetValue(this XmlNode node, string attribute, uint def)
        {
            if (node == null) { return def; }
            return node.Attributes.GetNamedItem(attribute).GetValue(def);
        }

        public static ulong GetValue(this XmlNode node, string attribute, ulong def)
        {
            if (node == null) { return def; }
            return node.Attributes.GetNamedItem(attribute).GetValue(def);
        }

        public static string GetValue(this XmlAttribute attribute, string def)
        {
            if (attribute == null) { return def; }
            return string.IsNullOrEmpty(attribute.Value) ? def : attribute.Value;
        }

        public static bool GetValue(this XmlAttribute attribute, bool def)
        {
            if (attribute == null) { return def; }
            bool result;
            return bool.TryParse(attribute.Value, out result) ? result : def;
        }

        public static short GetValue(this XmlAttribute attribute, short def)
        {
            if (attribute == null) { return def; }
            short result;
            return short.TryParse(attribute.Value, out result) ? result : def;
        }

        public static int GetValue(this XmlAttribute attribute, int def)
        {
            if (attribute == null) { return def; }
            int result;
            return int.TryParse(attribute.Value, out result) ? result : def;
        }

        public static long GetValue(this XmlAttribute attribute, long def)
        {
            if (attribute == null) { return def; }
            long result;
            return long.TryParse(attribute.Value, out result) ? result : def;
        }

        public static ushort GetValue(this XmlAttribute attribute, ushort def)
        {
            if (attribute == null) { return def; }
            ushort result;
            return ushort.TryParse(attribute.Value, out result) ? result : def;
        }

        public static uint GetValue(this XmlAttribute attribute, uint def)
        {
            if (attribute == null) { return def; }
            uint result;
            return uint.TryParse(attribute.Value, out result) ? result : def;
        }

        public static ulong GetValue(this XmlAttribute attribute, ulong def)
        {
            if (attribute == null) { return def; }
            ulong result;
            return ulong.TryParse(attribute.Value, out result) ? result : def;
        }
    }
}
