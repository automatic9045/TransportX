using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

namespace Bus.Common.Scenery
{
    public class PlateCollection : IEnumerable<Plate>
    {
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, Plate>> Items = new();

        public Plate this[int x, int z]
        {
            get => TryGetValue(x, z, out Plate? result) ? result! : throw new KeyNotFoundException();
            set => Add(value, true);
        }

        public PlateCollection()
        {
        }

        public void SetCameraPosition(LocatableObject camera)
        {
            foreach (Plate plate in this)
            {
                PlateOffset fromCamera = new PlateOffset(plate.X - camera.PlateX, plate.Z - camera.PlateZ);
                plate.SetFromCamera(fromCamera);
            }
        }

        public bool TryGetValue(int x, int z, out Plate? result)
        {
            ConcurrentDictionary<int, Plate> xDictionary = FilterByX(x);
            return xDictionary.TryGetValue(z, out result);
        }

        public void Add(Plate item, bool allowOverwrite)
        {
            ConcurrentDictionary<int, Plate> xDictionary = FilterByX(item.X);
            if (allowOverwrite)
            {
                xDictionary.AddOrUpdate(item.Z, item, (_, _) => item);
            }
            else
            {
                if (!xDictionary.TryAdd(item.Z, item)) throw new ArgumentException("項目は既に存在します。", nameof(item));
            }
        }

        public void Add(Plate item) => Add(item, false);

        public Plate GetOrAdd(int x, int z, Func<int, int, Plate>? itemFactory = null)
        {
            if (TryGetValue(x, z, out Plate? result))
            {
                return result!;
            }
            else
            {
                itemFactory ??= (x, z) => new Plate(x, z);
                Plate item = itemFactory(x, z);
                Add(item);
                return item;
            }
        }

        private ConcurrentDictionary<int, Plate> FilterByX(int x)
        {
            ConcurrentDictionary<int, Plate> result = Items.GetOrAdd(x, new ConcurrentDictionary<int, Plate>());
            return result;
        }

        public IEnumerator<Plate> GetEnumerator() => Items.Values.Select(x => x.Values).SelectMany(x => x).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
