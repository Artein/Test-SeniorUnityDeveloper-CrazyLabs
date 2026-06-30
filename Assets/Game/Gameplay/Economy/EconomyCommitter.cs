using System;
using UnityEngine;

namespace Game.Gameplay.Economy
{
    public interface IEconomyCommitter
    {
        bool IsCommitPending { get; }
        EconomyPersistenceResult CommitImportant(string reason);
        EconomyPersistenceResult RequestBestEffortFlush(string reason);
    }

    public sealed class EconomyCommitter : IEconomyCommitter
    {
        private readonly PlayerEconomyState _state;
        private readonly EconomySaveQueue _saveQueue;

        public bool IsCommitPending { get; private set; }

        public EconomyCommitter(PlayerEconomyState state, EconomySaveQueue saveQueue)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _saveQueue = saveQueue ?? throw new ArgumentNullException(nameof(saveQueue));
        }

        public EconomyPersistenceResult CommitImportant(string reason)
        {
            return Commit(reason);
        }

        public EconomyPersistenceResult RequestBestEffortFlush(string reason)
        {
            return Commit(reason);
        }

        private EconomyPersistenceResult Commit(string reason)
        {
            var snapshot = _state.CreateSnapshot();
            IsCommitPending = true;

            try
            {
                var result = _saveQueue.EnqueueImportantAsync(snapshot, reason).GetAwaiter().GetResult();

                if (!result.IsSuccess)
                    Debug.LogError("Economy save commit failed. " + result.Message);

                return result;
            }
            catch (Exception exception)
            {
                Debug.LogError("Economy save commit failed. " + exception.Message);

                return new EconomyPersistenceResult(
                    EconomyPersistenceStatus.Failed,
                    reason,
                    exception.Message,
                    exception);
            }
            finally
            {
                IsCommitPending = false;
            }
        }
    }

    public sealed class NoOpEconomyCommitter : IEconomyCommitter
    {
        public bool IsCommitPending => false;

        public EconomyPersistenceResult CommitImportant(string reason)
        {
            return CreateSuccess(reason);
        }

        public EconomyPersistenceResult RequestBestEffortFlush(string reason)
        {
            return CreateSuccess(reason);
        }

        private EconomyPersistenceResult CreateSuccess(string reason)
        {
            return new EconomyPersistenceResult(
                EconomyPersistenceStatus.Saved,
                reason,
                "No economy persistence configured.",
                exception: null);
        }
    }
}
