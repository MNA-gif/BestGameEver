using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AnomalyInventory : MonoBehaviour
{
    public static AnomalyInventory Instance;

    public List<Image> slots; // SlotImage'ları sürükle

    private int currentSlot = 0;

    void Awake() => Instance = this;

    void Start()
    {
        // Başta tüm slotları gizle
        foreach (var slot in slots)
        {
            var c = slot.color;
            c.a = 0f;
            slot.color = c;
        }
    }

public void AddAnomaly(Sprite sprite)
{
    Debug.Log("AddAnomaly çağrıldı, slot: " + currentSlot);
    
    if (currentSlot >= slots.Count)
    {
        Debug.Log("Slot dolu!");
        return;
    }

    var slot = slots[currentSlot];

    if (sprite != null)
    {
        slot.sprite = sprite;
        slot.color = Color.white;
    }
    else
    {
        slot.color = Color.green;
    }

    currentSlot++;
}
    public void ResetInventory()
    {
        currentSlot = 0;
        foreach (var slot in slots)
        {
            slot.sprite = null;
            var c = slot.color;
            c.a = 0f;
            slot.color = c;
        }
    }
}