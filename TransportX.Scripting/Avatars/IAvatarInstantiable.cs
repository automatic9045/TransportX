using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Avatars
{
    public interface IAvatarInstantiable<T> where T : IAvatarInstantiable<T>
    {
        static abstract T Create(ScriptAvatar avatar);
    }
}
