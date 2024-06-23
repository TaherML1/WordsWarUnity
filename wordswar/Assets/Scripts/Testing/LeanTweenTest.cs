using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeanTweenTest : MonoBehaviour
{
    void Start()
    {
        // Move the object from its current position to (5, 2, 0) over 2 seconds
        Vector3 targetPosition = new Vector3(5f, 2f, 0f);
        LeanTween.move(gameObject, targetPosition, 2f).setEase(LeanTweenType.easeOutQuad);

        // Rotate the object around the z-axis by 180 degrees over 1.5 seconds
        LeanTween.rotateAround(gameObject, Vector3.forward, 180f, 1.5f).setEase(LeanTweenType.easeInOutCubic);

        // Scale the object to (2, 2, 2) over 1 second
        Vector3 targetScale = new Vector3(2f, 2f, 2f);
        LeanTween.scale(gameObject, targetScale, 1f).setEase(LeanTweenType.easeOutElastic);

        // Fade out the object's alpha to 0 over 1 second
        LeanTween.alpha(gameObject, 0f, 1f).setEase(LeanTweenType.easeInExpo).setDelay(1.5f);

        // Chain tweens
        LeanTween.move(gameObject, new Vector3(-5f, 0f, 0f), 2f).setEase(LeanTweenType.easeInOutBack).setDelay(3f);
    }
}
