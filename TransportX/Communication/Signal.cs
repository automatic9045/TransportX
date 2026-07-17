using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Communication
{
    public class Signal<T>
    {
        public T Value
        {
            get;
            set
            {
                T oldValue = field;
                field = value;
                ValueChanged?.Invoke(oldValue, value);
            }
        }

        public event ValueChangedEventHandler? ValueChanged;

        public Signal(T initialValue)
        {
            Value = initialValue;
        }


        public delegate void ValueChangedEventHandler(T oldValue, T newValue);
    }
}
