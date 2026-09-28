using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Collections
{
    public class RingBuffer<T> : IReadOnlyList<T>
    {
        private T?[] Buffer;
        private int Head;
        private int Version = 0;

        public int Count { get; private set; }
        public int Capacity => Buffer.Length;

        public T this[int index]
        {
            get
            {
                if (index < 0 || Count <= index) throw new ArgumentOutOfRangeException(nameof(index));

                int actualIndex = (Head + index) % Buffer.Length;
                return Buffer[actualIndex]!;
            }
        }

        public RingBuffer(int capacity = 8)
        {
            if (capacity < 1) capacity = 1;
            Buffer = new T?[capacity];
            Head = 0;
            Count = 0;
        }

        public void Add(T item)
        {
            if (Count == Buffer.Length)
            {
                Expand();
            }

            int tailIndex = (Head + Count) % Buffer.Length;
            Buffer[tailIndex] = item;
            Count++;
            Version++;
        }

        public bool TryPop(out T item)
        {
            if (Count == 0)
            {
                item = default!;
                return false;
            }

            item = Buffer[Head]!;
            Buffer[Head] = default;
            Head = (Head + 1) % Buffer.Length;
            Count--;
            Version++;
            return true;
        }

        public void RemoveOldest()
        {
            if (Count == 0) throw new InvalidOperationException("バッファは空です。");

            Buffer[Head] = default;
            Head = (Head + 1) % Buffer.Length;
            Count--;
            Version++;
        }

        private void Expand()
        {
            int newCapacity = Buffer.Length * 2;
            T?[] newBuffer = new T?[newCapacity];

            for (int i = 0; i < Count; i++)
            {
                newBuffer[i] = Buffer[(Head + i) % Buffer.Length];
            }

            Buffer = newBuffer;
            Head = 0;
        }

        public void Clear()
        {
            Head = 0;
            Count = 0;
            Version++;
            Array.Clear(Buffer, 0, Buffer.Length);
        }

        public IEnumerator<T> GetEnumerator()
        {
            int version = Version;
            for (int i = 0; i < Count; i++)
            {
                yield return version == Version ? this[i] : throw new InvalidOperationException("列挙中にコレクションが変更されました。");
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
