using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Coimbra
{
    /// <summary>
    /// Use this to hold a collection of <see cref="PoolData"/>.
    /// </summary>
    [System.Serializable]
    public sealed class PoolDataCollection : ScriptableObject, IList, IList<PoolData>
    {
        [SerializeField] private List<PoolData> _datas = new List<PoolData>();

        public PoolData this[int index]
        {
            get => _datas[index];
            set => _datas[index] = value;
        }

        public int Count => _datas.Count;
        public int Length => _datas.Count;

        public int Capacity
        {
            get => _datas.Capacity;
            set => _datas.Capacity = value;
        }

        bool ICollection.IsSynchronized => ((ICollection)_datas).IsSynchronized;
        bool IList.IsFixedSize => ((IList)_datas).IsFixedSize;
        bool IList.IsReadOnly => ((IList)_datas).IsReadOnly;
        bool ICollection<PoolData>.IsReadOnly => ((ICollection<PoolData>)_datas).IsReadOnly;
        object ICollection.SyncRoot => ((ICollection)_datas).SyncRoot;

        object IList.this[int index]
        {
            get => ((IList)_datas)[index];
            set => ((IList)_datas)[index] = value;
        }

        /// <summary>
        /// Create a new asset with the given data.
        /// </summary>
        public static PoolDataCollection CreateInstance(IEnumerable<PoolData> datas)
        {
            var collection = CreateInstance<PoolDataCollection>();
            collection.AddRange(datas);

            return collection;
        }

        public void Add(PoolData item)
        {
            _datas.Add(item);
        }

        public void AddRange(IEnumerable<PoolData> collection)
        {
            _datas.AddRange(collection);
        }

        public void Clear()
        {
            _datas.Clear();
        }

        public void CopyTo(PoolData[] array)
        {
            _datas.CopyTo(array);
        }

        public void CopyTo(PoolData[] array, int arrayIndex)
        {
            _datas.CopyTo(array, arrayIndex);
        }

        public void CopyTo(int index, PoolData[] array, int arrayIndex, int count)
        {
            _datas.CopyTo(index, array, arrayIndex, count);
        }

        public void ForEach(System.Action<PoolData> action)
        {
            _datas.ForEach(action);
        }

        public void Insert(int index, PoolData item)
        {
            _datas.Insert(index, item);
        }

        public void InsertRange(int index, IEnumerable<PoolData> collection)
        {
            _datas.InsertRange(index, collection);
        }

        public void RemoveAt(int index)
        {
            _datas.RemoveAt(index);
        }

        public void RemoveRange(int index, int count)
        {
            _datas.RemoveRange(index, count);
        }

        public void Reverse(int index, int count)
        {
            _datas.Reverse(index, count);
        }

        public void Reverse()
        {
            _datas.Reverse();
        }

        public void Sort()
        {
            _datas.Sort();
        }

        public void Sort(IComparer<PoolData> comparer)
        {
            _datas.Sort(comparer);
        }

        public void Sort(int index, int count, IComparer<PoolData> comparer)
        {
            _datas.Sort(index, count, comparer);
        }

        public void Sort(System.Comparison<PoolData> comparison)
        {
            _datas.Sort(comparison);
        }

        public void TrimExcess()
        {
            _datas.TrimExcess();
        }

        public bool Contains(PoolData item)
        {
            return _datas.Contains(item);
        }

        public bool Exists(System.Predicate<PoolData> match)
        {
            return _datas.Exists(match);
        }

        public bool Remove(PoolData item)
        {
            return _datas.Remove(item);
        }

        public bool TrueForAll(System.Predicate<PoolData> match)
        {
            return _datas.TrueForAll(match);
        }

        public int BinarySearch(PoolData item)
        {
            return _datas.BinarySearch(item);
        }

        public int BinarySearch(PoolData item, IComparer<PoolData> comparer)
        {
            return _datas.BinarySearch(item, comparer);
        }

        public int BinarySearch(int index, int count, PoolData item, IComparer<PoolData> comparer)
        {
            return _datas.BinarySearch(index, count, item, comparer);
        }

        public int FindIndex(System.Predicate<PoolData> match)
        {
            return _datas.FindIndex(match);
        }

        public int FindIndex(int startIndex, System.Predicate<PoolData> match)
        {
            return _datas.FindIndex(startIndex, match);
        }

        public int FindIndex(int startIndex, int count, System.Predicate<PoolData> match)
        {
            return _datas.FindIndex(startIndex, count, match);
        }

        public int FindLastIndex(System.Predicate<PoolData> match)
        {
            return _datas.FindLastIndex(match);
        }

        public int FindLastIndex(int startIndex, System.Predicate<PoolData> match)
        {
            return _datas.FindLastIndex(startIndex, match);
        }

        public int FindLastIndex(int startIndex, int count, System.Predicate<PoolData> match)
        {
            return _datas.FindLastIndex(startIndex, count, match);
        }

        public int IndexOf(PoolData item)
        {
            return _datas.IndexOf(item);
        }

        public int IndexOf(PoolData item, int index)
        {
            return _datas.IndexOf(item, index);
        }

        public int IndexOf(PoolData item, int index, int count)
        {
            return _datas.IndexOf(item, index, count);
        }

        public int LastIndexOf(PoolData item)
        {
            return _datas.LastIndexOf(item);
        }

        public int LastIndexOf(PoolData item, int index)
        {
            return _datas.LastIndexOf(item, index);
        }

        public int LastIndexOf(PoolData item, int index, int count)
        {
            return _datas.LastIndexOf(item, index, count);
        }

        public int RemoveAll(System.Predicate<PoolData> match)
        {
            return _datas.RemoveAll(match);
        }

        public PoolData Find(System.Predicate<PoolData> match)
        {
            return _datas.Find(match);
        }

        public PoolData FindLast(System.Predicate<PoolData> match)
        {
            return _datas.FindLast(match);
        }

        public PoolData[] ToArray()
        {
            return _datas.ToArray();
        }

        public List<PoolData> FindAll(System.Predicate<PoolData> match)
        {
            return _datas.FindAll(match);
        }

        public List<PoolData> GetRange(int index, int count)
        {
            return _datas.GetRange(index, count);
        }

        public List<TOutput> ConvertAll<TOutput>(System.Converter<PoolData, TOutput> converter)
        {
            return _datas.ConvertAll(converter);
        }

        public ReadOnlyCollection<PoolData> AsReadOnly()
        {
            return _datas.AsReadOnly();
        }

        public List<PoolData>.Enumerator GetEnumerator()
        {
            return _datas.GetEnumerator();
        }

        void ICollection.CopyTo(System.Array array, int index)
        {
            ((ICollection)_datas).CopyTo(array, index);
        }

        void IList.Insert(int index, object value)
        {
            ((IList)_datas).Insert(index, value);
        }

        void IList.Remove(object value)
        {
            ((IList)_datas).Remove(value);
        }

        bool IList.Contains(object value)
        {
            return ((IList)_datas).Contains(value);
        }

        int IList.Add(object value)
        {
            return ((IList)_datas).Add(value);
        }

        int IList.IndexOf(object value)
        {
            return ((IList)_datas).IndexOf(value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<PoolData> IEnumerable<PoolData>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
