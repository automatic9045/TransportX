using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Traffic;

namespace Bus.Common.Extensions.Traffic
{
    public interface IParticipantFactory
    {
        float Length { get; }

        ITrafficParticipant Create();
    }
}
