using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Constraints;

using TransportX.Diagnostics;
using TransportX.Spatial;
using TransportX.Scripting.Collections;
using TransportX.Physics;
using BepuUtilities;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Joints
    {
        private readonly ScriptAvatar Avatar;

        private readonly ScriptKeyedList<string, Joint> AllKey;
        public IReadOnlyScriptKeyedList<string, Joint> All => AllKey;

        internal Joints(ScriptAvatar avatar)
        {
            Avatar = avatar;

            AllKey = new ScriptKeyedList<string, Joint>(joint => joint.Key, Avatar.ErrorCollector, "ジョイント", key => Joint.Empty(Avatar, key));
        }

        internal void Dispose()
        {
            foreach (Joint joint in All)
            {
                Avatar.PhysicsHost.Simulation.Solver.Remove(joint.Handle);
            }
        }

        public Joint Add<TDescription>(string key, DynamicPart partA, DynamicPart partB, in TDescription description)
            where TDescription : unmanaged, ITwoBodyConstraintDescription<TDescription>
        {
            ConstraintHandle handle = Avatar.PhysicsHost.Simulation.Solver.Add(partA.Model.Handle, partB.Model.Handle, description);

            Joint joint = new(Avatar, key, handle);
            AllKey.Add(joint);
            return joint;
        }

        public Joint Add<TDescription>(string key, string partKeyA, string partKeyB, in TDescription description)
            where TDescription : unmanaged, ITwoBodyConstraintDescription<TDescription>
        {
            if (!CheckPart(partKeyA, out DynamicPart? partA)) return Joint.Empty(Avatar, key);
            if (!CheckPart(partKeyB, out DynamicPart? partB)) return Joint.Empty(Avatar, key);

            return Add(key, partA, partB, description);


            bool CheckPart(string partKey, [MaybeNullWhen(false)] out DynamicPart dynamicPart)
            {
                if (!Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part? part))
                {
                    dynamicPart = null;
                    return false;
                }
                else if (part is not DynamicPart dynamicPartCast)
                {
                    ScriptError error = new(ErrorLevel.Error, "ジョイントにダイナミックでないパーツを接続することはできません。");
                    Avatar.ErrorCollector.Report(error);

                    dynamicPart = null;
                    return false;
                }
                else
                {
                    dynamicPart = dynamicPartCast;
                    return true;
                }
            }
        }

        public Joint Add<TDescription>(string key, DynamicPart partA, DynamicPart partB, Constraint<TDescription> constraint)
            where TDescription : unmanaged, ITwoBodyConstraintDescription<TDescription>
        {
            BodyEnumerator enumerator = new();
            Avatar.PhysicsHost.Simulation.Solver.EnumerateConnectedBodyReferences(constraint.Handle, ref enumerator);
            
            if (enumerator.Bodies.Count != 2)
            {
                ScriptError error = new(ErrorLevel.Error, "2 体間制約以外には対応していません。");
                Avatar.ErrorCollector.Report(error);
                return Joint.Empty(Avatar, key);
            }

            if (enumerator.Bodies[0] != partA.Model.Handle) return ReportInvalidBody("A", partA);
            if (enumerator.Bodies[1] != partB.Model.Handle) return ReportInvalidBody("B", partB);

            Joint joint = new(Avatar, key, constraint.Handle);
            AllKey.Add(joint);
            return joint;


            Joint ReportInvalidBody(string bodyName, DynamicPart part)
            {
                ScriptError error = new(ErrorLevel.Error, $"この制約が対象としている物体 {bodyName} はパーツ '{part.Key}' ではありません。");
                Avatar.ErrorCollector.Report(error);
                return Joint.Empty(Avatar, key);
            }
        }

        internal void RegisterComponents()
        {
            foreach (Joint joint in All)
            {
                joint.RegisterComponents();
            }
        }


        private readonly struct BodyEnumerator : IForEach<int>
        {
            private readonly List<BodyHandle> BodiesKey = [];
            public IReadOnlyList<BodyHandle> Bodies => BodiesKey;

            public BodyEnumerator()
            {
            }

            public void LoopBody(int i)
            {
                BodyHandle body = new(i);
                BodiesKey.Add(body);
            }
        }
    }
}
