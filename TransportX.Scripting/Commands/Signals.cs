using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

namespace TransportX.Scripting.Commands
{
    public class Signals
    {
        public SignalBus Source { get; }

        public Signal<float> Float(string key) => Source.Single.GetOrAdd(key, key => new Signal<float>(0));
        public Signal<int> Int(string key) => Source.Int32.GetOrAdd(key, key => new Signal<int>(0));
        public Signal<bool> Bool(string key) => Source.Boolean.GetOrAdd(key, key => new Signal<bool>(false));

        public Signals(SignalBus source)
        {
            Source = source;
        }

        public float ReadFloat(string key) => Float(key).Value;
        public int ReadInt(string key) => Int(key).Value;
        public bool ReadBool(string key) => Bool(key).Value;

        public Signal<float> WriteFloat(string key, float value)
        {
            Signal<float> signal = Float(key);
            signal.Value = value;
            return signal;
        }

        public Signal<int> WriteInt(string key, int value)
        {
            Signal<int> signal = Int(key);
            signal.Value = value;
            return signal;
        }

        public Signal<bool> WriteBool(string key, bool value)
        {
            Signal<bool> signal = Bool(key);
            signal.Value = value;
            return signal;
        }


        public Signal<float> ForwardFloat(string key, Func<float> valueFactory)
        {
            Signal<float> signal = Float(key);
            Source.Forward(signal, valueFactory);
            return signal;
        }

        public Signal<int> ForwardInt(string key, Func<int> valueFactory)
        {
            Signal<int> signal = Int(key);
            Source.Forward(signal, valueFactory);
            return signal;
        }

        public Signal<bool> ForwardBool(string key, Func<bool> valueFactory)
        {
            Signal<bool> signal = Bool(key);
            Source.Forward(signal, valueFactory);
            return signal;
        }
    }
}
