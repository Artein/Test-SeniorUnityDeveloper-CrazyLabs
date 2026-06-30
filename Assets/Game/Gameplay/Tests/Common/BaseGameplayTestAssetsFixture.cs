using NUnit.Framework;

namespace Game.Gameplay.Tests.Common
{
    public abstract class BaseGameplayTestAssetsFixture
    {
        protected GameplayTestAssetsProvider TestAssets { get; private set; }

        [OneTimeSetUp]
        public void OneTimeSetUpGameplayTestAssetsFixture()
        {
            TestAssets = GameplayTestAssetsProvider.LoadSingleFromAssetDatabase();
        }
    }
}
