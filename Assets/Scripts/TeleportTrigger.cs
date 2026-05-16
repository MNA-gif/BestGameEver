using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class TeleportTrigger : MonoBehaviour
{
    [Header("Işınlanma")]
    public Transform destination;
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;

    [Header("VFX")]
    public VisualEffect teleportEffect;

    private bool playerNearby = false;
    private bool teleporting = false;

    [Header("Ses")]
public AudioClip teleportClip;
private AudioSource audioSource;

void Start()
{
    audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.playOnAwake = false;
}

    void Update()
    {
        if (playerNearby && !teleporting && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(Teleport());
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
    }

    IEnumerator Teleport()
    {
        teleporting = true;

        // Orb'u aç ve buildup başlat
         if (teleportClip != null)
        audioSource.PlayOneShot(teleportClip);
        
        if (teleportEffect != null)
        {
            teleportEffect.gameObject.SetActive(true);
            teleportEffect.SendEvent("buildup");
        }

        yield return new WaitForSeconds(1f);

        // Create eventi
        if (teleportEffect != null)
            teleportEffect.SendEvent("create");

        yield return new WaitForSeconds(1.5f);

        // Ekran kararır
        yield return Fade(1f);

        // Karakteri taşı
        var player = GameObject.FindGameObjectWithTag("Player");
        var cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.transform.position = destination.position;
        player.transform.rotation = destination.rotation;
        yield return new WaitForSeconds(0.2f);
        cc.enabled = true;

        // Ekran açılır
        yield return Fade(0f);

        // Orb'u durdur ve gizle
        if (teleportEffect != null)
        {
            teleportEffect.SendEvent("stop");
            yield return new WaitForSeconds(1f);
            teleportEffect.gameObject.SetActive(false);
        }

        // Oyunu başlat
        RoomStateManager.Instance.StartGame();

        teleporting = false;
    }

    IEnumerator Fade(float target)
    {
        float start = fadeCanvas.alpha;
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = target;
    }
}