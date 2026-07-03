using System;
using System.Collections;
using UnityEngine;

namespace Game.Gameplay
{
    internal interface IFinishPresentationView
    {
        void PlaySuccessCelebration();
        void ResetForRunPreparation();
    }

    public sealed class FinishPresentationView : MonoBehaviour, IFinishPresentationView
    {
        [SerializeField] private Renderer _thresholdRenderer;
        [SerializeField] private ParticleSystem[] _successParticles = Array.Empty<ParticleSystem>();
        [SerializeField] [Min(0f)] private float _thresholdFadeSeconds = 0.35f;

        private readonly string _baseColorPropertyName = "_BaseColor";
        private readonly string _colorPropertyName = "_Color";
        private MaterialPropertyBlock _thresholdPropertyBlock;
        private Coroutine _thresholdFadeCoroutine;

        private void Awake()
        {
            ResetForRunPreparation();
        }

        public void PlaySuccessCelebration()
        {
            PlaySuccessParticles();
            StartThresholdFadeOut();
        }

        public void ResetForRunPreparation()
        {
            StopThresholdFade();
            ApplyThresholdAlpha(1f);
            StopAndClearSuccessParticles();
        }

        private void PlaySuccessParticles()
        {
            var particles = GetSuccessParticles();

            for (var particleIndex = 0; particleIndex < particles.Length; particleIndex += 1)
            {
                var particle = particles[particleIndex];

                if (particle == null)
                    continue;

                particle.Play(true);
            }
        }

        private void StopAndClearSuccessParticles()
        {
            var particles = GetSuccessParticles();

            for (var particleIndex = 0; particleIndex < particles.Length; particleIndex += 1)
            {
                var particle = particles[particleIndex];

                if (particle == null)
                    continue;

                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particle.Clear(true);
            }
        }

        private void StartThresholdFadeOut()
        {
            StopThresholdFade();

            if (_thresholdFadeSeconds <= 0f)
            {
                ApplyThresholdAlpha(0f);
                return;
            }

            _thresholdFadeCoroutine = StartCoroutine(FadeThresholdOut());
        }

        private IEnumerator FadeThresholdOut()
        {
            var elapsedSeconds = 0f;

            while (elapsedSeconds < _thresholdFadeSeconds)
            {
                elapsedSeconds += Time.deltaTime;
                ApplyThresholdAlpha(1f - Mathf.Clamp01(elapsedSeconds / _thresholdFadeSeconds));
                yield return null;
            }

            ApplyThresholdAlpha(0f);
            _thresholdFadeCoroutine = null;
        }

        private void StopThresholdFade()
        {
            if (_thresholdFadeCoroutine == null)
                return;

            StopCoroutine(_thresholdFadeCoroutine);
            _thresholdFadeCoroutine = null;
        }

        private void ApplyThresholdAlpha(float alpha)
        {
            if (_thresholdRenderer == null)
                return;

            var clampedAlpha = Mathf.Clamp01(alpha);
            _thresholdRenderer.enabled = clampedAlpha > 0f;
            EnsureThresholdPropertyBlock();
            _thresholdRenderer.GetPropertyBlock(_thresholdPropertyBlock);
            ApplyMaterialColorAlpha(_baseColorPropertyName, clampedAlpha);
            ApplyMaterialColorAlpha(_colorPropertyName, clampedAlpha);
            _thresholdRenderer.SetPropertyBlock(_thresholdPropertyBlock);
        }

        private void ApplyMaterialColorAlpha(string propertyName, float alpha)
        {
            var material = _thresholdRenderer.sharedMaterial;

            if (material == null || !material.HasProperty(propertyName))
                return;

            var color = material.GetColor(propertyName);
            color.a = alpha;
            _thresholdPropertyBlock.SetColor(propertyName, color);
        }

        private void EnsureThresholdPropertyBlock()
        {
            _thresholdPropertyBlock ??= new MaterialPropertyBlock();
        }

        private ParticleSystem[] GetSuccessParticles()
        {
            return _successParticles ?? Array.Empty<ParticleSystem>();
        }

#if UNITY_INCLUDE_TESTS
        internal Renderer ThresholdRendererForTests => _thresholdRenderer;
        internal ParticleSystem[] SuccessParticlesForTests => GetSuccessParticles();

        internal void SetReferencesForTests(Renderer thresholdRenderer, ParticleSystem[] successParticles, float thresholdFadeSeconds)
        {
            _thresholdRenderer = thresholdRenderer;
            _successParticles = successParticles ?? Array.Empty<ParticleSystem>();
            _thresholdFadeSeconds = thresholdFadeSeconds;
        }
#endif
    }
}
