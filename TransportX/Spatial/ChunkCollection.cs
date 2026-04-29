using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

namespace TransportX.Spatial
{
    public class ChunkCollection : IEnumerable<Chunk>, IDisposable
    {
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, Chunk>> Items = new();

        public Chunk this[int x, int z]
        {
            get => TryGetValue(x, z, out Chunk? result) ? result! : throw new KeyNotFoundException();
            set => Add(value, true);
        }

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

        public void SetCameraPosition(ILocatable camera, int computeChunkCount)
        {
            foreach (Chunk chunk in this)
            {
                ChunkOffset fromCamera = new(chunk.X - camera.WorldPose.ChunkX, chunk.Z - camera.WorldPose.ChunkZ);
                chunk.SetFromCamera(fromCamera, computeChunkCount);
            }
        }

        public bool TryGetValue(int x, int z, out Chunk? result)
        {
            ConcurrentDictionary<int, Chunk> xDictionary = FilterByX(x);
            return xDictionary.TryGetValue(z, out result);
        }

        public void Add(Chunk item, bool allowOverwrite)
        {
            ConcurrentDictionary<int, Chunk> xDictionary = FilterByX(item.X);
            if (allowOverwrite)
            {
                xDictionary.AddOrUpdate(item.Z, item, (_, _) => item);
            }
            else
            {
                if (!xDictionary.TryAdd(item.Z, item)) throw new ArgumentException("項目は既に存在します。", nameof(item));
            }
        }

        public void Add(Chunk item) => Add(item, false);

        public Chunk GetOrAdd(int x, int z, Func<int, int, Chunk>? itemFactory = null)
        {
            if (TryGetValue(x, z, out Chunk? result))
            {
                return result!;
            }
            else
            {
                itemFactory ??= (x, z) => new Chunk(x, z);
                Chunk item = itemFactory(x, z);
                Add(item);
                return item;
            }
        }

        public Chunk GetOrAddFor(ILocatable locatable, Func<int, int, Chunk>? itemFactory = null)
        {
            return GetOrAdd(locatable.WorldPose.ChunkX, locatable.WorldPose.ChunkZ, itemFactory);
        }

        private ConcurrentDictionary<int, Chunk> FilterByX(int x)
        {
            ConcurrentDictionary<int, Chunk> result = Items.GetOrAdd(x, new ConcurrentDictionary<int, Chunk>());
            return result;
        }

        public IEnumerator<Chunk> GetEnumerator() => Items.Values.Select(x => x.Values).SelectMany(x => x).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
