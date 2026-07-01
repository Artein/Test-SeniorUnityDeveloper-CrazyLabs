using System.Collections.Generic;
using Game.Gameplay;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
public sealed class RunEndedUIViewTests
{
    private readonly List<Object> _objects = new();

    [TearDown]
    public void OnTearDown()
    {
        foreach (var unityObject in _objects)
        {
            Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void Render_SuccessState_ShowsSuccessTitleAndBindsStats()
    {
        var fixture = CreateFixture();

        fixture.View.Apply(new RunEndedViewState(
            isVisible: true,
            isSuccess: true,
            titleText: "VICTORY",
            earnedCoins: 13,
            earnedCoinsText: "RUN TOTAL\n13",
            reachedMeters: 87,
            reachedDistanceText: "DISTANCE\n87 m",
            hasBestImprovement: true,
            bestImprovementMeters: 87,
            bestImprovementText: "NEW BEST\n+87 m",
            rewardSourceRows: new[]
            {
                new RunEndedRewardSourceRowViewState("Picked-Up Coins", 5, "5"),
                new RunEndedRewardSourceRowViewState("Distance Bonus", 8, "8")
            }));

        Assert.That(fixture.Root.activeSelf, Is.True);
        Assert.That(fixture.TitleText.text, Is.EqualTo("VICTORY"));
        Assert.That(fixture.EarnedCoinsText.text, Is.EqualTo("RUN TOTAL\n13"));
        Assert.That(fixture.ReachedDistanceText.text, Is.EqualTo("DISTANCE\n87 m"));
        Assert.That(fixture.View.RewardSourceRowsForTests, Has.Count.EqualTo(2));
        Assert.That(fixture.View.RewardSourceRowsForTests[0].LabelTextForTests.text, Is.EqualTo("Picked-Up Coins"));
        Assert.That(fixture.View.RewardSourceRowsForTests[0].AmountTextForTests.text, Is.EqualTo("5"));
        AssertRevealGraphicsAlpha(fixture.View.RewardSourceRowsForTests[0].RevealGraphicsForTests, 1f);
        Assert.That(fixture.View.RewardSourceRowsForTests[1].LabelTextForTests.text, Is.EqualTo("Distance Bonus"));
        Assert.That(fixture.View.RewardSourceRowsForTests[1].AmountTextForTests.text, Is.EqualTo("8"));
        AssertRevealGraphicsAlpha(fixture.View.RewardSourceRowsForTests[1].RevealGraphicsForTests, 1f);
        AssertRevealGraphicsAlpha(fixture.View.EarnedCoinsRevealGraphicsForTests, 1f);
        Assert.That(fixture.BestImprovementRoot.activeSelf, Is.True);
        Assert.That(fixture.BestImprovementText.text, Is.EqualTo("NEW BEST\n+87 m"));
        Assert.That(fixture.TapToContinueRoot.activeSelf, Is.True);
        Assert.That(fixture.View.CanAcknowledgeForTests, Is.True);
    }

    [Test]
    public void Render_FailureWithoutBestImprovement_ShowsFailureTitleAndHidesBestImprovement()
    {
        var fixture = CreateFixture();

        fixture.View.Apply(new RunEndedViewState(
            isVisible: true,
            isSuccess: false,
            titleText: "DEFEAT",
            earnedCoins: 2,
            earnedCoinsText: "RUN TOTAL\n2",
            reachedMeters: 12,
            reachedDistanceText: "DISTANCE\n12 m",
            hasBestImprovement: false,
            bestImprovementMeters: 0,
            bestImprovementText: string.Empty));

        Assert.That(fixture.Root.activeSelf, Is.True);
        Assert.That(fixture.TitleText.text, Is.EqualTo("DEFEAT"));
        Assert.That(fixture.BestImprovementRoot.activeSelf, Is.False);
        Assert.That(fixture.BestImprovementText.text, Is.Empty);
        Assert.That(fixture.View.RewardSourceRowsForTests, Is.Empty);
    }

    private Fixture CreateFixture()
    {
        var root = CreateGameObject("RunEndedPanel");
        var view = root.AddComponent<RunEndedUIView>();
        var titleText = CreateChildText(root.transform, "Run Ended Title");
        var earnedCoinsIcon = CreateChildImage(root.transform, "Icon");
        var earnedCoinsText = CreateChildText(root.transform, "RunTotalLabel");
        var reachedDistanceText = CreateChildText(root.transform, "ReachedDistanceLabel");
        var rewardSourceRowsRoot = CreateChildGameObject(root.transform, "RewardSourceContainer");
        var rewardSourceRowPrefab = CreateRewardSourceRowTemplate(rewardSourceRowsRoot.transform);
        var bestImprovementRoot = CreateChildGameObject(root.transform, "BestImprovementRoot");
        var bestImprovementText = CreateChildText(bestImprovementRoot.transform, "BestImprovementLabel");
        var tapToContinueRoot = CreateChildGameObject(root.transform, "Run Ended Continue Label");
        var button = root.AddComponent<Button>();

        view.SetReferencesForTests(
            root,
            titleText,
            earnedCoinsText,
            new Graphic[] { earnedCoinsIcon, earnedCoinsText },
            reachedDistanceText,
            rewardSourceRowsRoot.transform,
            rewardSourceRowPrefab,
            bestImprovementRoot,
            bestImprovementText,
            tapToContinueRoot,
            button);

        return new Fixture(
            root,
            titleText,
            earnedCoinsText,
            reachedDistanceText,
            bestImprovementRoot,
            bestImprovementText,
            tapToContinueRoot,
            view);
    }

    private RunEndedRewardSourceRowUIView CreateRewardSourceRowTemplate(Transform parent)
    {
        var rowObject = CreateChildGameObject(parent, "RowTemplate");
        var row = rowObject.AddComponent<RunEndedRewardSourceRowUIView>();
        var labelText = CreateChildText(rowObject.transform, "Label");
        var currencyIcon = CreateChildImage(rowObject.transform, "CurrencyIcon");
        var amountText = CreateChildText(rowObject.transform, "Amount");
        amountText.color = Color.white;
        row.SetReferencesForTests(labelText, amountText, labelText, currencyIcon, amountText);
        rowObject.SetActive(false);
        return row;
    }

    private GameObject CreateGameObject(string name)
    {
        var gameObject = new GameObject(name);
        _objects.Add(gameObject);
        return gameObject;
    }

    private GameObject CreateChildGameObject(Transform parent, string name)
    {
        var child = CreateGameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private TMP_Text CreateChildText(Transform parent, string name)
    {
        var child = CreateChildGameObject(parent, name);
        return child.AddComponent<TextMeshProUGUI>();
    }

    private Image CreateChildImage(Transform parent, string name)
    {
        var child = CreateChildGameObject(parent, name);
        return child.AddComponent<Image>();
    }

    private static void AssertRevealGraphicsAlpha(IReadOnlyList<Graphic> graphics, float expectedAlpha)
    {
        Assert.That(graphics, Is.Not.Empty);

        foreach (var graphic in graphics)
        {
            Assert.That(graphic.color.a, Is.EqualTo(expectedAlpha).Within(0.001f));
        }
    }

    private readonly struct Fixture
    {
        public GameObject Root { get; }
        public TMP_Text TitleText { get; }
        public TMP_Text EarnedCoinsText { get; }
        public TMP_Text ReachedDistanceText { get; }
        public GameObject BestImprovementRoot { get; }
        public TMP_Text BestImprovementText { get; }
        public GameObject TapToContinueRoot { get; }
        public RunEndedUIView View { get; }

        public Fixture(
            GameObject root,
            TMP_Text titleText,
            TMP_Text earnedCoinsText,
            TMP_Text reachedDistanceText,
            GameObject bestImprovementRoot,
            TMP_Text bestImprovementText,
            GameObject tapToContinueRoot,
            RunEndedUIView view)
        {
            Root = root;
            TitleText = titleText;
            EarnedCoinsText = earnedCoinsText;
            ReachedDistanceText = reachedDistanceText;
            BestImprovementRoot = bestImprovementRoot;
            BestImprovementText = bestImprovementText;
            TapToContinueRoot = tapToContinueRoot;
            View = view;
        }
    }
}
