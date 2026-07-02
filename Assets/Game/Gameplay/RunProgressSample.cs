namespace Game.Gameplay
{
    public readonly struct RunProgressSample
    {
        public bool HasValidSnapshot { get; }
        public string SnapshotError { get; }
        public RunProgressFrameSnapshot Snapshot { get; }
        public float CurrentForwardProgress { get; }
        public float MaximumForwardProgress { get; }

        public RunProgressSample(
            bool hasValidSnapshot,
            string snapshotError,
            RunProgressFrameSnapshot snapshot,
            float currentForwardProgress,
            float maximumForwardProgress)
        {
            HasValidSnapshot = hasValidSnapshot;
            SnapshotError = snapshotError ?? string.Empty;
            Snapshot = snapshot;
            CurrentForwardProgress = currentForwardProgress;
            MaximumForwardProgress = maximumForwardProgress;
        }
    }
}
