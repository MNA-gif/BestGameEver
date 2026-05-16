using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoomStateManager : MonoBehaviour
{
    public static RoomStateManager Instance;

    [Header("Nesneler")]
    public AnomalyObject[] allObjects;
    [Header("Zorunlu Anomaliler")]
     public AnomalyObject[] forcedAnomalies; // Her zaman çıksın

    [Header("Ayarlar")]
    public float observationTime = 10f;
    public float roundTime = 60f;
    public int minAnomaly = 3;
    public int maxAnomaly = 4;

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;

    [HideInInspector] public bool isAnomalyPhase = false;

    private List<AnomalyObject> activeAnomalies = new();
    private int foundCount = 0;
    private int wrongCount = 0;

    void Awake() => Instance = this;

    void Start() { }

    public void StartGame()
    {
        StartCoroutine(GameFlow());
    }

    IEnumerator GameFlow()
    {
        SetAllNormal();
        isAnomalyPhase = false;
        Debug.Log("Odayı incele! " + observationTime + " saniye.");
        yield return new WaitForSeconds(observationTime);

        yield return Fade(1f);
        PlaceAnomalies();
        yield return new WaitForSeconds(0.5f);
        yield return Fade(0f);
        yield return AnomalyScreenUI.Instance.ShowAnomalyScreen();

        isAnomalyPhase = true;
        float timer = roundTime;
        Debug.Log("Anomalileri bul! (" + activeAnomalies.Count + " tane)");

        while (timer > 0 && foundCount < activeAnomalies.Count)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        isAnomalyPhase = false;
        Debug.Log($"Tur bitti! Bulunan: {foundCount}/{activeAnomalies.Count} | Yanlış: {wrongCount}");
    }

    void SetAllNormal()
    {
        foreach (var obj in allObjects)
            obj.SetNormalState();
    }

    void PlaceAnomalies()
{
    activeAnomalies.Clear();
    foundCount = 0;
    wrongCount = 0;

    // Zorunlu anomalileri ekle
    foreach (var obj in forcedAnomalies)
    {
        obj.isAnomaly = true;
        obj.SetAnomalyState();
        activeAnomalies.Add(obj);
        Debug.Log("Zorunlu Anomali: " + obj.gameObject.name);
    }

    // Geri kalanı rastgele seç
    int count = Random.Range(minAnomaly, maxAnomaly + 1);
    var shuffled = System.Linq.Enumerable
        .OrderBy(allObjects, _ => Random.value)
        .Take(count)
        .ToList();

    foreach (var obj in shuffled)
    {
        if (activeAnomalies.Contains(obj)) continue; // Zaten ekliyse atla
        obj.isAnomaly = true;
        obj.SetAnomalyState();
        activeAnomalies.Add(obj);
        Debug.Log("Anomali: " + obj.gameObject.name);
    }
}

    public void AnomalyFound()
    {
        foundCount++;
        Debug.Log($"Bulunan: {foundCount}/{activeAnomalies.Count}");
    }

    public void WrongClick()
    {
        wrongCount++;
        Debug.Log("Yanlış tıklama! Toplam: " + wrongCount);
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