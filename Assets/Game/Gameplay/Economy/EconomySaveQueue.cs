using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Economy
{
    public sealed class EconomySaveQueue
    {
        private readonly IEconomySaveRepository _repository;
        private readonly object _gate = new();

        private Task _tail = Task.CompletedTask;

        public EconomySaveQueue(IEconomySaveRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<EconomyPersistenceResult> EnqueueImportantAsync(PlayerEconomySnapshot snapshot, string reason)
        {
            if (snapshot is null)
                throw new ArgumentNullException(nameof(snapshot));

            lock (_gate)
            {
                var previous = _tail;

                var saveTask = previous.ContinueWith(
                    _ => SaveSnapshot(snapshot, reason),
                    TaskScheduler.Default);

                _tail = saveTask;
                return saveTask;
            }
        }

        private EconomyPersistenceResult SaveSnapshot(PlayerEconomySnapshot snapshot, string reason)
        {
            try
            {
                return _repository.Save(snapshot, reason);
            }
            catch (Exception exception)
            {
                Debug.LogError("Economy save queue worker failed. " + exception.Message);

                return new EconomyPersistenceResult(
                    EconomyPersistenceStatus.Failed,
                    reason,
                    exception.Message,
                    exception);
            }
        }
    }
}
