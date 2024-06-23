using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;


public class ButtonAnimation : MonoBehaviour, IPointerClickHandler
{
    public float scaleFactor = 1.2f;
    public float animationTime = 0.3f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Scale up animation
        LeanTween.scale(gameObject, originalScale * scaleFactor, animationTime)
                 .setEase(LeanTweenType.easeOutQuad)
                 .setOnComplete(ScaleDown);
    }

    void ScaleDown()
    {
        // Scale down animation (optional)
        LeanTween.scale(gameObject, originalScale, animationTime)
                 .setEase(LeanTweenType.easeOutQuad);
    }
}