using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Rendering
{
    public interface IMesh : IDisposable
    {
        Material Material { get; }
        string? DebugName { get; set; }

        void Draw(DrawContext context);
    }
}
