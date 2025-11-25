using CMFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMFramework.Core
{
    public class CMNameEntry
    {
        private static CMNameEntry _instance;
        public static CMNameEntry instance => _instance ??= new CMNameEntry();

        private Dictionary<string, int> dic_str_hashID = new();
        private Dictionary<int, string> dic_hashID_str = new();

        public int GetHashFromString(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            if (dic_str_hashID.TryGetValue(name, out int ID)) return ID;

            int newHash = GetStrHashOrdinalIgnore(name);
            if (dic_hashID_str.TryGetValue(newHash, out string str) && str != name)
                DebugUtility.Error($"FName Hash collision detected between '{name}' and '{str}'. Hash: {newHash}");

            dic_hashID_str[newHash] = name;
            dic_str_hashID[name] = newHash;

            return newHash;
        }

        public string GetStringFromHash(int hash)
        {
            if (dic_hashID_str.TryGetValue(hash, out string outName))
            {
                return outName;
            }

            return CMName.None;
        }

        /// <summary>
        /// 获取到不分大小写的字符串哈希值
        /// </summary>
        /// <param name="name">字符串</param>
        /// <returns></returns>
        private int GetStrHashOrdinalIgnore(string name)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(name);
        }
    }
}
