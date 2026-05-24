using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Environment
{
    public class BloomProfile
    {
        public static readonly BloomProfile Default = new()
        {
            Threshold = 2,
            Intensity = 0.2f,
            Scatter = 1,
            SoftKnee = 0.5f,
            Tint = Vector3.One,
        };


        /// <summary>
        /// ブルームを発生させる光の強さの閾値を取得・設定します。
        /// </summary>
        public required float Threshold { get; init; }

        /// <summary>
        /// ブルームの強度を取得・設定します。
        /// </summary>
        public required float Intensity { get; init; }

        /// <summary>
        /// ブルームの光の広がり (拡散) 具合を取得・設定します。
        /// </summary>
        public required float Scatter { get; init; }

        /// <summary>
        /// 閾値付近のあふれ出しを滑らかにするソフトニーの幅を取得・設定します。
        /// </summary>
        public required float SoftKnee { get; init; }

        /// <summary>
        /// ブルームとしてあふれ出た光に適用する色 (カラーティント) を取得・設定します。
        /// </summary>
        public required Vector3 Tint { get; init; }

        public BloomProfile()
        {
        }
    }
}
