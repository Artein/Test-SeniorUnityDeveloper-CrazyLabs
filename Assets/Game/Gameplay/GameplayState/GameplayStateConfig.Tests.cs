#if UNITY_INCLUDE_TESTS

using System.Collections.Generic;

namespace Game.Gameplay.GameplayState
{
    public sealed partial class GameplayStateConfig
    {
        internal void SetValuesForTests(
            GameplayStateId initialStateId,
            IReadOnlyList<GameplayStateTransition> allowedTransitions)
        {
            _initialStateId = initialStateId;
            _allowedTransitions.Clear();

            if (allowedTransitions is null)
                return;

            foreach (var transition in allowedTransitions)
            {
                _allowedTransitions.Add(transition);
            }
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
