using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Diagnostics
{
    public class ErrorEventArgs : EventArgs
    {
        public Error Error { get; }

        public ErrorEventArgs(Error error)
        {
            Error = error;
        }
    }
}
