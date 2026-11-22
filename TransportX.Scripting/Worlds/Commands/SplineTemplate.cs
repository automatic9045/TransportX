using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Network;
using TransportX.Rendering;
using TransportX.Rendering.Procedural;
using TransportX.Spatial;

using TransportX.Extensions.Mathematics;
using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Worlds.Commands
{
    public class SplineTemplate
    {
        private readonly ScriptWorld World;

        public LaneLayout OutletLayout { get; }

        private readonly List<SplineProp> PropsKey = [];
        public IReadOnlyList<SplineProp> Props => PropsKey;

        private readonly List<SplineExtrusion> ExtrusionsKey = [];
        public IReadOnlyList<SplineExtrusion> Extrusions => ExtrusionsKey;

        public IComponentCollection<ITemplateComponent<IReadOnlyList<SplineBase>>> Components { get; }
            = new ComponentCollection<ITemplateComponent<IReadOnlyList<SplineBase>>>();
        public IErrorCollector ErrorCollector => World.ErrorCollector;

        internal SplineTemplate(ScriptWorld world, string outletLayoutKey)
        {
            World = world;
            OutletLayout = World.Commander.Network.LaneLayouts[outletLayoutKey];
        }

        public SplineProp PutProp(string? key, IReadOnlyList<string> modelKeys, Pose pose, double from, double span, double interval)
        {
            TransformedModelTemplate[] models = [.. modelKeys.Select(key =>
            {
                ModelResourceSet model = World.Models.GetModel(key);
                return StaticTransformedModelTemplate.CreateStaticOrNonCollision(World.PhysicsHost, model, pose);
            })];

            SplineProp prop = new(models, (float)from, (float)span, (float)interval, int.MaxValue)
            {
                Key = key,
            };

            PropsKey.Add(prop);
            return prop;
        }

        public SplineProp PutProp(IReadOnlyList<string> modelKeys, Pose pose, double from, double span, double interval)
            => PutProp(null, modelKeys, pose, from, span, interval);

        public SplineProp PutProp(string? key, IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutProp(key, modelKeys, position.ToPose(), from, span, interval);
        }

        public SplineProp PutProp(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval)
            => PutProp(null, modelKeys, x, y, z, rotationX, rotationY, rotationZ, from, span, interval);

        public SplineProp PutProp(string? key, IReadOnlyList<string> modelKeys, double x, double y, double z, double from, double span, double interval)
            => PutProp(key, modelKeys, x, y, z, 0, 0, 0, from, span, interval);

        public SplineProp PutProp(IReadOnlyList<string> modelKeys, double x, double y, double z, double from, double span, double interval)
            => PutProp(null, modelKeys, x, y, z, from, span, interval);

        public SplineExtrusion PutExtrusion(string? key, string crossSectionKey,
            double x, double y, double baseInterval, double maxDeviation, double maxAngleDelta, double vIntervalScale, double vOffset, double from)
        {
            if (!World.Commander.CrossSections.All.GetValue(crossSectionKey, out CrossSection? crossSection))
            {
                crossSection = CrossSection.Empty;
            }

            AdaptiveCurveSampler sampler = new((float)baseInterval, (float)maxDeviation, MathHelper.ToRadians((float)maxAngleDelta), (float)from, float.MaxValue);
            SplineExtrusion extrusion = new(crossSection, sampler, new Vector2((float)x, (float)y))
            {
                VIntervalScale = (float)vIntervalScale,
                VOffset = (float)vOffset - sampler.FromS,
                Key = key,
            };

            ExtrusionsKey.Add(extrusion);
            return extrusion;
        }

        public SplineExtrusion PutExtrusion(string crossSectionKey,
            double x, double y, double baseInterval, double maxDeviation, double maxAngleDelta, double vIntervalScale, double vOffset, double from)
            => PutExtrusion(null, crossSectionKey, x, y, baseInterval, maxDeviation, maxAngleDelta, vIntervalScale, vOffset, from);

        public SplineExtrusion PutExtrusion(string? key, string crossSectionKey,
            double x, double y, double baseInterval, double maxDeviation, double maxAngleDelta, double from)
            => PutExtrusion(key, crossSectionKey, x, y, baseInterval, maxDeviation, maxAngleDelta, 1, 0, from);

        public SplineExtrusion PutExtrusion(string crossSectionKey,
            double x, double y, double baseInterval, double maxDeviation, double maxAngleDelta, double from)
            => PutExtrusion(null, crossSectionKey, x, y, baseInterval, maxDeviation, maxAngleDelta, from);

        public SplineExtrusion PutExtrusion(string? key, string crossSectionKey, double baseInterval, double maxDeviation, double maxAngleDelta, double from)
            => PutExtrusion(key, crossSectionKey, 0, 0, baseInterval, maxDeviation, maxAngleDelta, from);

        public SplineExtrusion PutExtrusion(string crossSectionKey, double baseInterval, double maxDeviation, double maxAngleDelta, double from)
            => PutExtrusion(null, crossSectionKey, baseInterval, maxDeviation, maxAngleDelta, from);

        internal SplineFactoryCommand Build(WorldPose worldPose, NetworkPort? sourcePort)
        {
            SplineFactory factory = new(worldPose, OutletLayout, sourcePort);
            factory.Props.AddRange(Props);
            factory.Extrusions.AddRange(Extrusions);

            SplineFactoryCommand factoryCommand = new(World, factory);
            foreach ((Type type, ITemplateComponent<IReadOnlyList<SplineBase>> component) in Components)
            {
                factoryCommand.Components.Add(type, component);
            }

            return factoryCommand;
        }
    }
}
