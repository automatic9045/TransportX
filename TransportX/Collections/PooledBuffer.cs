using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransportX.Collections
{
    public class PooledBuffer<T> : IList<T>, IReadOnlyList<T>, IDisposable
    {
        private T[] Buffer;
        private int CountKey = 0;
        private readonly object Lock = new();

        public T this[int index]
        {
            get => 0 <= index && index < Count ? Buffer[index] : throw new ArgumentOutOfRangeException(nameof(index));
            set
            {
                if (0 <= index && index < Count) Buffer[index] = value;
                else throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public int Count => CountKey;
        public int Capacity => Buffer.Length;
        public ReadOnlySpan<T> Span => Buffer.AsSpan(0, Count);

        bool ICollection<T>.IsReadOnly => false;

        public PooledBuffer(int initialCapacity = 256)
        {
            Buffer = ArrayPool<T>.Shared.Rent(initialCapacity);
        }

        public void Dispose()
        {
            if (Buffer is not null)
            {
                ArrayPool<T>.Shared.Return(Buffer, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                Buffer = null!;
            }
        }

        public void Add(T item)
        {
            lock (Lock)
            {
                if (Buffer.Length <= CountKey)
                {
                    T[] newBuffer = ArrayPool<T>.Shared.Rent(Buffer.Length * 2);
                    Array.Copy(Buffer, newBuffer, Buffer.Length);
                    ArrayPool<T>.Shared.Return(Buffer, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                    Buffer = newBuffer;
                }

                Buffer[CountKey] = item;
                CountKey++;
            }
        }

        public void Clear()
        {
            CountKey = 0;
        }

        public int IndexOf(T item) => Span.IndexOf(item);
        public bool Contains(T item) => Span.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => Span.CopyTo(array.AsSpan(arrayIndex));
        public ReadOnlySpan<T>.Enumerator GetEnumerator() => Span.GetEnumerator();

        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();
        void IList<T>.RemoveAt(int index) => throw new NotSupportedException();
        public bool Remove(T item) => throw new NotSupportedException();
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return Buffer[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
    }
}
