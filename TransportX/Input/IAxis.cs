using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input
{
    public interface IAxis : IDisposable
    {
        public static IAxis Empty(string key) => new EmptyAxis(key);


        string Key { get; }

        float Min { get; }
        float Neutral { get; }
        float Max { get; }

        float Value { get; }

        void Tick(TimeSpan elapsed);


        private class EmptyAxis : IAxis
        {
            public string Key { get; }

            public float Min => 0;
            public float Neutral => 0;
            public float Max => 0;

            public float Value => 0;

            public EmptyAxis(string key)
            {
                Key = key;
            }

            public void Dispose()
            {
            }

            public void Tick(TimeSpan elapsed)
            {
            }
        }
    }
}
