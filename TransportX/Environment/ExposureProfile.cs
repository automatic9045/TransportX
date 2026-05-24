using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Environment
{
    public class ExposureProfile
    {
        public static readonly ExposureProfile Default = new()
        {
            Key = 0.118f,
            Min = 0.01f,
            Max = 5,
            DarkAdaptationSpeed = 0.5f,
            LightAdaptationSpeed = 1.5f,
        };

        /// <summary>
        /// 自動露出が目標とする画面の基準輝度を取得・設定します。
        /// </summary>
        public required float Key { get; init; }

        /// <summary>
        /// 露出係数の下限値 (これ以上暗く絞らない制限) を取得・設定します。
        /// </summary>
        public required float Min { get; init; }

        /// <summary>
        /// 露出係数の上限値 (これ以上明るく広げない制限) を取得・設定します。
        /// </summary>
        public required float Max { get; init; }

        /// <summary>
        /// 暗い場所に突入したときの目の慣れやすさ (暗順応速度) を取得・設定します。
        /// </summary>
        public required float DarkAdaptationSpeed { get; init; }

        /// <summary>
        /// 明るい場所に突入したときの目の慣れやすさ (明順応速度) を取得・設定します。
        /// </summary>
        public required float LightAdaptationSpeed { get; init; }

        public ExposureProfile()
        {
        }
    }
}
