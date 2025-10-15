using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Rendering
{
    public interface IDrawable
    {
        void Draw(DrawContext context);
    }
}
