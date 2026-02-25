using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Components
{
    public class ComponentEventArgs<T> : EventArgs where T : IComponent
    {
        public T Item { get; }

        public ComponentEventArgs(T item)
        {
            Item = item;
        }
    }
}
