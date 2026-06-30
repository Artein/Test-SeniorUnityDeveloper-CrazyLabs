using System.IO;
using System.Linq;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
public sealed class PullHintViewTests
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
    public void ShowAt_WithFingerImage_ActivatesRootPositionsWithOffsetAndResetsHiddenPose()
    {
        var view = CreateView(out var root, out var canvasGroup, out var fingerRoot, out var fingerImage);
        canvasGroup.alpha = 1f;
        fingerRoot.anchoredPosition = new Vector2(5f, -50f);
        fingerRoot.localScale = new Vector3(0.5f, 1.5f, 1f);

        view.SetReferencesForTests(root, canvasGroup, fingerRoot, fingerImage, root.GetComponent<Animator>(), 2f, 1.25f, 4f, new Vector2(3f, 4f),
            "PlayPullHint");

        ((IPullHintView)view).ShowAt(new Vector2(10f, 20f));

        Assert.That(root.gameObject.activeSelf, Is.True);
        Assert.That(root.position.x, Is.EqualTo(13f).Within(0.0001f));
        Assert.That(root.position.y, Is.EqualTo(24f).Within(0.0001f));
        Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(fingerRoot.anchoredPosition.x, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(fingerRoot.anchoredPosition.y, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(fingerRoot.localScale, Is.EqualTo(Vector3.one));
        Assert.That(fingerImage.raycastTarget, Is.False);
    }

    [Test]
    public void Play_WithAnimator_DoesNotRequireSpritesheetFrames()
    {
        var view = CreateView(out var root, out var canvasGroup, out var fingerRoot, out var fingerImage);

        view.SetReferencesForTests(root, canvasGroup, fingerRoot, fingerImage, root.GetComponent<Animator>(), 2f, 1.25f, 4f, Vector2.zero,
            "PlayPullHint");

        Assert.That(() => ((IPullHintView)view).Play(), Throws.Nothing);
        Assert.That(fingerImage.raycastTarget, Is.False);
    }

    [Test]
    public void Hide_WhenVisible_HidesRootAndSetsAlphaToZero()
    {
        var view = CreateView(out var root, out var canvasGroup, out var fingerRoot, out var fingerImage);

        view.SetReferencesForTests(root, canvasGroup, fingerRoot, fingerImage, root.GetComponent<Animator>(), 2f, 1.25f, 4f, Vector2.zero,
            "PlayPullHint");
        ((IPullHintView)view).ShowAt(Vector2.zero);
        canvasGroup.alpha = 1f;

        ((IPullHintView)view).Hide();

        Assert.That(root.gameObject.activeSelf, Is.False);
        Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(fingerImage.raycastTarget, Is.False);
    }

    [Test]
    public void TuningProperties_ReturnClampedSerializedValues()
    {
        var view = CreateView(out var root, out var canvasGroup, out var fingerRoot, out var fingerImage);

        view.SetReferencesForTests(root, canvasGroup, fingerRoot, fingerImage, root.GetComponent<Animator>(), 2f, 1.25f, 4f, Vector2.zero,
            "PlayPullHint");

        Assert.That(((IPullHintTuning)view).InitialIdleDelaySeconds, Is.EqualTo(2f));
        Assert.That(((IPullHintTuning)view).PlaybackDurationSeconds, Is.EqualTo(1.25f));
        Assert.That(((IPullHintTuning)view).RepeatCooldownSeconds, Is.EqualTo(4f));
    }

    [Test]
    public void PullHintFingerAsset_ImportedForAnimatorDrivenUi()
    {
        var assetPath = FindSinglePullHintFingerAssetPath();
        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        var sourceTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Track(sourceTexture);

        Assert.That(sourceTexture.LoadImage(File.ReadAllBytes(assetPath)), Is.True);

        var alphaBounds = CalculateAlphaBounds(sourceTexture);
        Assert.That(sourceTexture.width, Is.EqualTo(512));
        Assert.That(sourceTexture.height, Is.EqualTo(512));
        Assert.That(alphaBounds.HasValue, Is.True);
        Assert.That(alphaBounds.Value.Left, Is.LessThanOrEqualTo(8));
        Assert.That(alphaBounds.Value.Top, Is.LessThanOrEqualTo(8));
        Assert.That(alphaBounds.Value.Right, Is.LessThanOrEqualTo(8));
        Assert.That(alphaBounds.Value.Bottom, Is.LessThanOrEqualTo(8));
        Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
        Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
        Assert.That(importer.maxTextureSize, Is.EqualTo(256));
        Assert.That(importer.alphaIsTransparency, Is.True);
        Assert.That(importer.isReadable, Is.False);
    }

    private PullHintView CreateView(
        out RectTransform root,
        out CanvasGroup canvasGroup,
        out RectTransform fingerRoot,
        out Image fingerImage)
    {
        var viewObject = Track(new GameObject("Pull Hint", typeof(RectTransform), typeof(CanvasGroup), typeof(Animator), typeof(PullHintView)));
        var fingerObject = new GameObject("Finger", typeof(RectTransform), typeof(Image));
        fingerObject.transform.SetParent(viewObject.transform, false);

        root = viewObject.GetComponent<RectTransform>();
        canvasGroup = viewObject.GetComponent<CanvasGroup>();
        fingerRoot = fingerObject.GetComponent<RectTransform>();
        fingerImage = fingerObject.GetComponent<Image>();
        fingerImage.raycastTarget = true;
        return viewObject.GetComponent<PullHintView>();
    }

    private static string FindSinglePullHintFingerAssetPath()
    {
        var assetPaths = AssetDatabase.FindAssets("PullHintFinger t:Texture2D")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => Path.GetFileNameWithoutExtension(path) == "PullHintFinger")
            .ToArray();

        Assert.That(assetPaths, Has.Length.EqualTo(1));
        return assetPaths[0];
    }

    private static AlphaBounds? CalculateAlphaBounds(Texture2D texture)
    {
        var pixels = texture.GetPixels32();
        var minX = texture.width;
        var minY = texture.height;
        var maxX = -1;
        var maxY = -1;

        for (var y = 0; y < texture.height; y += 1)
        {
            for (var x = 0; x < texture.width; x += 1)
            {
                if (pixels[(y * texture.width) + x].a == 0)
                    continue;

                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }

        if (maxX < minX || maxY < minY)
            return null;

        return new AlphaBounds(minX, texture.height - 1 - maxY, texture.width - 1 - maxX, minY);
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private readonly struct AlphaBounds
    {
        public AlphaBounds(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left { get; }
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }
    }
}
