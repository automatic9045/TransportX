using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Collections;

namespace TransportX.Rendering
{
    public class RenderQueue : IRenderQueue
    {
        private readonly ConcurrentDictionary<RenderPass, ConcurrentDictionary<IModel, PooledBuffer<InstanceData>>> Instances = [];

        public RenderQueue()
        {
        }

        public void Clear()
        {
            foreach (ConcurrentDictionary<IModel, PooledBuffer<InstanceData>> perPass in Instances.Values)
            {
                foreach (PooledBuffer<InstanceData> perModel in perPass.Values)
                {
                    perModel.Clear();
                }
            }
        }

        public void Submit(RenderPass pass, IModel model, in InstanceData instance)
        {
            ConcurrentDictionary<IModel, PooledBuffer<InstanceData>> perPass = Instances.GetOrAdd(pass, pass => []);

            if (!perPass.TryGetValue(model, out PooledBuffer<InstanceData>? perModel))
            {
                PooledBuffer<InstanceData> newBuffer = [];
                if (perPass.TryAdd(model, newBuffer))
                {
                    perModel = newBuffer;
                }
                else
                {
                    newBuffer.Dispose();
                    perModel = perPass[model];
                }
            }

            perModel.Add(instance);
        }

        public unsafe void Render(RenderPass pass, in DrawContext context)
        {
            if (!Instances.TryGetValue(pass, out ConcurrentDictionary<IModel, PooledBuffer<InstanceData>>? perPass)) return;

            foreach ((IModel model, PooledBuffer<InstanceData> perModel) in perPass)
            {
                if (perModel.Count == 0) continue;

                ReadOnlySpan<InstanceData> fullSpan = perModel.Span;
                int maxInstanceCount = (int)context.InstanceBuffer.Description.ByteWidth / InstanceData.Size;

                for (int offset = 0; offset < fullSpan.Length; offset += maxInstanceCount)
                {
                    int count = int.Min(maxInstanceCount, fullSpan.Length - offset);
                    ReadOnlySpan<InstanceData> slice = fullSpan.Slice(offset, count);

                    MappedSubresource mapped = context.DeviceContext.Map(context.InstanceBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
                    Span<InstanceData> destination = new(mapped.DataPointer.ToPointer(), count);
                    slice.CopyTo(destination);
                    context.DeviceContext.Unmap(context.InstanceBuffer, 0);

                    DrawContext instancedContext = context with
                    {
                        InstanceCount = count,
                    };
                    model.Draw(instancedContext);
                }
            }
        }
    }
}
