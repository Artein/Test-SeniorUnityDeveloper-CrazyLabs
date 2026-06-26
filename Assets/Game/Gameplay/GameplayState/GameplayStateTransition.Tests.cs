#if UNITY_INCLUDE_TESTS

namespace Game.Gameplay.GameplayState
{
    public sealed partial class GameplayStateTransition
    {
        internal void SetStateIdsForTests(GameplayStateId fromStateId, GameplayStateId toStateId)
        {
            _fromStateId = fromStateId;
            _toStateId = toStateId;
        }
    }
}

#endif
