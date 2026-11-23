using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Diagnostics;
using TransportX.Mathematics;
using TransportX.Rendering;
using TransportX.Rendering.Procedural;
using TransportX.Spatial;

using TransportX.Extensions.Mathematics;
using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Worlds.Commands
{
    public abstract class CurveTemplateBase<T> where T : CurveTemplateBase<T>
    {
        protected readonly List<IPoseCurve> Curves = [];
        protected bool IsFinalized = false;

        public JunctionTemplate Parent { get; }

        public string Key { get; }

        public Pose StartPose { get; }
        public Pose CurrentPose => Curves.Count == 0 ? StartPose : Curves[^1].EndPose;

        public float Length { get; private set; } = 0;

        private readonly List<SplineProp> PropsKey = [];
        public IReadOnlyList<SplineProp> Props => PropsKey;

        private readonly List<SplineExtrusion> ExtrusionsKey = [];
        public IReadOnlyList<SplineExtrusion> Extrusions => ExtrusionsKey;

        private protected CurveTemplateBase(JunctionTemplate parent, string key, Pose startPose)
        {
            Parent = parent;

            Key = key;
            StartPose = startPose;
        }

        private bool CheckNotFinalized()
        {
            if (IsFinalized)
            {
                ScriptError error = new(ErrorLevel.Error, "これ以上セグメントを追加することはできません。");
                Parent.World.ErrorCollector.Report(error);
                return false;
            }

            return true;
        }

        private static Pose CreatePose(double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return position.ToPose();
        }

        public T StraightTo(Pose to, out double s)
        {
            if (!CheckNotFinalized())
            {
                s = Length;
                return (T)this;
            }

            LinearPoseCurve curve = new(CurrentPose, to);
            Curves.Add(curve);

            Length += curve.Length;
            s = Length;
            return (T)this;
        }

        public T StraightTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ, out double s)
        {
            Pose to = CreatePose(x, y, z, rotationX, rotationY, rotationZ);
            return StraightTo(to, out s);
        }

        public T StraightTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ)
            => StraightTo(x, y, z, rotationX, rotationY, rotationZ, out _);

        public T BezierTo(Pose to, out double s, double handleScale = 0.5)
        {
            if (!CheckNotFinalized())
            {
                s = Length;
                return (T)this;
            }

            BezierPoseCurve curve = new(CurrentPose, to, (float)handleScale);
            Curves.Add(curve);

            Length += curve.Length;
            s = Length;
            return (T)this;
        }

        public T BezierTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ, out double s, double handleScale = 0.5)
        {
            Pose to = CreatePose(x, y, z, rotationX, rotationY, rotationZ);
            return BezierTo(to, out s, handleScale);
        }

        public T BezierTo(double x, double y, double z, double rotationX, double rotationY, double rotationZ, double handleScale = 0.5)
            => BezierTo(x, y, z, rotationX, rotationY, rotationZ, out _, handleScale);

        public SplineProp PutProp(IReadOnlyList<string> modelKeys, Pose pose, double from, double span, double interval, int count = int.MaxValue)
        {
            TransformedModelTemplate[] models = modelKeys.Select(key =>
            {
                ModelResourceSet model = Parent.World.Models.GetModel(key);
                return StaticTransformedModelTemplate.CreateStaticOrNonCollision(Parent.World.PhysicsHost, model, pose);
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
            if (!Parent.World.Commander.CrossSections.All.GetValue(crossSectionKey, out CrossSection? crossSection))
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
