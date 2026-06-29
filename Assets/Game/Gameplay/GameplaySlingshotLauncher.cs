using System;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;

namespace Game.Gameplay
{
    public interface IGameplaySlingshotLauncher
    {
        void Launch(SlingshotLaunchRequest request);
    }

    public sealed class GameplaySlingshotLauncher : IGameplaySlingshotLauncher
    {
        private readonly IGameplaySlingshotLaunchConfig _config;
        private readonly SlingshotLaunchImpulseCalculator _calculator;
        private readonly ILaunchImpulseApplier _applier;
        private readonly IRunGameplayStatResolver _statResolver;
        private readonly GameplayStatId _slingshotLaunchPowerStatId;
        private readonly ISlingshotLaunchAppliedPublisher _launchAppliedPublisher;

        public GameplaySlingshotLauncher(
            IGameplaySlingshotLaunchConfig config,
            SlingshotLaunchImpulseCalculator calculator,
            ILaunchImpulseApplier applier,
            IRunGameplayStatResolver statResolver,
            GameplayStatId slingshotLaunchPowerStatId,
            ISlingshotLaunchAppliedPublisher launchAppliedPublisher)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _applier = applier ?? throw new ArgumentNullException(nameof(applier));
            _statResolver = statResolver ?? throw new ArgumentNullException(nameof(statResolver));

            _slingshotLaunchPowerStatId = slingshotLaunchPowerStatId != null
                ? slingshotLaunchPowerStatId
                : throw new ArgumentNullException(nameof(slingshotLaunchPowerStatId));
            
            _launchAppliedPublisher = launchAppliedPublisher ?? throw new ArgumentNullException(nameof(launchAppliedPublisher));
        }

        public void Launch(SlingshotLaunchRequest request)
        {
            var launchPower = ResolveLaunchPower();
            var impulse = _calculator.Calculate(request, _config, launchPower);
            _applier.Apply(request, impulse);

            _launchAppliedPublisher.Publish(new SlingshotLaunchAppliedEvent(
                request,
                impulse.VelocityChange,
                impulse.LaunchDirection,
                impulse.LaunchUpDirection));
        }

        private float ResolveLaunchPower()
        {
            return _statResolver.Resolve(_slingshotLaunchPowerStatId, 1f);
        }
    }
}
