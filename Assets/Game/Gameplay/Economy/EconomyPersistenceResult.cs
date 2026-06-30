using System;

namespace Game.Gameplay.Economy
{
    public enum EconomyPersistenceStatus
    {
        Saved = 0,
        LoadedPrimary = 1,
        LoadedBackup = 2,
        LoadedDefaults = 3,
        Failed = 4
    }

    public readonly struct EconomyPersistenceResult
    {
        public EconomyPersistenceStatus Status { get; }
        public string Operation { get; }
        public string Message { get; }
        public Exception Exception { get; }

        public bool IsSuccess => Status != EconomyPersistenceStatus.Failed;

        public EconomyPersistenceResult(
            EconomyPersistenceStatus status,
            string operation,
            string message,
            Exception exception)
        {
            Status = status;
            Operation = operation ?? string.Empty;
            Message = message ?? string.Empty;
            Exception = exception;
        }
    }

    public readonly struct EconomyLoadResult
    {
        public PlayerEconomySnapshot Snapshot { get; }
        public EconomyPersistenceResult Result { get; }

        public EconomyLoadResult(PlayerEconomySnapshot snapshot, EconomyPersistenceResult result)
        {
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            Result = result;
        }
    }
}
