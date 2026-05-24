using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Environment
{
    public class ToneMapProfile
    {
        public static readonly ToneMapProfile Default = new()
        {
            Contrast = 1.6f,
            Shoulder = 0.977f,
            MaxLuminance = 12,
            MidtoneScale = 1.5f,
        };

        /// <summary>
        /// 画面全体の明暗のメリハリ (S字カーブの傾き) を取得・設定します。
        /// </summary>
        public required float Contrast { get; init; }

        /// <summary>
        /// ハイライトが白に収束するときの丸み具合を取得・設定します。
        /// </summary>
        public required float Shoulder { get; init; }

        /// <summary>
        /// これ以上のHDR輝度を純白 (1.0) にする白飛び限界値を取得・設定します。
        /// </summary>
        public required float MaxLuminance { get; init; }

        /// <summary>
        /// 中間調および暗部の輝度を押し上げる倍率を取得・設定します (1.0 ~ 2.0 程度)。
        /// </summary>
        public required float MidtoneScale { get; init; }

        public ToneMapProfile()
        {
        }
    }
}
