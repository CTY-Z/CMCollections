using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

        //这是一个UnityEngine.Rendering底下SwapCollectionExtensions内部的一个方法，不知道为什么别的项目就找不到了，直接移植过来
        /// <summary>
        /// Tries to remove a range of elements from the list in the given range.
        /// </summary>
        /// <param name="list">The list to remove the range</param>
        /// <param name="from">From index</param>
        /// <param name="to">To index</param>
        /// <param name="error">The exception raised by the implementation</param>
        /// <typeparam name="TValue">The value type stored on the list</typeparam>
        /// <returns>True if succeed, false otherwise</returns>
        [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
        [MustUseReturnValue]
        public static bool TrySwap<TValue>([DisallowNull] this IList<TValue> list, int from, int to, [NotNullWhen(false)] out Exception error)
        {
            error = null;
            if (list == null)
            {
                error = new ArgumentNullException(nameof(list));
            }
            else
            {
                if (from < 0 || from >= list.Count)
                    error = new ArgumentOutOfRangeException(nameof(from));
                if (to < 0 || to >= list.Count)
                    error = new ArgumentOutOfRangeException(nameof(to));
            }

            if (error != null)
                return false;

            // https://tearth.dev/posts/performance-of-the-different-ways-to-swap-two-values/
            (list[to], list[from]) = (list[from], list[to]);
            return true;
        }
    }
}
    

