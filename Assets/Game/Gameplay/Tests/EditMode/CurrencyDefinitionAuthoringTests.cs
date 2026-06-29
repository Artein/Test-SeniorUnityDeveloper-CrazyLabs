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

    private Sprite CreateIcon()
    {
        var texture = Track(new Texture2D(1, 1));
        return Track(Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f)));
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }
}
