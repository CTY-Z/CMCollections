using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CM.Core
{
    public static class PathUtil
    {
        /// <summary>
        /// 获取规范的路径
        /// </summary>
        public static string GetRegularPath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
