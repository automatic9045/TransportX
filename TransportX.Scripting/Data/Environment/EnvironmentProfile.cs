using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Data.Environment
{
    [XmlRoot]
    public class EnvironmentProfile
    {
        public IBL IBL = new();
        public Bloom Bloom = new();

        [XmlIgnore]
        public Error[] Errors => [.. IBL.Errors, .. Bloom.Errors];
    }
}
