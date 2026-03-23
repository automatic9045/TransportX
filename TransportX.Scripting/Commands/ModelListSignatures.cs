using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Commands.Functions;

namespace TransportX.Scripting.Commands
{
    internal static class ModelListSignatures
    {
        public static readonly FunctionSignature LeftHanded = FunctionSignature.Create("LH");

        public static readonly FunctionSignature NonCollision = FunctionSignature.Create("NonCollision");

        public static readonly FunctionSignature BoundingBox1 = FunctionSignature.Create<float, float, float, float>("BoundingBox");
        public static readonly FunctionSignature BoundingBox2 = FunctionSignature.Create("BoundingBox");

        public static readonly FunctionSignature ConvexHull1 = FunctionSignature.Create<float, float, float, float>("ConvexHull");
        public static readonly FunctionSignature ConvexHull2 = FunctionSignature.Create("ConvexHull");

        public static readonly FunctionSignature ClosedModel1 = FunctionSignature.Create<string, float, float, float, float>("ClosedModel");
        public static readonly FunctionSignature ClosedModel2 = FunctionSignature.Create<string>("ClosedModel");
        public static readonly FunctionSignature ClosedModel3 = FunctionSignature.Create<float, float, float, float>("ClosedModel");
        public static readonly FunctionSignature ClosedModel4 = FunctionSignature.Create("ClosedModel");

        public static readonly FunctionSignature OpenModel1 = FunctionSignature.Create<string, float, float, float, float>("OpenModel");
        public static readonly FunctionSignature OpenModel2 = FunctionSignature.Create<string>("OpenModel");
        public static readonly FunctionSignature OpenModel3 = FunctionSignature.Create<float, float, float, float>("OpenModel");
        public static readonly FunctionSignature OpenModel4 = FunctionSignature.Create("OpenModel");

        public static readonly IReadOnlyList<FunctionSignature> All = [
            LeftHanded,
            NonCollision,
            BoundingBox1, BoundingBox2,
            ConvexHull1, ConvexHull2,
            ClosedModel1, ClosedModel2, ClosedModel3, ClosedModel4,
            OpenModel1, OpenModel2, OpenModel3, OpenModel4,
        ];
    }
}
