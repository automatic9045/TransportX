using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Traffic
{
    public class TrafficRegistry : ITrafficRegistry, IDisposable
    {
        private static readonly IReadOnlyList<ITrafficEntity> EmptyList = [];


        private readonly List<ITrafficEntity> EntitiesKey = [];
        private readonly Dictionary<ChunkIndex, List<ITrafficEntity>> Buckets = [];
        private readonly Dictionary<ITrafficEntity, ChunkIndex> EntityChunks = [];

        public IReadOnlyList<ITrafficEntity> Entities => EntitiesKey;

        public TrafficRegistry()
        {
        }

        public void Dispose()
        {
            foreach (ITrafficEntity entity in EntitiesKey)
            {
                entity.Moved -= OnMoved;
            }
        }

        public void Register(ITrafficEntity entity)
        {
            if (EntityChunks.ContainsKey(entity)) return;

            entity.Moved += OnMoved;

            EntitiesKey.Add(entity);
            EntityChunks.Add(entity, entity.WorldPose.Chunk);
            GetOrCreateBucket(entity.WorldPose.Chunk).Add(entity);
        }

        public void Unregister(ITrafficEntity entity)
        {
            if (EntityChunks.Remove(entity, out ChunkIndex chunk))
            {
                entity.Moved -= OnMoved;

                EntitiesKey.Remove(entity);
                if (Buckets.TryGetValue(chunk, out List<ITrafficEntity>? bucket))
                {
                    bucket.Remove(entity);
                }
            }
        }

        private void OnMoved(IWorldObject sender, ChunkIndex chunkOffset)
        {
            if (sender is not ITrafficEntity entity) throw new InvalidOperationException();
            if (chunkOffset.IsZero) return;

            ChunkIndex previousChunk = EntityChunks[entity];
            if (Buckets.TryGetValue(previousChunk, out List<ITrafficEntity>? oldBucket))
            {
                oldBucket.Remove(entity);
            }

            GetOrCreateBucket(entity.WorldPose.Chunk).Add(entity);
            EntityChunks[entity] = entity.WorldPose.Chunk;
        }

        public IReadOnlyList<ITrafficEntity> GetEntitiesInChunk(ChunkIndex chunk)
        {
            return Buckets.TryGetValue(chunk, out List<ITrafficEntity>? list) ? list : EmptyList;
        }

        private List<ITrafficEntity> GetOrCreateBucket(ChunkIndex chunk)
        {
            if (!Buckets.TryGetValue(chunk, out List<ITrafficEntity>? bucket))
            {
                bucket = new List<ITrafficEntity>(8);
                Buckets.Add(chunk, bucket);
            }

            return bucket;
        }
    }
}
