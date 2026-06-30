using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Gameplay.Tests.Common
{
    [UnityPlatform(
        RuntimePlatform.OSXEditor,
        RuntimePlatform.WindowsEditor,
        RuntimePlatform.LinuxEditor)]
    public abstract class BaseGameplayTestAssetsFixture
    {
        protected GameplayTestAssetsProvider TestAssets { get; private set; }

        [OneTimeSetUp]
        public void OnOneTimeSetUpGameplayTestAssetsFixture()
        {
            TestAssets = GameplayTestAssetsProvider.LoadSingleFromAssetDatabase();
        }
    }
}
