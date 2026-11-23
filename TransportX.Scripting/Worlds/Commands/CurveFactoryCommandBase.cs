using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Mathematics;
using TransportX.Rendering;
using TransportX.Rendering.Procedural;
using TransportX.Spatial;

using TransportX.Extensions.Mathematics;
using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Worlds.Commands
{
    public abstract class CurveFactoryCommandBase
    {
        protected readonly ScriptWorld World;

        public string Key { get; }
        public IPoseCurve Curve { get; }

        private readonly List<SplineProp> PropsKey;
        public IReadOnlyList<SplineProp> Props => PropsKey;

        private readonly List<SplineExtrusion> ExtrusionsKey = [];
        public IReadOnlyList<SplineExtrusion> Extrusions => ExtrusionsKey;

        private protected CurveFactoryCommandBase(ScriptWorld world, string key, IPoseCurve curve, IEnumerable<SplineProp> props, IEnumerable<SplineExtrusion> extrusions)
        {
            World = world;

            Key = key;
            Curve = curve;
            PropsKey = [.. props];
            ExtrusionsKey = [.. extrusions];
        }

        public SplineProp PutProp(IReadOnlyList<string> modelKeys, Pose pose, double from, double span, double interval, int count = int.MaxValue)
        {
            TransformedModelTemplate[] models = modelKeys.Select(key =>
            {
                ModelResourceSet model = World.Models.GetModel(key);
                return StaticTransformedModelTemplate.CreateStaticOrNonCollision(World.PhysicsHost, model, pose);
            }).ToArray();
            SplineProp prop = new(models, (float)from, (float)span, (float)interval, count);
            PropsKey.Add(prop);
            return prop;
        }

        public SplineProp PutProp(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval, int count = int.MaxValue)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutProp(modelKeys, position.ToPose(), from, span, interval, count);
        }

        public SplineProp PutProp(IReadOnlyList<string> modelKeys, double x, double y, double z, double from, double span, double interval, int count = int.MaxValue)
            => PutProp(modelKeys, x, y, z, 0, 0, 0, from, span, interval, count);

        public SplineExtrusion PutExtrusion(string? key, string crossSectionKey,
            double x, double y, double baseInterval, double maxDeviation, double maxAngleDelta, double vIntervalScale, double vOffset, double from, double to = double.MaxValue)
        {
            if (!World.Commander.CrossSections.All.GetValue(crossSectionKey, out CrossSection? crossSection))
            {
                crossSection = CrossSection.Empty;
            }

            AdaptiveCurveSampler sampler = new((float)baseInterval, (float)maxDeviation, MathHelper.ToRadians((float)maxAngleDelta), (float)from, (float)to);
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
            double x, double y, double baseInterval, double maxDeviation, double maxAngleDelta, double vIntervalScale, double vOffset, double from, double to = double.MaxValue)
            => PutExtrusion(null, crossSectionKey, x, y, baseInterval, maxDeviation, maxAngleDelta, vIntervalScale, vOffset, from, to);

        public SplineExtrusion PutExtrusion(string? key, string crossSectionKey,
            double x, double y, double baseInterval, double maxDeviation, double maxAngleDelta, double from, double to = double.MaxValue)
            => PutExtrusion(key, crossSectionKey, x, y, baseInterval, maxDeviation, maxAngleDelta, 1, 0, from, to);

        public SplineExtrusion PutExtrusion(string crossSectionKey,
            double x, double y, double baseInterval, double maxDeviation, double maxAngleDelta, double from, double to = double.MaxValue)
            => PutExtrusion(null, crossSectionKey, x, y, baseInterval, maxDeviation, maxAngleDelta, from, to);

        public SplineExtrusion PutExtrusion(string? key, string crossSectionKey,
            double baseInterval, double maxDeviation, double maxAngleDelta, double from, double to = double.MaxValue)
            => PutExtrusion(key, crossSectionKey, 0, 0, baseInterval, maxDeviation, maxAngleDelta, from, to);

        public SplineExtrusion PutExtrusion(string crossSectionKey,
            double baseInterval, double maxDeviation, double maxAngleDelta, double from, double to = double.MaxValue)
            => PutExtrusion(null, crossSectionKey, baseInterval, maxDeviation, maxAngleDelta, from, to);
    }
}
