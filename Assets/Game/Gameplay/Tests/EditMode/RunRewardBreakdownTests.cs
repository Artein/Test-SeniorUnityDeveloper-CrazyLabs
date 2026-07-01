using System;
using System.Linq;
using Game.Gameplay.Economy;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunRewardBreakdownTests
{
    private CurrencyDefinition _coins;
    private CurrencyDefinition _gems;

    [SetUp]
    public void OnSetUp()
    {
        _coins = ScriptableObject.CreateInstance<CurrencyDefinition>();
        _coins.name = "Coins";
        _coins.SetSaveIdForTests("currency-coins");

        _gems = ScriptableObject.CreateInstance<CurrencyDefinition>();
        _gems.name = "Gems";
        _gems.SetSaveIdForTests("currency-gems");
    }

    [TearDown]
    public void OnTearDown()
    {
        UnityEngine.Object.DestroyImmediate(_coins);
        UnityEngine.Object.DestroyImmediate(_gems);
    }

    [Test]
    public void Constructor_SourceAmounts_AggregatesBySourceAndCurrencyAndDerivesSnapshot()
    {
        var pickedUpCoins = new RunRewardSource("picked-up-coins", "Picked-Up Coins", 10, showWhenZero: false);
        var distanceBonus = new RunRewardSource("distance-bonus", "Distance Bonus", 20, showWhenZero: false);

        var breakdown = new RunRewardBreakdown(new[]
        {
            new RunRewardSourceAmount(distanceBonus, _coins, 4),
            new RunRewardSourceAmount(pickedUpCoins, _coins, 3),
            new RunRewardSourceAmount(pickedUpCoins, _coins, 5),
            new RunRewardSourceAmount(pickedUpCoins, _gems, 2)
        });

        var sourceAmounts = breakdown.SourceAmounts.ToArray();
        Assert.That(sourceAmounts, Has.Length.EqualTo(3));
        Assert.That(sourceAmounts[0].Source.StableId, Is.EqualTo("picked-up-coins"));
        Assert.That(sourceAmounts[0].CurrencyDefinition, Is.SameAs(_coins));
        Assert.That(sourceAmounts[0].Amount, Is.EqualTo(8));
        Assert.That(sourceAmounts[1].Source.StableId, Is.EqualTo("picked-up-coins"));
        Assert.That(sourceAmounts[1].CurrencyDefinition, Is.SameAs(_gems));
        Assert.That(sourceAmounts[1].Amount, Is.EqualTo(2));
        Assert.That(sourceAmounts[2].Source.StableId, Is.EqualTo("distance-bonus"));
        Assert.That(sourceAmounts[2].Amount, Is.EqualTo(4));
        Assert.That(breakdown.CurrencySnapshot.GetAmount(_coins), Is.EqualTo(12));
        Assert.That(breakdown.CurrencySnapshot.GetAmount(_gems), Is.EqualTo(2));
    }

    [Test]
    public void Constructor_ZeroAmountWithoutOptIn_HidesSourceRowAndExcludesSnapshot()
    {
        var hiddenZero = new RunRewardSource("distance-bonus", "Distance Bonus", 20, showWhenZero: false);
        var visibleZero = new RunRewardSource("air-time-bonus", "Air Time Bonus", 30, showWhenZero: true);

        var breakdown = new RunRewardBreakdown(new[]
        {
            new RunRewardSourceAmount(hiddenZero, _coins, 0),
            new RunRewardSourceAmount(visibleZero, _coins, 0)
        });

        var sourceAmounts = breakdown.SourceAmounts.ToArray();
        Assert.That(sourceAmounts, Has.Length.EqualTo(1));
        Assert.That(sourceAmounts[0].Source.StableId, Is.EqualTo("air-time-bonus"));
        Assert.That(sourceAmounts[0].Amount, Is.Zero);
        Assert.That(breakdown.CurrencySnapshot.Amounts, Is.Empty);
    }

    [Test]
    public void Constructor_DuplicateSourceWithDifferentLabels_Throws()
    {
        var first = new RunRewardSource("picked-up-coins", "Picked-Up Coins", 10, showWhenZero: false);
        var second = new RunRewardSource("picked-up-coins", "Other Label", 10, showWhenZero: false);

        Assert.That(
            () => new RunRewardBreakdown(new[]
            {
                new RunRewardSourceAmount(first, _coins, 1),
                new RunRewardSourceAmount(second, _coins, 1)
            }),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("same Stable Id"));
    }
}
