using System.Collections.Generic;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
public sealed class RunSteeringAffordanceViewTests
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
    public void Show_VisibleState_AppliesLayoutAndNonInteractiveSettings()
    {
        var view = CreateView(
            showSeconds: 0f,
            hideSeconds: 0f,
            out var root,
            out var canvasGroup,
            out var knobRoot,
            out var knobImage,
            out var leftRangeEndRoot,
            out var leftRangeEndImage,
            out var rightRangeEndRoot,
            out var rightRangeEndImage,
            out var deadzoneRoot,
            out var deadzoneImage);

        ((IRunSteeringAffordanceView)view).Show(CreateState(
            knob: new Vector2(150f, 200f),
            leftRangeEnd: new Vector2(20f, 200f),
            rightRangeEnd: new Vector2(180f, 200f),
            deadzoneDiameter: 40f));

        Assert.That(root.gameObject.activeSelf, Is.True);
        Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
        Assert.That(canvasGroup.interactable, Is.False);
        Assert.That(canvasGroup.blocksRaycasts, Is.False);
        Assert.That(knobImage.raycastTarget, Is.False);
        Assert.That(leftRangeEndImage.raycastTarget, Is.False);
        Assert.That(rightRangeEndImage.raycastTarget, Is.False);
        Assert.That(deadzoneImage.raycastTarget, Is.False);
        AssertVector2(knobRoot.anchoredPosition, new Vector2(150f, 200f));
        AssertVector2(leftRangeEndRoot.anchoredPosition, new Vector2(20f, 200f));
        AssertVector2(rightRangeEndRoot.anchoredPosition, new Vector2(180f, 200f));
        AssertVector2(deadzoneRoot.anchoredPosition, new Vector2(100f, 200f));
        AssertVector2(deadzoneRoot.sizeDelta, new Vector2(40f, 40f));
        Assert.That(knobRoot.localScale, Is.EqualTo(Vector3.one));
    }

    [Test]
    public void Update_VisibleState_MovesKnobImmediatelyWithoutChangingRangeEndpointIntent()
    {
        var view = CreateView(
            showSeconds: 0f,
            hideSeconds: 0f,
            out _,
            out _,
            out var knobRoot,
            out _,
            out var leftRangeEndRoot,
            out _,
            out var rightRangeEndRoot,
            out _,
            out _,
            out _);

        ((IRunSteeringAffordanceView)view).Show(CreateState(
            knob: new Vector2(100f, 200f),
            leftRangeEnd: new Vector2(20f, 200f),
            rightRangeEnd: new Vector2(180f, 200f),
            deadzoneDiameter: 40f));

        ((IRunSteeringAffordanceView)view).Update(CreateState(
            knob: new Vector2(180f, 200f),
            leftRangeEnd: new Vector2(20f, 200f),
            rightRangeEnd: new Vector2(180f, 200f),
            deadzoneDiameter: 40f));

        AssertVector2(knobRoot.anchoredPosition, new Vector2(180f, 200f));
        AssertVector2(leftRangeEndRoot.anchoredPosition, new Vector2(20f, 200f));
        AssertVector2(rightRangeEndRoot.anchoredPosition, new Vector2(180f, 200f));
    }

    [Test]
    public void Hide_VisibleState_HidesFromFinalKnobPositionWithoutReturningToOrigin()
    {
        var view = CreateView(
            showSeconds: 0f,
            hideSeconds: 0f,
            out var root,
            out var canvasGroup,
            out var knobRoot,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);

        ((IRunSteeringAffordanceView)view).Show(CreateState(
            knob: new Vector2(100f, 200f),
            leftRangeEnd: new Vector2(20f, 200f),
            rightRangeEnd: new Vector2(180f, 200f),
            deadzoneDiameter: 40f));

        ((IRunSteeringAffordanceView)view).Hide(CreateState(
            knob: new Vector2(180f, 200f),
            leftRangeEnd: new Vector2(20f, 200f),
            rightRangeEnd: new Vector2(180f, 200f),
            deadzoneDiameter: 40f));

        Assert.That(root.gameObject.activeSelf, Is.False);
        Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.001f));
        AssertVector2(knobRoot.anchoredPosition, new Vector2(180f, 200f));
    }

    [Test]
    public void Show_WithNonzeroAnimation_AppliesLayoutBeforeAnimationCompletes()
    {
        var view = CreateView(
            showSeconds: 1f,
            hideSeconds: 0f,
            out var root,
            out var canvasGroup,
            out var knobRoot,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);

        ((IRunSteeringAffordanceView)view).Show(CreateState(
            knob: new Vector2(150f, 200f),
            leftRangeEnd: new Vector2(20f, 200f),
            rightRangeEnd: new Vector2(180f, 200f),
            deadzoneDiameter: 40f));

        Assert.That(root.gameObject.activeSelf, Is.True);
        Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
        AssertVector2(knobRoot.anchoredPosition, new Vector2(150f, 200f));

        view.TickAnimationForTests(1f);

        Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
        AssertVector2(knobRoot.anchoredPosition, new Vector2(150f, 200f));
    }

    [Test]
    public void GetReferenceValidationErrorsForTests_MissingReferences_ReportsSetupIssues()
    {
        var view = Track(new GameObject("Broken Run Steering Affordance")).AddComponent<RunSteeringAffordanceView>();

        var errors = view.GetReferenceValidationErrorsForTests();

        Assert.That(errors, Has.Some.Contains("Root RectTransform"));
        Assert.That(errors, Has.Some.Contains("CanvasGroup"));
        Assert.That(errors, Has.Some.Contains("Knob"));
        Assert.That(errors, Has.Some.Contains("Range End"));
        Assert.That(errors, Has.Some.Contains("Deadzone"));
    }

    [Test]
    public void Reset_DestroyedUnityObject_DoesNotThrow()
    {
        var view = CreateView(
            showSeconds: 0f,
            hideSeconds: 0f,
            out var root,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);

        UnityEngine.Object.DestroyImmediate(root.gameObject);

        Assert.That(() => view.Reset(), Throws.Nothing);
    }

    private RunSteeringAffordanceView CreateView(
        float showSeconds,
        float hideSeconds,
        out RectTransform root,
        out CanvasGroup canvasGroup,
        out RectTransform knobRoot,
        out Image knobImage,
        out RectTransform leftRangeEndRoot,
        out Image leftRangeEndImage,
        out RectTransform rightRangeEndRoot,
        out Image rightRangeEndImage,
        out RectTransform deadzoneRoot,
        out Image deadzoneImage)
    {
        var rootObject = Track(new GameObject("Run Steering Affordance", typeof(RectTransform), typeof(CanvasGroup)));
        root = rootObject.GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.zero;
        root.pivot = Vector2.zero;
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = new Vector2(1000f, 1000f);
        canvasGroup = rootObject.GetComponent<CanvasGroup>();

        deadzoneRoot = CreateImageChild(root, "Deadzone Hint", out deadzoneImage);
        leftRangeEndRoot = CreateImageChild(root, "Left Range End Hint", out leftRangeEndImage);
        rightRangeEndRoot = CreateImageChild(root, "Right Range End Hint", out rightRangeEndImage);
        knobRoot = CreateImageChild(root, "Knob", out knobImage);

        var view = rootObject.AddComponent<RunSteeringAffordanceView>();
        view.SetReferencesForTests(
            root,
            canvasGroup,
            knobRoot,
            knobImage,
            leftRangeEndRoot,
            leftRangeEndImage,
            rightRangeEndRoot,
            rightRangeEndImage,
            deadzoneRoot,
            deadzoneImage);
        view.SetAnimationDurationsForTests(showSeconds, hideSeconds);
        rootObject.SetActive(false);
        return view;
    }

    private RectTransform CreateImageChild(Transform parent, string objectName, out Image image)
    {
        var childObject = Track(new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)));
        childObject.transform.SetParent(parent, false);
        var rectTransform = childObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        image = childObject.GetComponent<Image>();
        return rectTransform;
    }

    private static RunSteeringAffordancePresentationState CreateState(
        Vector2 knob,
        Vector2 leftRangeEnd,
        Vector2 rightRangeEnd,
        float deadzoneDiameter)
    {
        return new RunSteeringAffordancePresentationState(
            true,
            new Vector2(100f, 200f),
            knob,
            leftRangeEnd,
            rightRangeEnd,
            deadzoneDiameter);
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private static void AssertVector2(Vector2 actual, Vector2 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
    }
}
