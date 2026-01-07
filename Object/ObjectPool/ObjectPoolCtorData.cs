using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CM.Core
{
    public struct ObjectPoolCtorData<T>
    {
        public string name;
        public int initialCapacity;
        public Func<T> factory;
        public bool allowGrow;
#nullable enable
        public Action<T>? OnRent;
        public Action<T>? OnReturn;
        public bool isPrepareItem;

        /// <summary>
        /// 构建一个对象池所需的数据
        /// </summary>
        /// <param name="name">对象池名字</param>
        /// <param name="initialCapacity">构造时的容量，建议按照最大并发量预先分配</param>
        /// <param name="factory">创建对象函数，只会在初始化或者扩容的时候调用</param>
        /// <param name="allowGrow">是否允许容量不足时自动扩容(会产生分配)</param>
        /// <param name="OnRent">租用回调</param>
        /// <param name="OnReturn">归还回调</param>
        /// <param name="isPrepareItem">是否在创建Pool时直接预创建所有对象</param>
        public ObjectPoolCtorData(string name, int initialCapacity, Func<T> factory, bool allowGrow = false,
            Action<T>? OnRent = null, Action<T>? OnReturn = null, bool isPrepareItem = false)
        {
            this.name = name;
            this.initialCapacity = initialCapacity;
            this.factory = factory;
            this.allowGrow = allowGrow;
            this.OnRent = OnRent;
            this.OnReturn = OnReturn;
            this.isPrepareItem = isPrepareItem;
        }
#nullable disable
    }
}
