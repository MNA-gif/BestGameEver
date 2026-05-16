using UnityEngine;

public class PaintingAnomaly : MonoBehaviour
{
    public Texture2D normalTexture;
    public Texture2D anomalyTexture;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        SetNormal();
    }

    public void SetNormal()
    {
        if (rend != null)
            rend.material.SetTexture("_BaseColorMap", normalTexture);
    }

    public void SetAnomaly()
    {
        if (rend != null)
            rend.material.SetTexture("_BaseColorMap", anomalyTexture);
    }
}