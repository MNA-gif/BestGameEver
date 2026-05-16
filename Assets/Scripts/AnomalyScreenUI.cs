using UnityEngine;
using TMPro;
using System.Collections;

public class AnomalyScreenUI : MonoBehaviour
{
    public static AnomalyScreenUI Instance;

    public CanvasGroup panelGroup;
    public TextMeshProUGUI anomalyText;

    public float fadeInDuration = 1f;
    public float holdDuration = 2f;
    public float fadeOutDuration = 1f;

    [Header("Ses")]
    public AudioClip whisperClip;
    private AudioSource audioSource;

    void Awake() => Instance = this;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        panelGroup.alpha = 0f;
        var c = anomalyText.color;
        c.a = 0f;
        anomalyText.color = c;
    }

    public IEnumerator ShowAnomalyScreen()
    {
        if (whisperClip != null)
            audioSource.PlayOneShot(whisperClip);

        yield return Fade(1f, fadeInDuration);
        yield return FadeText(1f, fadeInDuration);
        yield return new WaitForSeconds(holdDuration);
        yield return FadeText(0f, fadeOutDuration);
        yield return Fade(0f, fadeOutDuration);
    }

    IEnumerator Fade(float target, float duration)
    {
        float start = panelGroup.alpha;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            panelGroup.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        panelGroup.alpha = target;
    }

    IEnumerator FadeText(float target, float duration)
    {
        Color c = anomalyText.color;
        float start = c.a;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(start, target, t / duration);
            anomalyText.color = c;
            yield return null;
        }
        c.a = target;
        anomalyText.color = c;
    }
}