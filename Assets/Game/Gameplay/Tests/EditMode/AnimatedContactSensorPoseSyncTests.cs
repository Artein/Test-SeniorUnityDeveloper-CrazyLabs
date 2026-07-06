using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.CharacterPresentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class AnimatedContactSensorPoseSyncTests
{
    private readonly List<UnityEngine.Object> _objects = new();

    [SetUp]
    public void OnSetUp()
    {
        Undo.ClearAll();
    }

    [TearDown]
    public void OnTearDown()
    {
        Undo.ClearAll();

        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void CopySourcePosesToTargets_SourcePoseChanged_CopiesPoseToTargetSensorAndReturnsCopiedCount()
    {
        var source = CreateGameObject("Character Left Hand").transform;
        var view = CreateValidView(out _, source, out var target);
        var sync = new AnimatedContactSensorPoseSync(view);

        source.SetPositionAndRotation(new Vector3(1f, 2f, 3f), Quaternion.Euler(10f, 20f, 30f));
        source.localScale = new Vector3(1.2f, 0.8f, 1.1f);

        var copiedCount = sync.CopySourcePosesToTargets();

        Assert.That(copiedCount, Is.EqualTo(1));
        Assert.That(target.position, Is.EqualTo(source.position));
        Assert.That(Quaternion.Angle(target.rotation, source.rotation), Is.EqualTo(0f).Within(0.0001f));
        Assert.That(target.localScale, Is.EqualTo(source.lossyScale));
    }

    [Test]
    public void LateTick_SourcePoseChanged_CopiesPoseToTargetSensor()
    {
        var source = CreateGameObject("Character Left Hand").transform;
        var view = CreateValidView(out _, source, out var target);
        var sync = new AnimatedContactSensorPoseSync(view);

        source.SetPositionAndRotation(new Vector3(1f, 2f, 3f), Quaternion.Euler(10f, 20f, 30f));
        source.localScale = new Vector3(1.2f, 0.8f, 1.1f);

        ((ILateTickable)sync).LateTick();

        Assert.That(target.position, Is.EqualTo(source.position));
        Assert.That(Quaternion.Angle(target.rotation, source.rotation), Is.EqualTo(0f).Within(0.0001f));
        Assert.That(target.localScale, Is.EqualTo(source.lossyScale));
    }

    [Test]
    public void GetReferenceValidationErrors_ValidRootAndBinding_ReturnsNoErrors()
    {
        var source = CreateGameObject("Character Left Hand").transform;
        var view = CreateValidView(out _, source, out _);

        var errors = GetReferenceValidationErrors(view).ToArray();

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void GetReferenceValidationErrors_NonKinematicRoot_ReturnsRigidbodyError()
    {
        var source = CreateGameObject("Character Left Hand").transform;
        var view = CreateValidView(out var rigidbody, source, out _);
        rigidbody.isKinematic = false;

        var errors = GetReferenceValidationErrors(view).ToArray();

        Assert.That(errors.Any(error => error.Contains("kinematic Rigidbody")), Is.True);
    }

    [Test]
    public void GetReferenceValidationErrors_TargetOutsidePhysicsRoot_ReturnsHierarchyError()
    {
        var source = CreateGameObject("Character Left Hand").transform;
        var view = CreateValidView(out _, source, out var target);
        target.SetParent(null, true);

        var errors = GetReferenceValidationErrors(view).ToArray();

        Assert.That(errors.Any(error => error.Contains("inside the Animated Contact Sensor Physics Root")), Is.True);
    }

    [Test]
    public void GetReferenceValidationErrors_DuplicateTarget_ReturnsDuplicateError()
    {
        var source = CreateGameObject("Character Left Hand").transform;
        var view = CreateValidView(out _, source, out var target);

        view.SetReferencesForTests(
            view.RootRigidbody,
            new[]
            {
                new AnimatedContactSensorPoseBinding(source, target),
                new AnimatedContactSensorPoseBinding(source, target)
            });

        var errors = GetReferenceValidationErrors(view).ToArray();

        Assert.That(errors.Any(error => error.Contains("duplicate")), Is.True);
    }

    [Test]
    public void CopySourcePosesToTargets_InvalidBindings_SkipsInvalidBindingsAndReturnsCopiedCount()
    {
        var source = CreateGameObject("Character Left Hand").transform;
        var secondSource = CreateGameObject("Character Right Hand").transform;
        var invalidBindingTarget = CreateGameObject("Invalid Binding Target").transform;
        var view = CreateValidView(out var rootRigidbody, source, out var target);
        var secondTarget = CreateGameObject("Right Hand Sensor").transform;
        var invalidTargetOriginalPosition = new Vector3(20f, 21f, 22f);
        var sync = new AnimatedContactSensorPoseSync(view);

        secondTarget.SetParent(rootRigidbody.transform, false);
        invalidBindingTarget.SetParent(rootRigidbody.transform, false);
        invalidBindingTarget.position = invalidTargetOriginalPosition;
        source.SetPositionAndRotation(new Vector3(1f, 2f, 3f), Quaternion.Euler(10f, 20f, 30f));
        secondSource.SetPositionAndRotation(new Vector3(4f, 5f, 6f), Quaternion.Euler(40f, 50f, 60f));

        view.SetReferencesForTests(
            rootRigidbody,
            new[]
            {
                new AnimatedContactSensorPoseBinding(source, target),
                new AnimatedContactSensorPoseBinding(null, invalidBindingTarget),
                new AnimatedContactSensorPoseBinding(secondSource, null),
                new AnimatedContactSensorPoseBinding(secondSource, secondTarget)
            });

        var copiedCount = sync.CopySourcePosesToTargets();

        Assert.That(copiedCount, Is.EqualTo(2));
        AssertTransformMatches(target, source.position, source.rotation, source.lossyScale);
        AssertTransformMatches(secondTarget, secondSource.position, secondSource.rotation, secondSource.lossyScale);
        Assert.That(invalidBindingTarget.position, Is.EqualTo(invalidTargetOriginalPosition));
    }

    [Test]
    public void SyncSourcePosesToTargetsForEditor_SourcePoseChanged_RecordsUndoForTargetSensor()
    {
        var source = CreateGameObject("Character Head").transform;
        var view = CreateValidView(out _, source, out var target);
        var originalPosition = new Vector3(-1f, -2f, -3f);
        var originalRotation = Quaternion.Euler(1f, 2f, 3f);
        var originalScale = new Vector3(0.5f, 0.6f, 0.7f);

        target.SetPositionAndRotation(originalPosition, originalRotation);
        target.localScale = originalScale;
        source.SetPositionAndRotation(new Vector3(4f, 5f, 6f), Quaternion.Euler(40f, 50f, 60f));
        source.localScale = new Vector3(1.4f, 1.5f, 1.6f);

        Undo.IncrementCurrentGroup();
        var undoGroup = Undo.GetCurrentGroup();
        var copiedCount = view.SyncSourcePosesToTargetsForEditor();
        Undo.CollapseUndoOperations(undoGroup);

        Assert.That(copiedCount, Is.EqualTo(1));
        AssertTransformMatches(target, source.position, source.rotation, source.lossyScale);

        Undo.PerformUndo();

        AssertTransformMatches(target, originalPosition, originalRotation, originalScale);
    }

    [Test]
    public void SyncSourcePosesToTargetsForEditor_InvalidBindings_SkipsInvalidBindingsAndReturnsCopiedCount()
    {
        var source = CreateGameObject("Character Left Hand").transform;
        var secondSource = CreateGameObject("Character Right Hand").transform;
        var invalidBindingTarget = CreateGameObject("Invalid Binding Target").transform;
        var view = CreateValidView(out var rootRigidbody, source, out var target);
        var secondTarget = CreateGameObject("Right Hand Sensor").transform;
        var invalidTargetOriginalPosition = new Vector3(20f, 21f, 22f);

        secondTarget.SetParent(rootRigidbody.transform, false);
        invalidBindingTarget.SetParent(rootRigidbody.transform, false);
        invalidBindingTarget.position = invalidTargetOriginalPosition;
        source.SetPositionAndRotation(new Vector3(1f, 2f, 3f), Quaternion.Euler(10f, 20f, 30f));
        secondSource.SetPositionAndRotation(new Vector3(4f, 5f, 6f), Quaternion.Euler(40f, 50f, 60f));

        view.SetReferencesForTests(
            rootRigidbody,
            new[]
            {
                new AnimatedContactSensorPoseBinding(source, target),
                new AnimatedContactSensorPoseBinding(null, invalidBindingTarget),
                new AnimatedContactSensorPoseBinding(secondSource, null),
                new AnimatedContactSensorPoseBinding(secondSource, secondTarget)
            });

        var copiedCount = view.SyncSourcePosesToTargetsForEditor();

        Assert.That(copiedCount, Is.EqualTo(2));
        AssertTransformMatches(target, source.position, source.rotation, source.lossyScale);
        AssertTransformMatches(secondTarget, secondSource.position, secondSource.rotation, secondSource.lossyScale);
        Assert.That(invalidBindingTarget.position, Is.EqualTo(invalidTargetOriginalPosition));
    }

    private AnimatedContactSensorPoseSyncView CreateValidView(
        out Rigidbody rootRigidbody,
        Transform source,
        out Transform target)
    {
        var root = CreateGameObject("Animated Contact Sensor Physics Root");
        rootRigidbody = root.AddComponent<Rigidbody>();
        rootRigidbody.isKinematic = true;
        rootRigidbody.useGravity = false;

        target = CreateGameObject("Left Hand Sensor").transform;
        target.SetParent(root.transform, false);

        var view = root.AddComponent<AnimatedContactSensorPoseSyncView>();
        view.SetReferencesForTests(rootRigidbody, new[] { new AnimatedContactSensorPoseBinding(source, target) });
        return view;
    }

    private GameObject CreateGameObject(string objectName)
    {
        return Track(new GameObject(objectName));
    }

    private IEnumerable<string> GetReferenceValidationErrors(AnimatedContactSensorPoseSyncView view)
    {
        var validator = new AnimatedContactSensorPoseSyncReferenceValidator();
        return validator.GetReferenceValidationErrors(view.RootRigidbody, view.Bindings);
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private void AssertTransformMatches(
        Transform transform,
        Vector3 expectedPosition,
        Quaternion expectedRotation,
        Vector3 expectedScale)
    {
        Assert.That(transform.position, Is.EqualTo(expectedPosition));
        Assert.That(Quaternion.Angle(transform.rotation, expectedRotation), Is.EqualTo(0f).Within(0.0001f));
        Assert.That(transform.localScale, Is.EqualTo(expectedScale));
    }
}
