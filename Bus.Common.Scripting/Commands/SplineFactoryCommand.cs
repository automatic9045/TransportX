using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Rendering;
using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

using Bus.Common.Extensions.Networks.Elements;

namespace Bus.Common.Scripting.Commands
{
    public class SplineFactoryCommand
    {
        private readonly ScriptWorld World;

        public SplineFactory SplineFactory { get; }

        public CurveList Curves { get; }
        public GradientList Gradients { get; }
        public CantList Cants { get; }

        public string Name
        {
            get => SplineFactory.DebugName ?? nameof(Spline);
            set => SplineFactory.DebugName = value;
        }

        internal SplineFactoryCommand(ScriptWorld world, SplineFactory splineFactory)
        {
            World = world;
            SplineFactory = splineFactory;

            Curves = new CurveList(SplineFactory.Curves);
            Gradients = new GradientList(SplineFactory.Gradients);
            Cants = new CantList(SplineFactory.Cants);
        }

        public void ConnectBezier(NetworkPort targetPort, double handleScale = 0.5)
        {
            SplineFactory.InterpolateByBezier(targetPort, (float)handleScale);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, Pose pose, double from, double span, double interval, int count = int.MaxValue)
        {
            LocatedModelTemplate[] models = modelKeys.Select(key =>
            {
                if (!World.Models.TryGetValue(key, out IModel? model))
                {
                    ScriptError error = new(ErrorLevel.Error, $"モデル '{key}' が見つかりません。");
                    World.ErrorCollector.Report(error);

                    model = Model.Empty();
                }

                return new LocatedModelTemplate(model, pose);
            }).ToArray();
            SplineStructure structure = new(models, (float)from, (float)span, (float)interval, count);
            SplineFactory.PutStructure(structure);

            return structure;
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval, int count = int.MaxValue)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKeys, position.ToPose(), from, span, interval, count);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double from, double span, double interval, int count = int.MaxValue)
        {
            return PutStructure(modelKeys, x, y, z, 0, 0, 0, from, span, interval, count);
        }

        public SplineCommand Build()
        {
            List<SplineBase> splines = SplineFactory.Build();
            foreach (SplineBase spline in splines)
            {
                Plate plate = World.Plates.GetOrAdd(spline.PlateX, spline.PlateZ);
                plate.Network.Add(spline);

                foreach (LanePath path in spline.Paths)
                {
                    path.CreateDebugResources(World.DXHost.Device);
                    path.DebugColor = World.Commander.Network.LaneTraffic.GetGroupColor(path.AllowedTraffic);
                }
            }

            return new SplineCommand(World, splines);
        }


        public class CurveList
        {
            private readonly SplineFactory.CurveList List;

            internal CurveList(SplineFactory.CurveList list)
            {
                List = list;
            }

            public void Straight(double length) => List.Straight((float)length);
            public void ByRadius(double radius, double length) => List.ByRadius((float)radius, (float)length);
            public void ByCurvature(double curvature, double length) => List.ByCurvature((float)curvature, (float)length);
        }

        public class GradientList
        {
            private readonly SplineFactory.GradientList List;

            internal GradientList(SplineFactory.GradientList list)
            {
                List = list;
            }

            public void Constant(double length) => List.Constant((float)length);
            public void TransitionByRadian(double radian, double length) => List.TransitionBy((float)radian, (float)length);
            public void TransitionByDegree(double degree, double length) => TransitionByRadian(degree / 180 * double.Pi, length);
            public void TransitionByPercent(double percent, double length) => TransitionByRadian(double.Atan(percent / 100), length);
            public void TransitionByPermil(double permil, double length) => TransitionByRadian(double.Atan(permil / 1000), length);
        }

        public class CantList
        {
            private readonly SplineFactory.CantList List;

            internal CantList(SplineFactory.CantList list)
            {
                List = list;
            }

            public void Constant(double length) => List.Constant((float)length);
            public void TransitionToRadian(double radian, double length) => List.TransitionTo((float)radian, (float)length);
            public void TransitionToDegree(double degree, double length) => TransitionToRadian(degree / 180 * double.Pi, length);
            public void TransitionToPercent(double percent, double length) => TransitionToRadian(double.Atan(percent / 100), length);
            public void TransitionToHeight(double height, double gauge, double length)
            {
                double ratio = double.Clamp(height / gauge, -1, 1);
                TransitionToRadian(double.Asin(ratio), length);
            }
        }
    }
}
