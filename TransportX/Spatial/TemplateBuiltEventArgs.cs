using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Spatial
{
    public class TemplateBuiltEventArgs<TTemplate, TResult> : EventArgs
    {
        public TTemplate Template { get; }
        public TResult Result { get; }

        public TemplateBuiltEventArgs(TTemplate template, TResult result)
        {
            Template = template;
            Result = result;
        }
    }
}
