using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utils
{
    public class ShuffleBag<T> : ICollection<T>, IList<T>
    {
        private List<T> data = new List<T>();
        private int cursor = 0;
        private T last;

        public T Next()
        {
            if (cursor < 1)
            {
                cursor = data.Count - 1;
                if (data.Count < 1)
                    return default(T);
                return data[0];
            }

            int grab = Mathf.FloorToInt(UnityEngine.Random.value * (cursor + 1));
            T temp = data[grab];
            data[grab] = this.data[this.cursor];
            data[cursor] = temp;
            cursor--;
            return temp;
        }

        public T[] Next(int n)
        {
            T[] ts = new T[n];
            for (int i = 0; i < n; i++)
            {
                ts[i] = Next();
            }
            return ts;
        }

        public T[] NextNoRepeat(int n)
        {
            T[] ts = new T[n];
            for (int i = 0, j = 0; i < n; i++, j++)
            {
                if (j > n * 5) throw new Exception("Couldn't generate array of unique values");

                T v = Next();
                if (ts.Contains(v))
                {
                    i--;
                    continue;
                }

                ts[i] = v;
            }
            return ts;
        }

        #region IList[T] implementation
        public int IndexOf(T item)
        {
            return data.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            data.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            data.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return data[index];
            }
            set
            {
                data[index] = value;
            }
        }
        #endregion

        #region IEnumerable[T] implementation
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return data.GetEnumerator();
        }
        #endregion

        #region ICollection[T] implementation
        public void Add(T item)
        {
            data.Add(item);
            cursor = data.Count - 1;
        }

        public void Add(T item, int weight)
        {
            for (int i =0; i < weight; i++)
                Add(item);
        }

        public int Count
        {
            get
            {
                return data.Count;
            }
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(T item)
        {
            return data.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (T item in data)
            {
                array.SetValue(item, arrayIndex);
                arrayIndex = arrayIndex + 1;
            }
        }

        public bool Remove(T item)
        {
            return data.Remove(item);
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }
        #endregion

    }
}
