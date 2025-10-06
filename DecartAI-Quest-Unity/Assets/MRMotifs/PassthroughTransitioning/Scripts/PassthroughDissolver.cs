// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.UI;

namespace MRMotifs.PassthroughTransitioning
{
    [MetaCodeSample("MRMotifs-PassthroughTransitioning")]
    public class PassthroughDissolver : MonoBehaviour
    {
        [Tooltip("The range of the passthrough dissolver sphere.")]
        [SerializeField] private float distance = 20f;

        [Tooltip("The inverted alpha value at which the contextual boundary should be enabled/disabled.")]
        [SerializeField] private float boundaryThreshold = 0.25f;

        [Tooltip("If enabled, automatically oscillates the dissolve level.")]
        [SerializeField] private bool autoAdjust = true;

        [Tooltip("Speed of automatic dissolution oscillation.")]
        [SerializeField] private float autoSpeed = 0.5f;

        private Camera m_mainCamera;
        private Material m_material;
        private MeshRenderer m_meshRenderer;
        private Slider m_alphaSlider;

        private static readonly int s_dissolutionLevel = Shader.PropertyToID("_Level");

        private float m_autoTime;

        private void Awake()
        {
            m_mainCamera = Camera.main;
            if (m_mainCamera != null)
            {
                // Required for proper blending with regular transparency
                //m_mainCamera.clearFlags = CameraClearFlags.Skybox;
            }

            // Recommended for clean blending behavior
            OVRManager.eyeFovPremultipliedAlphaModeEnabled = true;

            m_meshRenderer = GetComponent<MeshRenderer>();
            m_material = m_meshRenderer.material;
            m_material.SetFloat(s_dissolutionLevel, 0);
            m_meshRenderer.enabled = true;

            SetSphereSize(distance);

#if UNITY_ANDROID
           // CheckIfPassthroughIsRecommended();
#endif
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

        private void CheckIfPassthroughIsRecommended()
        {
            float val = OVRManager.IsPassthroughRecommended() ? 1f : 0f;
            m_material.SetFloat(s_dissolutionLevel, val);
            OVRManager.instance.shouldBoundaryVisibilityBeSuppressed = OVRManager.IsPassthroughRecommended();
        }

        private void HandleSliderChange(float value)
        {
            m_material.SetFloat(s_dissolutionLevel, value);
            if (value > boundaryThreshold || value < boundaryThreshold)
            {
                OVRManager.instance.shouldBoundaryVisibilityBeSuppressed = value > boundaryThreshold;
            }
        }
    }
}
