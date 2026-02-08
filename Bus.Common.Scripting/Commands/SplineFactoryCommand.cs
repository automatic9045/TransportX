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

                foreach (ILanePath path in spline.Paths)
                {
                    path.CreateDebugModel(World.DXHost.Device);
                    path.DebugModel!.Color = World.Commander.Network.LaneTraffic.GetGroupColor(path.AllowedTraffic);
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

            public CurveList Straight(double length, out double s)
            {
                List.Straight((float)length);
                s = List.S;
                return this;
            }

            public CurveList ByRadius(double radius, double length, out double s)
            {
                List.ByRadius((float)radius, (float)length);
                s = List.S;
                return this;
            }

            public CurveList ByCurvature(double curvature, double length, out double s)
            {
                List.ByCurvature((float)curvature, (float)length);
                s = List.S;
                return this;
            }

            public CurveList Straight(double length) => Straight(length, out _);
            public CurveList ByRadius(double radius, double length) => ByRadius(radius, length, out _);
            public CurveList ByCurvature(double curvature, double length) => ByCurvature(curvature, length, out _);
        }

        public class GradientList
        {
            private readonly SplineFactory.GradientList List;

            internal GradientList(SplineFactory.GradientList list)
            {
                List = list;
            }

            public GradientList Constant(double length, out double s)
            {
                List.Constant((float)length);
                s = List.S;
                return this;
            }
            public GradientList TransitionByRadian(double radian, double length, out double s)
            {
                List.TransitionBy((float)radian, (float)length);
                s = List.S;
                return this;
            }

            public GradientList TransitionByDegree(double degree, double length, out double s) => TransitionByRadian(degree / 180 * double.Pi, length, out s);
            public GradientList TransitionByPercent(double percent, double length, out double s) => TransitionByRadian(double.Atan(percent / 100), length, out s);
            public GradientList TransitionByPermil(double permil, double length, out double s) => TransitionByRadian(double.Atan(permil / 1000), length, out s);

            public GradientList Constant(double length) => Constant(length, out _);
            public GradientList TransitionByRadian(double radian, double length) => TransitionByRadian(radian, length, out _);
            public GradientList TransitionByDegree(double degree, double length) => TransitionByDegree(degree, length, out _);
            public GradientList TransitionByPercent(double percent, double length) => TransitionByPercent(percent, length, out _);
            public GradientList TransitionByPermil(double permil, double length) => TransitionByPermil(permil, length, out _);
        }

        public class CantList
        {
            private readonly SplineFactory.CantList List;

            internal CantList(SplineFactory.CantList list)
            {
                List = list;
            }

            public CantList Constant(double length, out double s)
            {
                List.Constant((float)length);
                s = List.S;
                return this;
            }

            public CantList TransitionToRadian(double radian, double length, out double s)
            {
                List.TransitionTo((float)radian, (float)length);
                s = List.S;
                return this;
            }

            public CantList TransitionToDegree(double degree, double length, out double s) => TransitionToRadian(degree / 180 * double.Pi, length, out s);
            public CantList TransitionToPercent(double percent, double length, out double s) => TransitionToRadian(double.Atan(percent / 100), length, out s);
            public CantList TransitionToHeight(double height, double gauge, double length, out double s)
            {
                double ratio = double.Clamp(height / gauge, -1, 1);
                return TransitionToRadian(double.Asin(ratio), length, out s);
            }

            public CantList Constant(double length) => Constant(length, out _);
            public CantList TransitionToRadian(double radian, double length) => TransitionToRadian(radian, length, out _);
            public CantList TransitionToDegree(double degree, double length) => TransitionToDegree(degree, length, out _);
            public CantList TransitionToPercent(double percent, double length) => TransitionToPercent(percent, length, out _);
            public CantList TransitionToHeight(double height, double gauge, double length) => TransitionToHeight(height, gauge, length, out _);
        }
    }
}
