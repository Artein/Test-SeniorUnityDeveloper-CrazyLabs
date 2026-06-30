using JetBrains.Annotations;
using UnityEngine;

namespace Game.Foundation.Persistence
{
    public interface IPersistentDataPathProvider
    {
        string PersistentDataPath { get; }
    }

    [UsedImplicitly]
    public sealed class UnityPersistentDataPathProvider : IPersistentDataPathProvider
    {
        public string PersistentDataPath => Application.persistentDataPath;
    }
}
