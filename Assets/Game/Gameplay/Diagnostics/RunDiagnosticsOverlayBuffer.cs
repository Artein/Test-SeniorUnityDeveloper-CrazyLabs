using System;

namespace Game.Gameplay.Diagnostics
{
    internal sealed class RunDiagnosticsOverlayBuffer
    {
        private readonly RunDiagnosticsOverlaySample[] _samples;

        private int _nextIndex;

        public int Capacity => _samples.Length;
        public int Count { get; private set; }
        public RunDiagnosticsOverlaySample Latest { get; private set; }

        public RunDiagnosticsOverlayBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Diagnostics sample capacity must be positive.");

            _samples = new RunDiagnosticsOverlaySample[capacity];
        }

        public void Add(RunDiagnosticsOverlaySample sample)
        {
            _samples[_nextIndex] = sample;
            Latest = sample;
            _nextIndex = (_nextIndex + 1) % _samples.Length;

            if (Count < _samples.Length)
                Count += 1;
        }

        public RunDiagnosticsOverlaySample GetChronological(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Diagnostics sample index is outside the buffered range.");

            var oldestIndex = Count == _samples.Length ? _nextIndex : 0;
            var sampleIndex = (oldestIndex + index) % _samples.Length;
            return _samples[sampleIndex];
        }

        public void Clear()
        {
            _nextIndex = 0;
            Count = 0;
            Latest = default;
        }
    }
}
