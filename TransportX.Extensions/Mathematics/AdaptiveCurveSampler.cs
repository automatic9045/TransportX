using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

namespace TransportX.Extensions.Mathematics
{
    public class AdaptiveCurveSampler : ICurveSampler
    {
        public float BaseInterval { get; }
        public float MaxDeviation { get; }
        public float MaxAngleDelta { get; }
        public float FromS { get; }
        public float ToS { get; }

        public AdaptiveCurveSampler(float baseInterval, float maxDeviation, float maxAngleDelta, float fromS, float toS)
        {
            BaseInterval = float.Max(1e-3f, baseInterval);
            MaxDeviation = float.Max(1e-3f, maxDeviation);
            MaxAngleDelta = float.Max(1e-3f, maxAngleDelta);
            FromS = float.Max(0, fromS);
            ToS = float.Max(FromS, toS);
        }

        public IReadOnlyList<float> Sample(IPoseCurve curve, float sOffset, float fromS, float toS)
        {
            float effectiveFromS = float.Max(FromS, fromS);
            float effectiveToS = float.Min(ToS, toS);

            float startS = sOffset;
            float endS = sOffset + curve.Length;

            if (endS <= effectiveFromS || effectiveToS <= startS) return [];

            float localStart = float.Max(0, effectiveFromS - sOffset);
            float localEnd = float.Min(curve.Length, effectiveToS - sOffset);
            float totalLength = localEnd - localStart;

            float minInterval = float.Max(1e-5f, float.Min(BaseInterval * 0.01f, MaxDeviation * 0.1f));
            float minStep = minInterval * 0.1f;

            if (totalLength < minStep) return [];

            List<float> samples = [localStart];

            float maxDeviationSquared = MaxDeviation * MaxDeviation;
            float minCosAngle = float.Cos(MaxAngleDelta);

            float anchorS = localStart;
            Pose anchorPose = curve.GetPose(anchorS);

            float s = localStart;
            while (s < localEnd)
            {
                float nextS = float.Min(s + BaseInterval, localEnd);
                Pose nextPose = curve.GetPose(nextS);

                float middleS = (anchorS + nextS) * 0.5f;
                Pose middlePose = curve.GetPose(middleS);

                if (!NeedsSubdivision(anchorPose, nextPose, middlePose))
                {
                    if (localEnd <= nextS)
                    {
                        if (minStep < nextS - samples[^1])
                        {
                            samples.Add(nextS);
                        }
                        break;
                    }

                    s = nextS;
                }
                else
                {
                    if (anchorS < s && minStep < s - samples[^1])
                    {
                        samples.Add(s);
                        anchorS = s;
                        anchorPose = curve.GetPose(anchorS);
                    }

                    Subdivide(anchorS, nextS, anchorPose, nextPose);

                    if (minStep < nextS - samples[^1])
                    {
                        samples.Add(nextS);
                    }

                    anchorS = nextS;
                    anchorPose = nextPose;
                    s = nextS;
                }
            }

            return samples.Count < 2 ? [] : samples;


            bool NeedsSubdivision(Pose pose0, Pose pose1, Pose middlePose)
            {
                Vector3 middlePosition = (pose0.Position + pose1.Position) * 0.5f;
                float deviationSquared = Vector3.DistanceSquared(middlePose.Position, middlePosition);
                if (maxDeviationSquared < deviationSquared) return true;

                float dot01 = float.Clamp(Vector3.Dot(pose0.Direction, pose1.Direction), -1, 1);
                float dot0M = float.Clamp(Vector3.Dot(pose0.Direction, middlePose.Direction), -1, 1);
                return dot01 < minCosAngle || dot0M < minCosAngle;
            }

            void Subdivide(float s0, float s1, Pose pose0, Pose pose1)
            {
                float length = s1 - s0;
                if (length <= minInterval) return;

                float middleS = (s0 + s1) * 0.5f;
                Pose middlePose = curve.GetPose(middleS);

                if (NeedsSubdivision(pose0, pose1, middlePose))
                {
                    Subdivide(s0, middleS, pose0, middlePose);
                    if (minStep < middleS - samples[^1])
                    {
                        samples.Add(middleS);
                    }
                    Subdivide(middleS, s1, middlePose, pose1);
                }
            }
        }

        public IReadOnlyList<float> Sample(IPoseCurve curve, float sOffset)
            => Sample(curve, sOffset, FromS, ToS);
    }
}
