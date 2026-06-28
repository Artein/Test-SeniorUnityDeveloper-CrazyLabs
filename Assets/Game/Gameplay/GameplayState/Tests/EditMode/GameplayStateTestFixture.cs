using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Game.Gameplay.GameplayState.Tests.EditMode
{
    public abstract class GameplayStateTestFixture
    {
        private readonly List<UnityEngine.Object> _objects = new();

        [TearDown]
        public void OnTearDown()
        {
            foreach (var unityObject in _objects)
            {
                Object.DestroyImmediate(unityObject);
            }

            _objects.Clear();
        }

        protected GameplayStateId CreateStateId(string stateName)
        {
            var stateId = Track(ScriptableObject.CreateInstance<GameplayStateId>());
            stateId.name = stateName;
            return stateId;
        }

        protected GameplayStateTransition CreateTransition(
            GameplayStateId fromStateId,
            GameplayStateId toStateId)
        {
            var transition = Track(ScriptableObject.CreateInstance<GameplayStateTransition>());
            transition.SetStateIdsForTests(fromStateId, toStateId);
            return transition;
        }

        protected GameplayStateConfig CreateConfig(
            GameplayStateId initialStateId,
            params GameplayStateTransition[] transitions)
        {
            var config = Track(ScriptableObject.CreateInstance<GameplayStateConfig>());
            config.SetValuesForTests(initialStateId, transitions);
            return config;
        }

        protected GameplayStateService CreateService(GameplayStateConfig config)
        {
            return new GameplayStateService(config, new GameplayStateValidator(), new GameplayStateModel());
        }

        private T Track<T>(T value)
            where T : UnityEngine.Object
        {
            _objects.Add(value);
            return value;
        }
    }
}
