using Game.Foundation.Presentation;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class SpinnerTests
{
    private readonly System.Collections.Generic.List<Object> _objects = new();

    [TearDown]
    public void OnTearDown()
    {
        foreach (var trackedObject in _objects)
        {
            Object.DestroyImmediate(trackedObject);
        }

        _objects.Clear();
    }

    [Test]
    public void TickForTests_AssignedTarget_RotatesTargetOnly()
    {
        var root = CreateGameObject("Spinner Root");
        var target = CreateGameObject("Spinner Target");
        target.transform.SetParent(root.transform, false);
        var spinner = root.AddComponent<Spinner>();
        spinner.SetValuesForTests(target.transform, Vector3.up, 90f, false, 0f);
        spinner.InitializeForTests();

        spinner.TickForTests(0.5f);

        Assert.That(Quaternion.Angle(target.transform.localRotation, Quaternion.Euler(0f, 45f, 0f)), Is.LessThan(0.01f));
        Assert.That(Quaternion.Angle(root.transform.localRotation, Quaternion.identity), Is.LessThan(0.01f));
    }

    [Test]
    public void TickForTests_NoAssignedTarget_RotatesOwnTransform()
    {
        var root = CreateGameObject("Spinner Root");
        var spinner = root.AddComponent<Spinner>();
        spinner.SetValuesForTests(null, Vector3.forward, 120f, false, 0f);
        spinner.InitializeForTests();

        spinner.TickForTests(0.25f);

        Assert.That(Quaternion.Angle(root.transform.localRotation, Quaternion.Euler(0f, 0f, 30f)), Is.LessThan(0.01f));
    }

    [Test]
    public void InitializeForTests_DeterministicPhase_StableForSameAuthoredInputs()
    {
        var first = CreateConfiguredSpinner("Scene Coin", new Vector3(2f, 3f, 4f), true, 15f);
        var second = CreateConfiguredSpinner("Scene Coin", new Vector3(2f, 3f, 4f), true, 15f);

        Assert.That(first.InitialPhaseDegreesForTests, Is.EqualTo(second.InitialPhaseDegreesForTests).Within(0.001f));
    }

    [Test]
    public void InitializeForTests_DeterministicPhase_VariesByAuthoredInstanceTransform()
    {
        var first = CreateConfiguredSpinner("Scene Coin", new Vector3(1f, 0f, 0f), true, 0f);
        var second = CreateConfiguredSpinner("Scene Coin", new Vector3(4f, 0f, 0f), true, 0f);

        Assert.That(first.InitialPhaseDegreesForTests, Is.Not.EqualTo(second.InitialPhaseDegreesForTests).Within(0.001f));
    }

    [Test]
    public void InitializeForTests_AuthoredPhaseOffset_AppliesWithoutDeterministicPhase()
    {
        var spinner = CreateConfiguredSpinner("Coin", Vector3.zero, false, 75f);

        Assert.That(spinner.InitialPhaseDegreesForTests, Is.EqualTo(75f).Within(0.001f));
        Assert.That(Quaternion.Angle(spinner.transform.localRotation, Quaternion.Euler(0f, 75f, 0f)), Is.LessThan(0.01f));
    }

    [Test]
    public void InitializeForTests_ReinitializedAfterRuntimeRotation_DoesNotAccumulateInitialPhase()
    {
        var spinner = CreateConfiguredSpinner("Coin", Vector3.zero, false, 45f);

        spinner.TickForTests(1f);
        spinner.InitializeForTests();

        Assert.That(Quaternion.Angle(spinner.transform.localRotation, Quaternion.Euler(0f, 45f, 0f)), Is.LessThan(0.01f));
    }

    private Spinner CreateConfiguredSpinner(
        string objectName,
        Vector3 position,
        bool useDeterministicPhase,
        float authoredPhaseOffsetDegrees)
    {
        var gameObject = CreateGameObject(objectName);
        gameObject.transform.position = position;
        var spinner = gameObject.AddComponent<Spinner>();
        spinner.SetValuesForTests(null, Vector3.up, 0f, useDeterministicPhase, authoredPhaseOffsetDegrees);
        spinner.InitializeForTests();
        return spinner;
    }

    private GameObject CreateGameObject(string objectName)
    {
        var gameObject = new GameObject(objectName);
        _objects.Add(gameObject);
        return gameObject;
    }
}
