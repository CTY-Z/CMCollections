using System;

namespace CMFramework.Core
{
    public class ObjectPool<T> : ObjectPoolBase where T : IPoolItem
    {
        private T[] arr_item;
        private int[] arr_next;
        private int freeHead;
        private int count;
        private readonly Func<T> factory;
        private readonly bool allowGrow;

        public override string PoolName { get { return poolName; } }

#nullable enable
        private readonly Action<T>? OnRent;
        private readonly Action<T>? OnReturn;

        /// <summary>
        /// 构造一个对象池
        /// </summary>
        /// <param name="initialCapacity">构造时的容量，建议按照最大并发量预先分配</param>
        /// <param name="factory">创建对象函数，只会在初始化或者扩容的时候调用</param>
        /// <param name="allowGrow">是否允许容量不足时自动扩容(会产生分配)</param>
        /// <param name="OnRent">租用回调</param>
        /// <param name="OnReturn">归还回调</param>
        /// <param name="isPrepareItem">是否在创建Pool时直接预创建所有对象</param>
        public ObjectPool(string name , int initialCapacity, Func<T> factory, bool allowGrow = false,
            Action<T>? OnRent = null, Action<T>? OnReturn = null, bool isPrepareItem = false)
        {
            if (initialCapacity < 0) 
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));

            this.poolName = name;
            this.arr_item = new T[initialCapacity];
            this.arr_next = new int[initialCapacity];
            this.freeHead = -1;
            this.count = 0;
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.allowGrow = allowGrow;
            this.OnRent = OnRent;
            this.OnReturn = OnReturn;

            if (isPrepareItem)
            {
                for (int i = 0; i < initialCapacity; i++)
                {
                    var item = factory();
                    this.arr_item[i] = item;
                    this.arr_next[i] = this.freeHead;
                    this.freeHead = i;
                    this.count++;
                }
            }
        }
#nullable disable

        /// <summary>
        /// (仅供编辑器模式)获取当前空闲数量
        /// </summary>
        public int Editor_freeCount => ComputeFreeCount();
        private int ComputeFreeCount()
        {
#if UNITY_EDITOR
            int c = 0;
            int idx = freeHead;
            while (idx != -1)
            {
                c++;
                idx = arr_next[idx];
            }
            return c;
#else
            return -1;
#endif
        }

        public T Rent()
        {
            int idx = PopFree();
            if (idx == -1)
            {
                if (!allowGrow)
                    throw new InvalidOperationException($"[ObjectPool.Rent()] - ObjectPool:{typeof(T).Name} exhausted and allowAlloc is false!");
                idx = ExpandAndTake();
            }

            var item = arr_item[idx];
            OnRent?.Invoke(item);
            item.idx = idx;
            return item;
        }

        internal void InternalReturn(int idx)
        {
            var item = arr_item[idx];
#if UNITY_EDITOR
            int t_idx = freeHead;
            while (t_idx != -1)
            {
                if (idx == t_idx)
                {
                    DebugUtility.Error("[ObjectPool.InternalReturn] - Double return detected in ObjectPool!");
                    break;
                }
                t_idx = arr_next[t_idx];
            }
#endif

            OnReturn?.Invoke(item);
            arr_next[idx] = freeHead;
            freeHead = idx;
        }

        private int PopFree()
        {
            int idx = freeHead;
            if (idx == -1) return -1;

            freeHead = arr_next[idx];
            arr_next[idx] = -1;
            return idx;
        }

        private int ExpandAndTake()
        {
            int oldCap = arr_item.Length;
            int newCap = oldCap * 2;

            Array.Resize(ref arr_item, newCap);
            Array.Resize(ref arr_next, newCap);

            for (int i = oldCap; i < newCap; i++)
            {
                var item = factory();
                arr_item[i] = item;
                arr_next[i] = freeHead;
                freeHead = i;
                count++;
            }

            return PopFree();
        }
    }
}
