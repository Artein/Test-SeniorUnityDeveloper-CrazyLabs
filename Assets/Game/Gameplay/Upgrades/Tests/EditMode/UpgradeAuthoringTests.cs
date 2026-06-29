using System;
using System.Collections.Generic;
using Game.Gameplay.Economy;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class UpgradeAuthoringTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private CurrencyDefinition _coins;
    private GameplayStatId _statId;
    private Sprite _icon;

    [SetUp]
    public void OnSetUp()
    {
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.name = "Coins";
        _statId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
        _statId.SetValuesForTests("SlingshotLaunchPower");
        _icon = CreateIcon();
    }

    [TearDown]
    public void OnTearDown()
    {
        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void GetEffectValue_LevelZero_ReturnsBaselineEffect()
    {
        var definition = CreateValidDefinition(effectProgression: LinearProgression(1f, 2f));
        var evaluator = new UpgradeDefinitionEvaluator();

        var effect = evaluator.GetEffectValue(definition, 0);

        Assert.That(effect, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void ShortDisplayName_Set_ReturnsCompactName()
    {
        var definition = CreateValidDefinition(shortDisplayName: "POWER");

        Assert.That(definition.ShortDisplayName, Is.EqualTo("POWER"));
    }

    [Test]
    public void ShortDisplayName_Empty_ReturnsDisplayName()
    {
        var definition = CreateValidDefinition(shortDisplayName: string.Empty);

        Assert.That(definition.ShortDisplayName, Is.EqualTo(definition.DisplayName));
    }

    [Test]
    public void GetCostValue_LevelZero_Throws()
    {
        var definition = CreateValidDefinition();
        var evaluator = new UpgradeDefinitionEvaluator();

        Assert.That(
            () => evaluator.GetCostValue(definition, 0),
            Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("level"));
    }

    [Test]
    public void GetCostValue_FirstAndMaxPurchaseLevel_ReturnsProgressionValues()
    {
        var definition = CreateValidDefinition(maxLevel: 5, costProgression: LinearProgression(100f, 1000f));
        var evaluator = new UpgradeDefinitionEvaluator();

        Assert.That(evaluator.GetCostValue(definition, 1), Is.EqualTo(100));
        Assert.That(evaluator.GetCostValue(definition, 5), Is.EqualTo(1000));
    }

    [Test]
    public void Evaluate_NonlinearCurve_UsesNormalizedLevelProjection()
    {
        var curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.5f, 0.25f),
            new Keyframe(1f, 1f));
        var progression = new UpgradeProgression(10f, 110f, curve, UpgradeProgressionRoundingMode.None, 0f);

        var value = progression.Evaluate(3, 1, 5);

        Assert.That(value, Is.EqualTo(35f).Within(0.0001f));
    }

    [Test]
    public void Evaluate_RoundingAndStepRules_AppliesToProjectedValue()
    {
        var curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.5f, 0.26f),
            new Keyframe(1f, 1f));
        var progression = new UpgradeProgression(0f, 10f, curve, UpgradeProgressionRoundingMode.Nearest, 5f);

        var value = progression.Evaluate(3, 1, 5);

        Assert.That(value, Is.EqualTo(5f).Within(0.0001f));
    }

    [Test]
    public void Evaluate_ExactLevelOverride_WinsOverCurveProjection()
    {
        var progression = new UpgradeProgression(
            10f,
            110f,
            AnimationCurve.Linear(0f, 0f, 1f, 1f),
            UpgradeProgressionRoundingMode.None,
            0f,
            new[] { new UpgradeLevelValueOverride(3, 777f) });

        var value = progression.Evaluate(3, 1, 5);

        Assert.That(value, Is.EqualTo(777f).Within(0.0001f));
    }

    [Test]
    public void Validate_NullCatalogPurchaseCurrencyDuplicateIdsAndNullEntry_ReturnsErrors()
    {
        var first = CreateValidDefinition(stableId: "launch-power");
        var second = CreateValidDefinition(stableId: "launch-power");
        var catalog = Track(ScriptableObject.CreateInstance<UpgradeCatalog>());
        catalog.SetValuesForTests(null, new[] { first, null, second });
        var validator = new UpgradeCatalogValidator(new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator()));

        var errors = validator.Validate(catalog);

        Assert.That(ErrorCodes(errors), Does.Contain(UpgradeValidationErrorCode.MissingPurchaseCurrency));
        Assert.That(ErrorCodes(errors), Does.Contain(UpgradeValidationErrorCode.NullUpgradeDefinition));
        Assert.That(ErrorCodes(errors), Does.Contain(UpgradeValidationErrorCode.DuplicateUpgradeId));
    }

    [Test]
    public void Validate_MissingDefinitionFieldsInvalidMaxLevelAndInvalidCurve_ReturnsErrors()
    {
        var definition = CreateValidDefinition(
            stableId: string.Empty,
            missingIcon: true,
            missingStatId: true,
            maxLevel: 0,
            costProgression: new UpgradeProgression(0f, 0f, null, UpgradeProgressionRoundingMode.None, 0f),
            effectProgression: new UpgradeProgression(
                1f,
                2f,
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                UpgradeProgressionRoundingMode.None,
                0f,
                new[] { new UpgradeLevelValueOverride(99, 3f) }));
        var validator = new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator());

        var errors = validator.Validate(definition);

        Assert.That(ErrorCodes(errors), Does.Contain(UpgradeValidationErrorCode.MissingUpgradeId));
        Assert.That(ErrorCodes(errors), Does.Contain(UpgradeValidationErrorCode.MissingIcon));
        Assert.That(ErrorCodes(errors), Does.Contain(UpgradeValidationErrorCode.MissingTargetStatId));
        Assert.That(ErrorCodes(errors), Does.Contain(UpgradeValidationErrorCode.InvalidMaxLevel));
        Assert.That(ErrorCodes(errors), Does.Contain(UpgradeValidationErrorCode.MissingProgressionCurve));
        Assert.That(ErrorCodes(errors), Does.Contain(UpgradeValidationErrorCode.ExactOverrideLevelOutOfRange));
    }

    [Test]
    public void Build_AffordableUpgrade_ReturnsNextCostAndEffectPreview()
    {
        var definition = CreateValidDefinition(
            maxLevel: 3,
            costProgression: LinearProgression(100f, 300f),
            effectProgression: LinearProgression(1f, 1.6f));

        var previewBuilder =
            new UpgradePreviewBuilder(new UpgradeDefinitionEvaluator(), new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator()));

        var preview = previewBuilder.Build(definition, 1, 250);

        Assert.That(preview.State, Is.EqualTo(UpgradePreviewState.Available));
        Assert.That(preview.Definition, Is.SameAs(definition));
        Assert.That(preview.CurrentLevel, Is.EqualTo(1));
        Assert.That(preview.CurrentEffect, Is.EqualTo(1.2f).Within(0.0001f));
        Assert.That(preview.NextEffect, Is.EqualTo(1.4f).Within(0.0001f));
        Assert.That(preview.NextCost, Is.EqualTo(200));
        Assert.That(preview.IsAffordable, Is.True);
        Assert.That(preview.IsMaxed, Is.False);
        Assert.That(preview.IsValid, Is.True);
    }

    [Test]
    public void Build_UnaffordableUpgrade_ReturnsUnaffordablePreview()
    {
        var definition = CreateValidDefinition(maxLevel: 3, costProgression: LinearProgression(100f, 300f));

        var previewBuilder =
            new UpgradePreviewBuilder(new UpgradeDefinitionEvaluator(), new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator()));

        var preview = previewBuilder.Build(definition, 1, 199);

        Assert.That(preview.State, Is.EqualTo(UpgradePreviewState.Unaffordable));
        Assert.That(preview.NextCost, Is.EqualTo(200));
        Assert.That(preview.IsAffordable, Is.False);
        Assert.That(preview.IsMaxed, Is.False);
        Assert.That(preview.IsValid, Is.True);
    }

    [Test]
    public void Build_MaxedUpgrade_ReturnsMaxedPreviewWithoutNextCost()
    {
        var definition = CreateValidDefinition(maxLevel: 3, effectProgression: LinearProgression(1f, 1.6f));

        var previewBuilder =
            new UpgradePreviewBuilder(new UpgradeDefinitionEvaluator(), new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator()));

        var preview = previewBuilder.Build(definition, 3, 999);

        Assert.That(preview.State, Is.EqualTo(UpgradePreviewState.Maxed));
        Assert.That(preview.CurrentEffect, Is.EqualTo(1.6f).Within(0.0001f));
        Assert.That(preview.NextEffect, Is.Null);
        Assert.That(preview.NextCost, Is.Null);
        Assert.That(preview.IsAffordable, Is.False);
        Assert.That(preview.IsMaxed, Is.True);
        Assert.That(preview.IsValid, Is.True);
    }

    [Test]
    public void Build_InvalidDefinition_ReturnsInvalidDefinitionPreview()
    {
        var definition = CreateValidDefinition(missingIcon: true);

        var previewBuilder =
            new UpgradePreviewBuilder(new UpgradeDefinitionEvaluator(), new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator()));

        var preview = previewBuilder.Build(definition, 0, 999);

        Assert.That(preview.State, Is.EqualTo(UpgradePreviewState.InvalidDefinition));
        Assert.That(preview.IsValid, Is.False);
        Assert.That(preview.ValidationErrors, Is.Not.Empty);
    }

    private UpgradeDefinition CreateValidDefinition(
        string stableId = "launch-power",
        Sprite icon = null,
        bool missingIcon = false,
        GameplayStatId statId = null,
        bool missingStatId = false,
        int maxLevel = 5,
        string shortDisplayName = null,
        UpgradeProgression costProgression = null,
        UpgradeProgression effectProgression = null)
    {
        var definition = Track(ScriptableObject.CreateInstance<UpgradeDefinition>());

        definition.SetValuesForTests(
            stableId,
            "Launch Power",
            "Launches harder.",
            missingIcon ? null : icon == null ? _icon : icon,
            missingStatId ? null : statId == null ? _statId : statId,
            maxLevel,
            costProgression ?? LinearProgression(100f, 500f),
            effectProgression ?? LinearProgression(1f, 2f),
            UpgradeOperationType.MultiplicativeFactor,
            UpgradeValueFormat.Multiplier,
            1,
            shortDisplayName);
        return definition;
    }

    private UpgradeProgression LinearProgression(float minimumValue, float maximumValue)
    {
        return new UpgradeProgression(
            minimumValue,
            maximumValue,
            AnimationCurve.Linear(0f, 0f, 1f, 1f),
            UpgradeProgressionRoundingMode.None,
            0f);
    }

    private Sprite CreateIcon()
    {
        var texture = Track(new Texture2D(1, 1));
        return Track(Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f)));
    }

    private IReadOnlyList<UpgradeValidationErrorCode> ErrorCodes(IReadOnlyList<UpgradeValidationError> errors)
    {
        var codes = new List<UpgradeValidationErrorCode>();

        foreach (var error in errors)
        {
            codes.Add(error.Code);
        }

        return codes;
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }
}
