using System;
using System.IO;
using Game.Gameplay.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class CurrencyDefinitionSaveIdEditorTests
{
    private string _assetDirectory;

    [SetUp]
    public void OnSetUp()
    {
        _assetDirectory = "Assets/__CurrencyDefinitionSaveIdEditorTests_" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(_assetDirectory);
        AssetDatabase.Refresh();
    }

    [TearDown]
    public void OnTearDown()
    {
        if (!string.IsNullOrWhiteSpace(_assetDirectory))
            AssetDatabase.DeleteAsset(_assetDirectory);

        AssetDatabase.Refresh();
    }

    [Test]
    public void CreateAsset_BlankAssetBackedDefinition_SetsGuidDerivedSaveId()
    {
        var definition = CreateCurrencyAsset("BlankCurrencyDefinition");

        Assert.That(definition.SaveId, Is.EqualTo(GetExpectedSaveId(definition)));
    }

    [Test]
    public void GetSaveIdStatusForEditor_MismatchedAssetBackedDefinition_ReportsCurrentAndExpectedIds()
    {
        var definition = CreateCurrencyAsset("MismatchedCurrencyDefinition");
        definition.SetSaveIdForTests("currency-copied");

        var status = definition.GetSaveIdStatusForEditor();

        Assert.That(status.State, Is.EqualTo(CurrencyDefinitionSaveIdState.Mismatched));
        Assert.That(status.AssetPath, Is.EqualTo(AssetDatabase.GetAssetPath(definition)));
        Assert.That(status.AssetGuid, Is.EqualTo(GetAssetGuid(definition)));
        Assert.That(status.CurrentSaveId, Is.EqualTo("currency-copied"));
        Assert.That(status.ExpectedSaveId, Is.EqualTo(GetExpectedSaveId(definition)));
    }

    [Test]
    public void TrySetSaveIdFromAssetGuidForEditor_MismatchedAssetBackedDefinition_SetsGuidDerivedSaveId()
    {
        var definition = CreateCurrencyAsset("FixableCurrencyDefinition");
        definition.SetSaveIdForTests("currency-copied");

        var changed = definition.TrySetSaveIdFromAssetGuidForEditor("Set Currency Save ID From Asset GUID");

        Assert.That(changed, Is.True);
        Assert.That(definition.SaveId, Is.EqualTo(GetExpectedSaveId(definition)));
    }

    [Test]
    public void GetSaveIdStatusForEditor_MatchingAssetBackedDefinition_ReportsValid()
    {
        var definition = CreateCurrencyAsset("MatchingCurrencyDefinition");
        definition.SetSaveIdForTests(GetExpectedSaveId(definition));

        var status = definition.GetSaveIdStatusForEditor();

        Assert.That(status.State, Is.EqualTo(CurrencyDefinitionSaveIdState.Valid));
        Assert.That(status.CurrentSaveId, Is.EqualTo(GetExpectedSaveId(definition)));
        Assert.That(status.ExpectedSaveId, Is.EqualTo(GetExpectedSaveId(definition)));
    }

    private CurrencyDefinition CreateCurrencyAsset(string assetName)
    {
        var definition = ScriptableObject.CreateInstance<CurrencyDefinition>();
        var assetPath = Path.Combine(_assetDirectory, assetName + ".asset").Replace('\\', '/');
        AssetDatabase.CreateAsset(definition, assetPath);
        AssetDatabase.Refresh();

        return (CurrencyDefinition)AssetDatabase.LoadAssetAtPath(assetPath, typeof(CurrencyDefinition));
    }

    private string GetExpectedSaveId(CurrencyDefinition definition)
    {
        return "currency-" + GetAssetGuid(definition);
    }

    private string GetAssetGuid(CurrencyDefinition definition)
    {
        return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(definition));
    }
}
