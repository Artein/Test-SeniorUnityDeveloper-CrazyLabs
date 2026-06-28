using System.Linq;
using Game.Gameplay.GameplayState;
using Game.Gameplay.GameplayState.Tests.EditMode;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class GameplayStateValidatorTests : GameplayStateTestFixture
{
    [Test]
    public void Validate_NullConfig_ReturnsNullConfig()
    {
        var errors = new GameplayStateValidator().Validate(null);

        Assert.That(
            errors.Select(error => error.Code),
            Is.EquivalentTo(new[] { GameplayStateValidationErrorCode.NullConfig }));
    }

    [Test]
    public void Validate_MissingInitialState_ReturnsMissingInitialState()
    {
        var config = CreateConfig(null);

        var errors = new GameplayStateValidator().Validate(config);

        Assert.That(
            errors.Select(error => error.Code),
            Does.Contain(GameplayStateValidationErrorCode.MissingInitialState));
    }

    [Test]
    public void Validate_NullTransitionEntry_ReturnsNullTransition()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var config = CreateConfig(preLaunch, (GameplayStateTransition)null);

        var errors = new GameplayStateValidator().Validate(config);

        Assert.That(
            errors.Select(error => error.Code),
            Does.Contain(GameplayStateValidationErrorCode.NullTransition));
    }

    [Test]
    public void Validate_MissingTransitionStateIds_ReturnsMissingFromAndTo()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var transition = CreateTransition(null, null);
        var config = CreateConfig(preLaunch, transition);

        var errors = new GameplayStateValidator().Validate(config);

        Assert.That(
            errors.Select(error => error.Code),
            Is.SupersetOf(new[]
            {
                GameplayStateValidationErrorCode.MissingTransitionFromState,
                GameplayStateValidationErrorCode.MissingTransitionToState
            }));
    }

    [Test]
    public void Validate_SelfTransition_ReturnsSelfTransition()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var transition = CreateTransition(preLaunch, preLaunch);
        var config = CreateConfig(preLaunch, transition);

        var errors = new GameplayStateValidator().Validate(config);

        Assert.That(
            errors.Select(error => error.Code),
            Does.Contain(GameplayStateValidationErrorCode.SelfTransition));
    }

    [Test]
    public void Validate_DuplicateTransitionPair_ReturnsDuplicateTransition()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var firstTransition = CreateTransition(preLaunch, running);
        var secondTransition = CreateTransition(preLaunch, running);
        var config = CreateConfig(preLaunch, firstTransition, secondTransition);

        var errors = new GameplayStateValidator().Validate(config);

        Assert.That(
            errors.Select(error => error.Code),
            Does.Contain(GameplayStateValidationErrorCode.DuplicateTransition));
    }
}
