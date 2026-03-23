using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;

namespace TransportX.Physics
{
    public class PhysicsHost : IPhysicsHost, IDisposable
    {
        private readonly CollidableProperty<ColliderGroupHandle> Groups = new();
        private readonly CollidableProperty<ColliderMaterial> Materials = new();

        private BufferPool BufferPool;
        private readonly ThreadDispatcher ThreadDispatcherKey;

        private bool IsDisposed = false;

        public Simulation Simulation { get; }
        public IThreadDispatcher ThreadDispatcher => ThreadDispatcherKey;

        protected PhysicsHost()
        {
            BufferPool = new BufferPool();
            NarrowPhaseCallbacks narrowPhaseCallbacks = new(Groups, Materials);
            Simulation = Simulation.Create(BufferPool, narrowPhaseCallbacks, new PoseIntegratorCallbacks(), new SolveDescription(1, 8));
            ThreadDispatcherKey = new ThreadDispatcher(System.Environment.ProcessorCount);
        }

        internal static PhysicsHost Create()
        {
            return new PhysicsHost();
        }

        public void Dispose()
        {
            if (IsDisposed) throw new InvalidOperationException();
            IsDisposed = true;

            int bodyCount = Enumerable.Range(0, Simulation.Bodies.HandleToLocation.Length)
                .Select(i => new BodyHandle(i))
                .Count(Simulation.Bodies.BodyExists);
            if (0 < bodyCount)
            {
                throw new Exception($"正常に解放されていない動的物理モデルを {bodyCount} 個検出しました。これはメモリリークの原因となります。");
            }

            int staticCount = Enumerable.Range(0, Simulation.Statics.HandleToIndex.Length)
                .Select(i => new StaticHandle(i))
                .Count(Simulation.Statics.StaticExists);
            if (0 < staticCount)
            {
                throw new Exception($"正常に解放されていない静的物理モデルを {staticCount} 個検出しました。これはメモリリークの原因となります。");
            }

            FieldInfo idPoolField = typeof(ShapeBatch).GetField("idPool", BindingFlags.NonPublic | BindingFlags.Instance)!;
            for (int i = 0; i < Simulation.Shapes.RegisteredTypeSpan; i++)
            {
                ShapeBatch? batch = (ShapeBatch?)Simulation.Shapes[i];
                if (batch is null) continue;

                IdPool idPool = (IdPool)idPoolField.GetValue(batch)!;
                int allocatedCount = idPool.HighestPossiblyClaimedId + 1 - idPool.AvailableIdCount;
                if (0 < allocatedCount)
                {
                    throw new Exception($"正常に解放されていない形状データをバッチ {batch.GetType()} から {allocatedCount} 個検出しました。" +
                        $"これはメモリリークの原因となります。");
                }
            }

            Simulation.Dispose();
            ThreadDispatcherKey.Dispose();
            BufferPool.Clear();
        }

        public void SetGroup(StaticHandle handle, ColliderGroupHandle group)
        {
            Groups.Allocate(handle) = group;
        }

        public void SetGroup(BodyHandle handle, ColliderGroupHandle group)
        {
            Groups.Allocate(handle) = group;
        }

        public void SetMaterial(StaticHandle handle, ColliderMaterial material)
        {
            Materials.Allocate(handle) = material;
        }

        public void SetMaterial(BodyHandle handle, ColliderMaterial material)
        {
            Materials.Allocate(handle) = material;
        }
    }
}
