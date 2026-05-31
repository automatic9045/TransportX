using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

namespace TransportX.Spatial
{
    public class ChunkCollection : IReadOnlyCollection<Chunk>, IDisposable
    {
        private readonly ConcurrentDictionary<ChunkIndex, Chunk> Items = new();

        public Chunk this[ChunkIndex index]
        {
            get => TryGetValue(index, out Chunk? result) ? result! : throw new KeyNotFoundException();
            set => Add(value, true);
        }

        public Chunk this[int x, int z]
        {
            get => this[new ChunkIndex(x, z)];
            set => this[new ChunkIndex(x, z)] = value;
        }

        public int Count => Items.Count;

        public ChunkCollection()
        {
        }

        public void Dispose()
        {
            foreach (Chunk chunk in this) chunk.Dispose();
        }

        public void RegisterComponents(ComponentEngine engine)
        {
            foreach (Chunk chunk in this)
            {
                chunk.RegisterComponents(engine);
            }
        }

        public void SetCameraPosition(WorldPose cameraWorldPose, int computeChunkCount)
        {
            foreach (Chunk chunk in this)
            {
                ChunkIndex fromCamera = chunk.Index - cameraWorldPose.Chunk;
                chunk.SetFromCamera(fromCamera, computeChunkCount);
            }
        }

        public bool TryGetValue(ChunkIndex index, [MaybeNullWhen(false)] out Chunk result)
        {
            return Items.TryGetValue(index, out result);
        }

        public void Add(Chunk item, bool allowOverwrite)
        {
            if (allowOverwrite)
            {
                Items.AddOrUpdate(item.Index, item, (_, _) => item);
            }
            else
            {
                if (!Items.TryAdd(item.Index, item)) throw new ArgumentException("項目は既に存在します。", nameof(item));
            }
        }

        public void Add(Chunk item) => Add(item, false);

        public Chunk GetOrAdd(ChunkIndex index, Func<ChunkIndex, Chunk>? itemFactory = null)
        {
            if (TryGetValue(index, out Chunk? result))
            {
                return result!;
            }
            else
            {
                itemFactory ??= index => new Chunk(index);
                Chunk item = itemFactory(index);
                Add(item);
                return item;
            }
        }

        public Chunk GetOrAddFor(IWorldObject worldObject, Func<ChunkIndex, Chunk>? itemFactory = null)
        {
            return GetOrAdd(worldObject.WorldPose.Chunk, itemFactory);
        }

        public IEnumerator<Chunk> GetEnumerator() => Items.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
