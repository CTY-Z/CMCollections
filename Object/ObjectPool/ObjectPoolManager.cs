using CMFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMFramework.Core
{
    public class ObjectPoolManager : ManagerBase
    {
        private Dictionary<Type, ObjectPoolBase> dic_type_pool;

        public override void Init()
        {
            dic_type_pool = new();
        }

        public ObjectPool<T>.Pooled GetObj<T>() where T : class
        {
            ObjectPool<T> pool = GetPool<T>();
            return pool!.Rent();
        }

        public void GetObj<T>(Action<T> func) where T : class
        {
            ObjectPool<T> pool = GetPool<T>();
            using (var pooled = pool!.Rent())
            {
                func(pooled.value);
            }
        }

        public ObjectPool<T> GetPool<T>() where T : class
        {
            ObjectPoolBase pool = null;
            if (dic_type_pool.TryGetValue(typeof(T), out pool))
            {
                return (ObjectPool<T>)pool;
            }
            else
            {
                ObjectPoolCtorData<T> data = new ObjectPoolCtorData<T>("", 50, () => { return default(T); });
                CreatePool<T>(data);
            }

            return null;
        }

        private ObjectPool<T> CreatePool<T>(ObjectPoolCtorData<T> data)
        {
            ObjectPool<T> pool = new ObjectPool<T>(data.name, data.initialCapacity, data.factory,
                data.allowGrow, data.OnRent, data.OnReturn, data.isPrepareItem);

            dic_type_pool[typeof(T)] = pool;

            return pool;
        }
    }
}
