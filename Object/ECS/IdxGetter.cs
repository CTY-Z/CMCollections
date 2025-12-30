using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CMFramework.Core.ECS
{
    public struct ComponentID { }
    public struct SingletonID { }
    public static class IdxGetter<C>
    {
        private static int curIdx = 0;
        private static readonly ConcurrentDictionary<Type, int> dic_type_ID = new();

        public static int Get<T>()
        {
            return dic_type_ID.GetOrAdd(typeof(T), _ => Interlocked.Increment(ref curIdx) - 1);
        }

        public static int Get(Type type)
        {
            return dic_type_ID.GetOrAdd(type, _ => Interlocked.Increment(ref curIdx) - 1);
        }
    }

    public class IDGenerator
    {
        private static int curID = 0;

        public static int Get()
        {
            int id = curID++;
            return id;
        }
    }
}

