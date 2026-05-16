using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FeedbackFlash : MonoBehaviour
{
    public static FeedbackFlash Instance;

    public Image flashOverlay;
    public float flashDuration = 0.3f;

    void Awake() => Instance = this;

    public void ShowCorrect() => StartCoroutine(Flash(Color.green));
    public void ShowWrong()   => StartCoroutine(Flash(Color.red));

    IEnumerator Flash(Color color)
    {
        color.a = 0.4f;
        flashOverlay.color = color;

        float t = 0;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0.4f, 0f, t / flashDuration);
            flashOverlay.color = color;
            yield return null;
        }

        color.a = 0f;
        flashOverlay.color = color;
    }
}