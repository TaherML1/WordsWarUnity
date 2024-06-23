using UnityEngine;
using UnityEngine.UI;

public class TopicAnimation : MonoBehaviour
{
    public GameObject topicObject; // Reference to the GameObject you want to animate
    public float initialDelay = 0.5f; // Delay before the animation starts
    public float moveTime = 1.0f; // Duration of the movement animation
    public Vector3 moveOffset = new Vector3(0, 100f, 0); // Offset for where the topic should move to
    public float scaleUpFactor = 1.5f; // Factor to scale up the topic when it appears in the middle

    private Vector3 originalScale;

    void Start()
    {
        originalScale = topicObject.transform.localScale;

        // Start the animation after initial delay
        LeanTween.delayedCall(initialDelay, PlayTopicAnimation);
    }

    void PlayTopicAnimation()
    {
        // Scale up the topic object
        LeanTween.scale(topicObject, originalScale * scaleUpFactor, moveTime / 2)
                 .setEase(LeanTweenType.easeOutQuad)
                 .setOnComplete(() =>
                 {
                     // Move the topic from the middle to the above section
                     LeanTween.moveLocalY(topicObject, topicObject.transform.localPosition.y + moveOffset.y, moveTime)
                              .setEase(LeanTweenType.easeOutQuad)
                              .setOnComplete(() =>
                              {
                                  // Optionally, you can scale down here if needed
                                  // Scale down the topic object back to original size (if desired)
                                  // LeanTween.scale(topicObject, originalScale, moveTime / 2)
                                  //         .setEase(LeanTweenType.easeOutQuad);

                                  // Optionally, fade out or hide the topicObject
                                  // LeanTween.alpha(topicObject.GetComponent<CanvasGroup>(), 0f, 0.5f)
                                  //         .setEase(LeanTweenType.easeOutQuad)
                                  //         .setOnComplete(() => Destroy(topicObject)); // Destroy the topicObject after fading out
                              });
                 });
    }
}
