using System;
using Game.Gameplay.Economy;
using JetBrains.Annotations;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    [UsedImplicitly]
    public sealed class PlayerEconomyStateLoader : IInitializable
    {
        private readonly PlayerEconomyState _state;
        private readonly IEconomySaveRepository _saveRepository;

        public PlayerEconomyStateLoader(PlayerEconomyState state, IEconomySaveRepository saveRepository)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _saveRepository = saveRepository ?? throw new ArgumentNullException(nameof(saveRepository));
        }

        void IInitializable.Initialize()
        {
            try
            {
                var loadResult = _saveRepository.Load();
                _state.ReplaceWith(loadResult.Snapshot);

                if (!loadResult.Result.IsSuccess)
                    Debug.LogError("Economy save load failed. " + loadResult.Result.Message);
            }
            catch (Exception exception)
            {
                Debug.LogError("Economy save load failed. " + exception.Message);
            }
        }
    }
}
