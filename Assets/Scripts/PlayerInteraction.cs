using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float rayDistance = 10f;
    private Camera cam;

    void Start() => cam = Camera.main;

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            var anomaly = hit.collider.GetComponentInParent<AnomalyObject>();
            if (anomaly != null)
                anomaly.TryInteract();
        }
    }
}