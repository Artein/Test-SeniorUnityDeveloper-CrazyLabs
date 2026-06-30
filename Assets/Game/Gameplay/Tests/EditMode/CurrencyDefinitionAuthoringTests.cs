using System.Collections.Generic;
using Game.Gameplay.Economy;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class CurrencyDefinitionAuthoringTests
{
    private readonly List<UnityEngine.Object> _objects = new();

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
    public void Icon_SetForCurrencyDefinition_ReturnsAssignedSprite()
    {
        var definition = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        var icon = CreateIcon();

        definition.SetIconForTests(icon);

        Assert.That(definition.Icon, Is.SameAs(icon));
    }

    [Test]
    public void EnsureSaveId_BlankDefinition_GeneratesStableNonEmptySaveId()
    {
        var definition = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());

        definition.EnsureSaveIdForTests();
        var firstSaveId = definition.SaveId;
        definition.SetIconForTests(CreateIcon());
        definition.EnsureSaveIdForTests();

        Assert.That(firstSaveId, Is.Not.Empty);
        Assert.That(definition.SaveId, Is.EqualTo(firstSaveId));
    }

    [Test]
    public void Validate_MissingSaveId_ReturnsMissingSaveIdError()
    {
        var definition = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        var validator = new CurrencyDefinitionValidator();

        var errors = validator.Validate(definition);

        Assert.That(ErrorCodes(errors), Does.Contain(CurrencyValidationErrorCode.MissingSaveId));
    }

    [Test]
    public void ValidateAll_DuplicateSaveIds_ReturnsDuplicateSaveIdError()
    {
        var first = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        var second = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        first.SetSaveIdForTests("currency-coins");
        second.SetSaveIdForTests("currency-coins");
        var validator = new CurrencyDefinitionValidator();

        var errors = validator.ValidateAll(new[] { first, second });

        Assert.That(ErrorCodes(errors), Does.Contain(CurrencyValidationErrorCode.DuplicateSaveId));
    }

    private Sprite CreateIcon()
    {
        var texture = Track(new Texture2D(1, 1));
        return Track(Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f)));
    }

    private IReadOnlyList<CurrencyValidationErrorCode> ErrorCodes(IReadOnlyList<CurrencyValidationError> errors)
    {
        var codes = new CurrencyValidationErrorCode[errors.Count];

        for (var i = 0; i < errors.Count; i += 1)
        {
            codes[i] = errors[i].Code;
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
