using UnityEngine;

public class ItemDeliveryZone : MonoBehaviour
{
    [Header("Settings")]
    public string requiredItemName;
    public Renderer zoneRenderer;
    public Color waitingColor = Color.yellow;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;

    private NPCDialogSystem dialogSystem;
    private bool isActive = false;

    public void StartWaitingForItem(string itemName, NPCDialogSystem system)
    {
        requiredItemName = itemName;
        dialogSystem = system;
        isActive = true;

        if (zoneRenderer != null)
        {
            zoneRenderer.material.color = waitingColor;
            zoneRenderer.gameObject.SetActive(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        QuestItem item = other.GetComponent<QuestItem>();
        if (item != null)
        {
            bool isCorrect = item.itemName == requiredItemName;
            dialogSystem.OnItemDelivered(item.itemName, isCorrect);

            // Визуальная обратная связь
            if (zoneRenderer != null)
            {
                zoneRenderer.material.color = isCorrect ? correctColor : incorrectColor;
            }

            // Убираем предмет
            item.gameObject.SetActive(false);

            isActive = false;
        }
    }

    public void StopWaiting()
    {
        isActive = false;
        if (zoneRenderer != null)
        {
            zoneRenderer.gameObject.SetActive(false);
        }
    }
}