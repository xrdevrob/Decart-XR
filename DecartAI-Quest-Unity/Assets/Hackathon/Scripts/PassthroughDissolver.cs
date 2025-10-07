// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MRMotifs.PassthroughTransitioning
{
    [MetaCodeSample("MRMotifs-PassthroughTransitioning")]
    public class PassthroughDissolver : MonoBehaviour
    {
        [Tooltip("The range of the passthrough dissolver sphere.")]
        [SerializeField] private float distance = 20f;

        [Tooltip("If enabled, automatically oscillates the dissolve level.")]
        [SerializeField] private bool autoAdjust = true;

        [Tooltip("Speed of automatic dissolution oscillation.")]
        [SerializeField] private float autoSpeed = 0.5f;

        [SerializeField] private float maxValue = 1.0f;
        [SerializeField] private float minValue = -0.2f;

        [Tooltip("Speed of fade transitions (seconds to reach target).")]
        [SerializeField] private float fadeDuration = 1.5f;

        private Camera m_mainCamera;
        private Material m_material;
        private MeshRenderer m_meshRenderer;
        private Slider m_alphaSlider;

        private static readonly int s_dissolutionLevel = Shader.PropertyToID("_Level");

        private float m_autoTime;
        private Coroutine fadeRoutine;

        private void Awake()
        {
            m_mainCamera = Camera.main;
            OVRManager.eyeFovPremultipliedAlphaModeEnabled = true;

            m_meshRenderer = GetComponent<MeshRenderer>();
            m_material = m_meshRenderer.material;
            m_material.SetFloat(s_dissolutionLevel, minValue);
            m_meshRenderer.enabled = true;

            SetSphereSize(distance);
        }

        private void Update()
        {
            if (autoAdjust)
            {
                m_autoTime += Time.deltaTime * autoSpeed;
                float value = Mathf.PingPong(m_autoTime, 1f);
                HandleSliderChange(value);
            }
        }

        private void SetSphereSize(float size)
        {
            transform.localScale = new Vector3(size, size, size);
        }

        private void HandleSliderChange(float value)
        {
            m_material.SetFloat(s_dissolutionLevel, value);
        }

        // -------------------- New Methods --------------------

        /// <summary>
        /// Smoothly fades the dissolution level to the maximum value.
        /// </summary>
        public void FadeToMax()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(maxValue));
        }

        /// <summary>
        /// Smoothly fades the dissolution level to the minimum value.
        /// </summary>
        public void FadeToMin()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(minValue));
        }

        private IEnumerator FadeRoutine(float target)
        {
            float start = m_material.GetFloat(s_dissolutionLevel);
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float newValue = Mathf.Lerp(start, target, t);
                m_material.SetFloat(s_dissolutionLevel, newValue);
                yield return null;
            }

            m_material.SetFloat(s_dissolutionLevel, target);
            fadeRoutine = null;
        }
    }
}
