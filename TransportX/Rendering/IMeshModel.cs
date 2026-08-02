using System;
using System.Collections.Generic;
using System.Text;

namespace TransportX.Rendering
{
    public interface IMeshModel : IModel
    {
        IReadOnlyList<IMesh> VisualMeshes { get; }
    }
}
