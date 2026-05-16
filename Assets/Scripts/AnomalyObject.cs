using UnityEngine;

public class AnomalyObject : MonoBehaviour
{
    [Header("Anomali Tipi")]
    public bool anomalyAppears = false;
    public bool anomalyDisappears = false;

    [Header("Anomali Pozisyon (sadece taşıyorsa)")]
    public Vector3 anomalyPosition;
    public Vector3 anomalyRotation;

    [Header("Görsel")]
    public Sprite anomalySprite;

    [HideInInspector] public bool isAnomaly = false;
    [HideInInspector] public bool foundByPlayer = false;

    private Vector3 normalPosition;
    private Vector3 normalRotation;

    void Start()
    {
        normalPosition = transform.position;
        normalRotation = transform.eulerAngles;

        if (anomalyAppears)
            gameObject.SetActive(false);
    }

    public void SetNormalState()
    {
        if (anomalyAppears)
        {
            gameObject.SetActive(false);
            isAnomaly = false;
            foundByPlayer = false;
            return;
        }

        transform.position = normalPosition;
        transform.eulerAngles = normalRotation;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        // Tablo normal haline dönsün
        var painting = GetComponentInChildren<PaintingAnomaly>();
        if (painting != null)
            painting.SetNormal();

        isAnomaly = false;
        foundByPlayer = false;
    }

    public void SetAnomalyState()
    {
        if (anomalyAppears)
        {
            gameObject.SetActive(true);
            return;
        }

        if (anomalyDisappears)
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
            return;
        }

        // Tablo anomalisi
        var painting = GetComponentInChildren<PaintingAnomaly>();
        if (painting != null)
        {
            painting.SetAnomaly();
            return;
        }

        transform.position = anomalyPosition;
        transform.eulerAngles = anomalyRotation;
    }

    public void TryInteract()
    {
        if (!RoomStateManager.Instance.isAnomalyPhase) return;
        if (foundByPlayer) return;

        if (isAnomaly)
        {
            foundByPlayer = true;
            RoomStateManager.Instance.AnomalyFound();
            FeedbackFlash.Instance.ShowCorrect();
            AnomalyInventory.Instance.AddAnomaly(anomalySprite);
            Debug.Log("Anomali bulundu: " + gameObject.name);
        }
        else
        {
            RoomStateManager.Instance.WrongClick();
            FeedbackFlash.Instance.ShowWrong();
            Debug.Log("Yanlış nesne!");
        }
    }
}