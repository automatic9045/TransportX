using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public readonly struct TextureSlot
    {
        public static readonly TextureSlot Empty = new(null, TextureSampleMode.None);


        public ID3D11ShaderResourceView? View { get; } = null;
        public TextureSampleMode Mode { get; } = TextureSampleMode.None;

        public TextureSlot(ID3D11ShaderResourceView? view, TextureSampleMode mode)
        {
            View = view;
            Mode = view is null ? TextureSampleMode.None : mode;
        }

        public static implicit operator TextureSlot(ID3D11ShaderResourceView? view)
        {
            return new TextureSlot(view, TextureSampleMode.Texture);
        }
    }
}
