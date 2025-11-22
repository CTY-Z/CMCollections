using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMFramework.Core.Collections
{
    public static class ListExtension
    {
        public static bool AddUnique<T>(this List<T> list, T item)
        {
            if (list.Contains(item))
                return false;

            list.Add(item);
            return true;
        }

        public static void MoveTemp<T>(this List<T> target, List<T> other)
        {
            if (target == null || other == null || target == other)
                return;

            target.Clear();
            target.AddRange(other);
            other.Clear();
        }

        public static int RemoveSingle<T>(this List<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index >= 0)
            {
                list.RemoveAt(index);
            }
            return index;
        }

        public static bool IsValidIndex<T>(this List<T> list, int idx)
        {
            if (list == null) return false;
            return idx < 0 || idx >= list.Count;
        }
    }
}
    

