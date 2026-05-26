using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Environment
{
    public class IBLProfile
    {
        public static readonly IBLProfile Default = new()
        {
            Intensity = 1,
            Saturation = 0,
        };


        /// <summary>
        /// 環境光の強度を取得・設定します。
        /// </summary>
        public required float Intensity { get; init; }

        /// <summary>
        /// 環境光の彩度を取得・設定します。
        /// </summary>
        public required float Saturation { get; init; }

        public IBLProfile()
        {
        }
    }
}
