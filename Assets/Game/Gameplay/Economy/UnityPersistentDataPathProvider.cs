using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Gameplay.Economy
{
    [UsedImplicitly]
    public sealed class UnityPersistentDataPathProvider : IPersistentDataPathProvider
    {
#if UNITY_INCLUDE_TESTS
        private readonly string _testPersistentDataPath = Path.Combine(
            Application.temporaryCachePath,
            "EconomyTests",
            Guid.NewGuid().ToString("N"));
#endif

        public string PersistentDataPath
        {
            get
            {
#if UNITY_INCLUDE_TESTS
                return _testPersistentDataPath;
#else
                return Application.persistentDataPath;
#endif
            }
        }
    }
}
