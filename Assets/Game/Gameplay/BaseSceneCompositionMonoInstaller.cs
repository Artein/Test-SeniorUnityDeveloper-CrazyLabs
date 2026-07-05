using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public abstract class BaseSceneCompositionMonoInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);

        internal virtual IEnumerable<string> GetReferenceValidationErrors()
        {
            yield break;
        }
    }
}
