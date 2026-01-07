using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Rendering;
using Bus.Common.Scenery;

using Bus.Common.Extensions.Networks;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scripting.Commands
{
    public class SplineCommand
    {
        private readonly ScriptWorld World;

        public SplineFactory SplineFactory { get; }

        public NetworkPort Inlet => SplineFactory.CreatedSplines[0].Inlet;
        public NetworkPort Outlet => SplineFactory.CreatedSplines[SplineFactory.CreatedSplines.Count - 1].Outlet;

        internal SplineCommand(ScriptWorld world, SplineFactory splineFactory)
        {
            World = world;
            SplineFactory = splineFactory;
        }

        public Spline Straight(double length)
        {
            Spline spline = SplineFactory.Straight((float)length);
            AddElementToPlate(spline);
            return spline;
        }

        public Spline Curve(double radius, double length)
        {
            Spline spline = SplineFactory.ByRadius((float)radius, (float)length);
            AddElementToPlate(spline);
            return spline;
        }

        public Spline CurveByCurvature(double curvature, double length)
        {
            Spline spline = SplineFactory.ByCurvature((float)curvature, (float)length);
            AddElementToPlate(spline);
            return spline;
        }

        public BezierSpline ConnectBezier(NetworkPort targetPort, double handleScale = 0.5)
        {
            BezierSpline spline = SplineFactory.InterpolateByBezier(targetPort, (float)handleScale);
            AddElementToPlate(spline);
            return spline;
        }

        public SplineCommand IntoSpline(string? templateKey)
        {
            PlateCommand plate = World.Commander.Plates[Outlet.Owner.PlateX, Outlet.Owner.PlateZ];
            SplineCommand spline = plate.BeginSpline(templateKey, SplineFactory.Transform, Outlet);

            return spline;
        }

        public JunctionCommand IntoJunction(string templateKey, string targetPortKey)
        {
            JunctionTemplate? template = World.Commander.Network.Templates.GetJunction(templateKey);
            PortDefinition? targetPort = template?.GetPort(targetPortKey);

            Junction junction = template is null || targetPort is null
                ? new Junction(SplineFactory.PlateX, SplineFactory.PlateZ, SplineFactory.Transform, [])
                : SplineFactory.ConnectNew(targetPort, template.Build);

            AddElementToPlate(junction);
            return new JunctionCommand(World, junction);
        }

        private void AddElementToPlate(NetworkElement element)
        {
            Plate plate = World.Plates.GetOrAdd(element.PlateX, element.PlateZ);
            plate.Network.Add(element);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, Matrix4x4 transform, double from, double span, double interval, int count = int.MaxValue)
        {
            LocatedModelTemplate[] models = modelKeys.Select(key =>
            {
                if (!World.Models.TryGetValue(key, out IModel? model))
                {
                    ScriptError error = new(ErrorLevel.Error, $"モデル '{key}' が見つかりません。");
                    World.ErrorCollector.Report(error);

                    model = Model.Empty();
                }

                return new LocatedModelTemplate(model, transform);
            }).ToArray();
            SplineStructure structure = new(models, (float)from, (float)span, (float)interval, count);
            SplineFactory.PutStructure(structure);

            return structure;
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval, int count = int.MaxValue)
        {
            SixDoF position = SixDoF.Deg((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKeys, position.CreateTransform(), from, span, interval, count);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double from, double span, double interval, int count = int.MaxValue)
        {
            return PutStructure(modelKeys, x, y, z, 0, 0, 0, from, span, interval, count);
        }
    }
}
