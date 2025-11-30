using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;
using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Worlds;

namespace Bus.Common.Vehicles
{
    public class VehicleBuilder
    {
        public required IDXHost DXHost { get; init; }
        public required IDXClient DXClient { get; init; }
        public required IPhysicsHost PhysicsHost { get; init; }
        public required ITimeManager TimeManager { get; init; }
        public required InputManager InputManager { get; init; }
        public required Camera Camera { get; init; }
        public required WorldBase World { get; init; }

        public VehicleBuilder()
        {
        }

        internal protected VehicleBase Build(string path, string? identifier)
        {
            Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            Type[] vehicleTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(VehicleBase)))
                .ToArray();

            if (vehicleTypes.Length == 0)
            {
                string fileName = Path.GetFileName(path);
                throw new ArgumentException($"{fileName} には車両が定義されていません。", nameof(path));
            }

            Type vehicleType;
            if (identifier is null)
            {
                if (vehicleTypes.Length == 1)
                {
                    vehicleType = vehicleTypes[0];
                }
                else
                {
                    string fileName = Path.GetFileName(path);
                    throw new ArgumentException($"{fileName} には 2 つ以上の車両が定義されています。", nameof(path));
                }
            }
            else
            {
                Type? type = null;
                foreach (Type t in vehicleTypes)
                {
                    VehicleIdentifierAttribute? identifierAttribute = t.GetCustomAttribute<VehicleIdentifierAttribute>();
                    if (identifierAttribute?.Identifier == identifier)
                    {
                        type = t;
                    }
                }

                if (type is null)
                {
                    string fileName = Path.GetFileName(path);
                    throw new ArgumentException($"{fileName} には車両 '{identifier}' が定義されていません。", nameof(identifier));
                }
                vehicleType = type;
            }

            ConstructorInfo constructor = vehicleType.GetConstructor([typeof(VehicleBuilder)])
                ?? throw new ArgumentException($"{vehicleType.Name} にはパラメータが {nameof(VehicleBuilder)} のコンストラクタが定義されていません。", nameof(path));

            VehicleBase vehicle = (VehicleBase)constructor.Invoke([this]);
            return vehicle;
        }
    }
}
