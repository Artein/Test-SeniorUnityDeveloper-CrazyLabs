using Game.Gameplay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunCameraConfigTests
{
    private RunCameraConfig _config;

    [SetUp]
    public void OnSetUp()
    {
        _config = ScriptableObject.CreateInstance<RunCameraConfig>();
    }

    [TearDown]
    public void OnTearDown()
    {
        Object.DestroyImmediate(_config);
    }

    [Test]
    public void DefaultValues_ExposeUsableAnchorAndPrioritySettings()
    {
        Assert.That(_config.AnchorOffset, Is.EqualTo(new Vector3(0f, 1.2f, 0f)));
        Assert.That(_config.PositionResponseRate, Is.GreaterThan(0f));
        Assert.That(_config.YawResponseRate, Is.GreaterThan(0f));
        Assert.That(_config.MinimumYawSpeed, Is.GreaterThan(0f));
        Assert.That(_config.PreLaunchCameraPriority, Is.GreaterThan(_config.RunCameraInactivePriority));
        Assert.That(_config.RunCameraActivePriority, Is.GreaterThan(_config.PreLaunchCameraPriority));
    }

    [Test]
    public void NegativeSerializedRatesAndSpeed_AreClampedToZero()
    {
        var serializedObject = new SerializedObject(_config);
        serializedObject.FindProperty(RunCameraConfig.Serialization.PositionResponseRate).floatValue = -1f;
        serializedObject.FindProperty(RunCameraConfig.Serialization.YawResponseRate).floatValue = -2f;
        serializedObject.FindProperty(RunCameraConfig.Serialization.MinimumYawSpeed).floatValue = -3f;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        Assert.That(_config.PositionResponseRate, Is.EqualTo(0f));
        Assert.That(_config.YawResponseRate, Is.EqualTo(0f));
        Assert.That(_config.MinimumYawSpeed, Is.EqualTo(0f));
    }
}
