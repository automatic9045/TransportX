using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Spatial
{
    public class MovableWorldSpaceModel : WorldSpaceModel, IMovable
    {
        private readonly new BodyTransformedModel Model;

        public Vector3 Velocity => Model.Velocity;
        public Vector3 AngularVelocity => Model.AngularVelocity;

        public MovableWorldSpaceModel(IWorldObject parent, BodyTransformedModel model) : base(parent, model)
        {
            Model = model;
        }
    }
}
