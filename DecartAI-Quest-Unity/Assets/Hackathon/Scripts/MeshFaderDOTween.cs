using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MeshFaderDOTween : MonoBehaviour
{
    [Tooltip("How long the fade takes in seconds.")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Tooltip("Target alpha when fading in (usually 1).")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeInAlpha = 1f;

    [Tooltip("Target alpha when fading out (usually 0).")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeOutAlpha = 0f;

    private List<Material> materials = new();
    private MeshRenderer[] renderers;

    private void Awake()
    {
        CollectMaterials();
    }

    private void CollectMaterials()
    {
        materials.Clear();
        renderers = GetComponentsInChildren<MeshRenderer>(true);

        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                // Duplicate instance so it doesn’t change shared material
                var instanceMat = r.material = new Material(mat);
                materials.Add(instanceMat);
            }
        }
    }

    public void FadeIn()
    {
        SetChildrenActive(true);
        FadeTo(fadeInAlpha, false);
    }

    public void FadeOut()
    {
        FadeTo(fadeOutAlpha, true);
        SetChildrenActive(false);
    }

    private void FadeTo(float targetAlpha, bool disableAfter)
    {
        int completedTweens = 0;
        int totalTweens = materials.Count;

        foreach (var mat in materials)
        {
            if (!mat.HasProperty("_Color")) continue;

            mat.DOFade(targetAlpha, fadeDuration)
               .SetEase(Ease.InOutSine)
               .SetUpdate(true)
               .OnComplete(() =>
               {
                   completedTweens++;
                   if (disableAfter && completedTweens >= totalTweens)
                       SetChildrenActive(false);
               });
        }
    }

    private void SetChildrenActive(bool active)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(active);
    }
}
