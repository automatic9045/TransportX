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
    public class PlateCollection : IEnumerable<LocatedPlate>
    {
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, LocatedPlate>> Items = new();

        public LocatedPlate this[int x, int z]
        {
            get => TryGetValue(x, z, out LocatedPlate? result) ? result! : throw new KeyNotFoundException();
            set => Add(value, true);
        }

        public PlateCollection()
        {
        }

        public void ComputeTick(int cameraX, int cameraZ)
        {
            foreach (LocatedPlate locatedPlate in this)
            {
                PlateOffset fromCamera = new PlateOffset(locatedPlate.X - cameraX, locatedPlate.Z - cameraZ);
                locatedPlate.Plate.ComputeTick(fromCamera);
            }
        }

        public bool TryGetValue(int x, int z, out LocatedPlate? result)
        {
            ConcurrentDictionary<int, LocatedPlate> xDictionary = FilterByX(x);
            return xDictionary.TryGetValue(z, out result);
        }

        public void Add(LocatedPlate item, bool allowOverwrite)
        {
            ConcurrentDictionary<int, LocatedPlate> xDictionary = FilterByX(item.X);
            if (allowOverwrite)
            {
                xDictionary.AddOrUpdate(item.Z, item, (_, _) => item);
            }
            else
            {
                if (!xDictionary.TryAdd(item.Z, item)) throw new ArgumentException("項目は既に存在します。", nameof(item));
            }
        }

        public void Add(LocatedPlate item) => Add(item, false);
        public void Add(int x, int z, Plate item, bool allowOverwrite) => Add(new LocatedPlate(x, z, item), allowOverwrite);
        public void Add(int x, int z, Plate item) => Add(x, z, item, false);

        public Plate GetOrAdd(int x, int z, Func<int, int, Plate>? itemFactory = null)
        {
            if (TryGetValue(x, z, out LocatedPlate? result))
            {
                return result!.Plate;
            }
            else
            {
                itemFactory ??= (x, z) => new Plate();
                Plate item = itemFactory(x, z);
                Add(x, z, item);
                return item;
            }
        }

        private ConcurrentDictionary<int, LocatedPlate> FilterByX(int x)
        {
            ConcurrentDictionary<int, LocatedPlate> result = Items.GetOrAdd(x, new ConcurrentDictionary<int, LocatedPlate>());
            return result;
        }

        public IEnumerator<LocatedPlate> GetEnumerator() => Items.Values.Select(x => x.Values).SelectMany(x => x).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
