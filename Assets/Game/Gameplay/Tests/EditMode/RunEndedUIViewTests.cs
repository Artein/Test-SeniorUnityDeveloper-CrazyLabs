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
            earnedCoinsText: "COINS\n13",
            reachedMeters: 87,
            reachedDistanceText: "DISTANCE\n87 m",
            hasBestImprovement: true,
            bestImprovementMeters: 87,
            bestImprovementText: "NEW BEST\n+87 m"));

        Assert.That(fixture.Root.activeSelf, Is.True);
        Assert.That(fixture.TitleText.text, Is.EqualTo("VICTORY"));
        Assert.That(fixture.EarnedCoinsText.text, Is.EqualTo("COINS\n13"));
        Assert.That(fixture.ReachedDistanceText.text, Is.EqualTo("DISTANCE\n87 m"));
        Assert.That(fixture.BestImprovementRoot.activeSelf, Is.True);
        Assert.That(fixture.BestImprovementText.text, Is.EqualTo("NEW BEST\n+87 m"));
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
            earnedCoinsText: "COINS\n2",
            reachedMeters: 12,
            reachedDistanceText: "DISTANCE\n12 m",
            hasBestImprovement: false,
            bestImprovementMeters: 0,
            bestImprovementText: string.Empty));

        Assert.That(fixture.Root.activeSelf, Is.True);
        Assert.That(fixture.TitleText.text, Is.EqualTo("DEFEAT"));
        Assert.That(fixture.BestImprovementRoot.activeSelf, Is.False);
        Assert.That(fixture.BestImprovementText.text, Is.Empty);
    }

    private Fixture CreateFixture()
    {
        var root = CreateGameObject("Run Ended Panel");
        var view = root.AddComponent<RunEndedUIView>();
        var titleText = CreateChildText(root.transform, "Run Ended Title");
        var earnedCoinsText = CreateChildText(root.transform, "Run Ended Earned Coins Label");
        var reachedDistanceText = CreateChildText(root.transform, "Run Ended Reached Distance Label");
        var bestImprovementRoot = CreateChildGameObject(root.transform, "Run Ended Best Improvement Label");
        var bestImprovementText = CreateChildText(bestImprovementRoot.transform, "Run Ended Best Improvement Value");
        var button = root.AddComponent<Button>();

        view.SetReferencesForTests(
            root,
            titleText,
            earnedCoinsText,
            reachedDistanceText,
            bestImprovementRoot,
            bestImprovementText,
            button);

        return new Fixture(
            root,
            titleText,
            earnedCoinsText,
            reachedDistanceText,
            bestImprovementRoot,
            bestImprovementText,
            view);
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

    private readonly struct Fixture
    {
        public GameObject Root { get; }
        public TMP_Text TitleText { get; }
        public TMP_Text EarnedCoinsText { get; }
        public TMP_Text ReachedDistanceText { get; }
        public GameObject BestImprovementRoot { get; }
        public TMP_Text BestImprovementText { get; }
        public RunEndedUIView View { get; }

        public Fixture(
            GameObject root,
            TMP_Text titleText,
            TMP_Text earnedCoinsText,
            TMP_Text reachedDistanceText,
            GameObject bestImprovementRoot,
            TMP_Text bestImprovementText,
            RunEndedUIView view)
        {
            Root = root;
            TitleText = titleText;
            EarnedCoinsText = earnedCoinsText;
            ReachedDistanceText = reachedDistanceText;
            BestImprovementRoot = bestImprovementRoot;
            BestImprovementText = bestImprovementText;
            View = view;
        }
    }
}
