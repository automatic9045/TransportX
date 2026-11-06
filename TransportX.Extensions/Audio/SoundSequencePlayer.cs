using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;

namespace TransportX.Extensions.Audio
{
    public class SoundSequencePlayer
    {
        private readonly Queue<QueueItem> PlayQueue = new();
        private TimeSpan DelayRemaining = TimeSpan.Zero;
        private bool IsWaitingDelay = false;

        public ISoundSource Source { get; }
        public ISoundAsset? Asset { get; private set; } = null;
        public int RemainingCount => PlayQueue.Count;
        public bool IsPlaying => Asset is not null;

        public event Action<ISoundAsset>? SoundStarted;
        public event Action<ISoundAsset>? SoundCompleted;
        public event Action? SequenceCompleted;

        public SoundSequencePlayer(ISoundSource source)
        {
            Source = source;
        }

        public void Enqueue(ISoundAsset asset, TimeSpan delayBeforeStart, float volume = 1, float pitch = 1)
        {
            if (delayBeforeStart < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delayBeforeStart));

            QueueItem item = new(asset, volume, pitch, delayBeforeStart);
            if (Asset is null)
            {
                PlayQueue.Enqueue(item);
                PlayNext();
            }
            else
            {
                PlayQueue.Enqueue(item);
            }
        }

        public void Enqueue(ISoundAsset asset, float volume = 1, float pitch = 1) => Enqueue(asset, TimeSpan.Zero, volume, pitch);

        public void Stop()
        {
            PlayQueue.Clear();

            if (Asset is not null)
            {
                Source.Stop();
                Asset = null;
            }

            IsWaitingDelay = false;
            DelayRemaining = TimeSpan.Zero;
        }

        public void Skip()
        {
            if (Asset is null) return;

            ISoundAsset skippedAsset = Asset;
            Source.Stop();

            Asset = null;
            IsWaitingDelay = false;
            DelayRemaining = TimeSpan.Zero;

            SoundCompleted?.Invoke(skippedAsset);
            PlayNext();
        }

        public void PlayImmediate(ISoundAsset asset, TimeSpan delayBeforeStart)
        {
            Stop();
            Enqueue(asset, delayBeforeStart);
        }

        public void PlayImmediate(ISoundAsset asset) => PlayImmediate(asset, TimeSpan.Zero);

        public void Tick(TimeSpan elapsed)
        {
            if (Asset is null) return;

            if (IsWaitingDelay)
            {
                DelayRemaining -= elapsed;
                if (DelayRemaining.Ticks < 1)
                {
                    IsWaitingDelay = false;
                    DelayRemaining = TimeSpan.Zero;

                    Source.Play(Asset, false);
                    SoundStarted?.Invoke(Asset);
                }

                return;
            }

            if (!Source.IsPlaying)
            {
                ISoundAsset completedAsset = Asset;
                Asset = null;

                SoundCompleted?.Invoke(completedAsset);
                PlayNext();
            }
        }

        private void PlayNext()
        {
            if (PlayQueue.Count < 1)
            {
                Asset = null;
                SequenceCompleted?.Invoke();
                return;
            }

            QueueItem item = PlayQueue.Dequeue();
            Asset = item.Asset;

            if (0 < item.DelayBeforeStart.Ticks)
            {
                IsWaitingDelay = true;
                DelayRemaining = item.DelayBeforeStart;
            }
            else
            {
                IsWaitingDelay = false;
                DelayRemaining = TimeSpan.Zero;

                Source.Play(Asset, false);
                SoundStarted?.Invoke(Asset);
            }
        }


        private readonly record struct QueueItem(ISoundAsset Asset, float Volume, float Pitch, TimeSpan DelayBeforeStart);
    }
}
