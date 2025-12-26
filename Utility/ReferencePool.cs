using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMFramework.Core
{
    public static class ReferencePool
    {
        private static Dictionary<Type, ObjectPoolBase> dic_type_pool = new();

        public static int Count { get { return dic_type_pool.Count; } }

        public static T GetRef<T>(bool isCreatePool = true) where T : IPoolItem, new()
        {
            ObjectPool<T> pool = GetPool<T>(isCreatePool);

            if (pool == null) return default(T);

            return pool!.Rent();
        }

        public static T GetRefByCreatePool<T>(ObjectPoolCtorData<T> data) where T : IPoolItem
        {
            ObjectPool<T> pool = GetPool<T>(data);
            return pool!.Rent();
        }

        public static ObjectPool<T> GetPool<T>(bool isCreatePool = true) where T : IPoolItem, new()
        {
            ObjectPoolBase pool = null;
            if (dic_type_pool.TryGetValue(typeof(T), out pool))
            {
                return (ObjectPool<T>)pool;
            }
            else
            {
                if (!isCreatePool) return null;

                ObjectPoolCtorData<T> data = new ObjectPoolCtorData<T>("", 32, () => { return new T(); }, false, null
                    , null, true);
                pool = CreatePool<T>(data);
                dic_type_pool[typeof(T)] = pool;
                return (ObjectPool<T>)pool;
            }
        }

        public static ObjectPool<T> GetPool<T>(ObjectPoolCtorData<T> data) where T : IPoolItem
        {
            ObjectPoolBase pool = null;
            if (dic_type_pool.TryGetValue(typeof(T), out pool))
            {
                return (ObjectPool<T>)pool;
            }
            else
            {
                pool = CreatePool<T>(data);
                dic_type_pool[typeof(T)] = pool;
                return (ObjectPool<T>)pool;
            }
        }

        public static void Return<T>(int idx) where T : IPoolItem
        {
            ObjectPoolBase pool = null;
            if (dic_type_pool.TryGetValue(typeof(T), out pool))
                ((ObjectPool<T>)pool).InternalReturn(idx);
        }

        private static ObjectPool<T> CreatePool<T>(ObjectPoolCtorData<T> data) where T : IPoolItem
        {
            return new ObjectPool<T>(data.name, data.initialCapacity, data.factory,
                data.allowGrow, data.OnRent, data.OnReturn, data.isPrepareItem);
        }


    }
}

