using System.Collections;
using System.Collections.Generic;

namespace CMFramework.Core.Collections
{
    public class SparseSet : IEnumerable<int>
    {
        private int m_pageSize;

        private List<int> list_density;
        private List<int[]> list_sparse;

        public List<int> List_Density { get; }

        public SparseSet(int pageSize)
        {
            list_density = new();
            list_sparse = new();

            m_pageSize = pageSize;
        }

        public void Add(int t)
        {
            list_density.Add(t);
            Assure(t);
            IndexRef(t) = list_density.Count - 1;
        }

        public void Remove(int t)
        {
            if (!Contain(t)) return;

            var idx = Index(t);
            var idxRef = IndexRef(t);
            if (idx == list_density.Count - 1)
            {
                idxRef = -1;
                list_density.RemoveAt(list_density.Count - 1);
            }
            else
            {
                int last = list_density[list_density.Count - 1];
                IndexRef(last) = idx;
                list_density.TrySwap(idx, list_density.Count - 1, out System.Exception error);
                idxRef = -1;
                list_density.RemoveAt(list_density.Count - 1);
            }
        }

        public bool Contain(int t)
        {
            if (t < 0)
                DebugUtility.Error("[SparseSet.Contain] param t less then 0");

            int p = Page(t);
            int o = Offset(t);

            return p < list_sparse.Count && list_sparse[p][o] >= 0;
        }

        public void Clear()
        {
            list_density.Clear();
            list_sparse.Clear();
        }

        private int Page(int t) { return t / m_pageSize; }
        private int Offset(int t) { return t % m_pageSize; }
        private int Index(int t) { return list_sparse[Page(t)][Offset(t)]; }
        private ref int IndexRef(int t) { return ref list_sparse[Page(t)][Offset(t)]; }

        private void Assure(int t)
        {
            int p = Page(t);
            if (p >= list_sparse.Count)
            {
                for (int i = list_sparse.Count; i <= p; i++)
                {
                    list_sparse.Add(new int[m_pageSize]);
                    for (int j = 0; j < m_pageSize; j++)
                        list_sparse[i][j] = -1;
                }
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < list_density.Count; i++)
                yield return list_density[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

