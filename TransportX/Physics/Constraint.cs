using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Constraints;

namespace TransportX.Physics
{
    public class Constraint<TDescription> where TDescription : unmanaged, IConstraintDescription<TDescription>
    {
        private readonly Simulation Simulation;

        public ConstraintHandle Handle { get; }
        public TDescription Description
        {
            get
            {
                Simulation.Solver.GetDescription(Handle, out TDescription description);
                return description;
            }
            set => Simulation.Solver.ApplyDescription(Handle, value);
        }

        public Constraint(Simulation simulation, ConstraintHandle handle)
        {
            Simulation = simulation;
            Handle = handle;
        }

        public void Update(Converter<TDescription, TDescription> updater)
        {
            TDescription oldDesc = Description;
            TDescription newDesc = updater(oldDesc);
            Description = newDesc;
        }
    }
}
