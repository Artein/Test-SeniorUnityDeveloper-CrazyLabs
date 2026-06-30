using System.Linq;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    private void AssertBandFiveCueLines(
        Scene scene,
        GameObject courseRoot,
        RunProgressFrameSource runProgressFrameSource)
    {
        var approachCuePositions = AssertCoinCueLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 5 Finish Approach Coin Cue",
            5,
            350f,
            360f,
            384f,
            392f);

        var funnelCuePositions = AssertCoinCueLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 5 Final Funnel Coin Cue",
            5,
            390f,
            400f,
            414f,
            420f);

        AssertCueLineMovesTowardCenter(runProgressFrameSource, approachCuePositions, "Band 5 finish approach cue");
        Assert.That(funnelCuePositions.Select(position => Mathf.Abs(position.x)).Max(), Is.LessThanOrEqualTo(0.75f));
    }

    private void AssertCueLineMovesTowardCenter(
        RunProgressFrameSource runProgressFrameSource,
        Vector3[] cuePositions,
        string cueDescription)
    {
        var cueProgresses = GetCueProgresses(runProgressFrameSource, cuePositions);

        var orderedPositions = cuePositions
            .Select((position, cueIndex) => new
            {
                Position = position,
                Progress = cueProgresses[cueIndex]
            })
            .OrderBy(sample => sample.Progress)
            .Select(sample => sample.Position)
            .ToArray();
        var firstAbsLateral = Mathf.Abs(orderedPositions[0].x);
        var finalAbsLateral = Mathf.Abs(orderedPositions[orderedPositions.Length - 1].x);

        Assert.That(firstAbsLateral, Is.GreaterThanOrEqualTo(1.5f), cueDescription);
        Assert.That(finalAbsLateral, Is.LessThanOrEqualTo(0.75f), cueDescription);
        Assert.That(firstAbsLateral, Is.GreaterThan(finalAbsLateral), cueDescription);
    }

    private void AssertObstacleCountInProgressRange(
        Scene scene,
        RunProgressFrameSource runProgressFrameSource,
        float minimumProgress,
        float maximumProgress,
        int maximumCount,
        string rangeDescription)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var obstacleNames = FindComponentsInScene<RunContact>(scene)
            .Where(contact => contact.Category == RunContactCategory.Obstacle)
            .Select(contact => new
            {
                Contact = contact,
                Collider = contact.GetComponent<Collider>()
            })
            .Where(candidate => candidate.Collider != null)
            .Where(candidate =>
            {
                var progress = frame.GetForwardProgress(candidate.Collider.bounds.center);
                return progress >= minimumProgress && progress <= maximumProgress;
            })
            .Select(candidate => candidate.Contact.name)
            .ToArray();

        Assert.That(
            obstacleNames.Length,
            Is.LessThanOrEqualTo(maximumCount),
            $"{rangeDescription}: {string.Join(", ", obstacleNames)}");
    }

    private Collider AssertRunFinishContact(
        Scene scene,
        GameObject courseRoot,
        RunProgressFrameSource runProgressFrameSource,
        string finishName)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var finishObject = FindGameObjectByName(scene, finishName);
        var collider = finishObject.GetComponent<Collider>();
        var runContact = finishObject.GetComponent<RunContact>();
        var meshRenderer = finishObject.GetComponent<MeshRenderer>();
        var classifier = new RunContactClassifier(new FakeRunEndConfig());

        Assert.That(finishObject.transform.IsChildOf(courseRoot.transform), Is.True, finishName);
        Assert.That(collider, Is.Not.Null, finishName);
        var progress = frame.GetForwardProgress(collider.bounds.center);

        Assert.That(collider.isTrigger, Is.True, finishName);
        Assert.That(runContact, Is.Not.Null, finishName);
        Assert.That(runContact.Category, Is.EqualTo(RunContactCategory.Finish), finishName);
        Assert.That(meshRenderer, Is.Not.Null, finishName);
        Assert.That(meshRenderer.enabled, Is.True, finishName);
        Assert.That(collider.bounds.size.x, Is.GreaterThanOrEqualTo(5.5f), finishName);
        Assert.That(collider.bounds.size.y, Is.GreaterThanOrEqualTo(1.5f), finishName);
        Assert.That(progress, Is.InRange(410f, 420f), finishName);
        Assert.That(classifier.TryClassify(new RigidbodyTriggerNotification(collider), out var candidate), Is.True, finishName);
        Assert.That(candidate.Reason, Is.EqualTo(RunEndReason.Finished), finishName);

        return collider;
    }

    private sealed class FakeRunEndConfig : IRunEndConfig
    {
        public float ObstacleImpactSpeedThreshold => 5f;
        public float LostMomentumLaunchGraceDuration => 0.1f;
        public float LostMomentumDuration => 0.2f;
        public float LostMomentumPlanarSpeedThreshold => 0.5f;
        public float LostMomentumProgressThreshold => 0.05f;
        public float RunEndedDelay => 0.02f;
    }
}
