using BepuUtilities.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransportX.Extensions.Utilities
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
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

                int actualIndex = Head - 1 - index;

                if (actualIndex < 0) actualIndex += Buffer.Length;
                if (actualIndex < 0) actualIndex = (actualIndex % Buffer.Length + Buffer.Length) % Buffer.Length;

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

            Buffer[Head] = item;
            Head = (Head + 1) % Buffer.Length;
            Count++;
            Version++;
        }

        public void RemoveOldest()
        {
            if (Count == 0) throw new InvalidOperationException("バッファは空です。");

            int oldestIndex = Head - Count;
            if (oldestIndex < 0) oldestIndex += Buffer.Length;

            Buffer[oldestIndex] = default;

            Count--;
            Version++;
        }

        private void Expand()
        {
            int newCapacity = Buffer.Length * 2;
            T?[] newBuffer = new T?[newCapacity];

            for (int i = 0; i < Count; i++)
            {
                int oldIndex = (Head + i) % Buffer.Length;
                newBuffer[i] = Buffer[oldIndex]!;
            }

            Buffer = newBuffer;
            Head = Count;
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
