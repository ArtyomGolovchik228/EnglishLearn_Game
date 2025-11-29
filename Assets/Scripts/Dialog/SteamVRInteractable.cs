using UnityEngine;

public class SteamVRInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string interactableName = "Object";

    [Header("Visual Feedback")]
    public Material highlightMaterial;
    public bool showGizmos = true;

    private Material originalMaterial;
    private Renderer objectRenderer;
    private TeleportToInteractable teleportSystem;

    public NPCDialogSystem dialogManager;


    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }

        teleportSystem = FindObjectOfType<TeleportToInteractable>();
    }

    // Вызывается из TeleportToInteractable при взаимодействии
    public void OnInteract()
    {
        if (teleportSystem != null)
        {
            teleportSystem.OnInteractableClicked(transform);
        }
    }

    // Для редактора - взаимодействие по клику
    void OnMouseDown()
    {
        if (!Application.isPlaying) return;

        OnInteract();
    }

    void OnMouseEnter()
    {
        if (!Application.isPlaying) return;

        if (objectRenderer != null && highlightMaterial != null)
        {
            objectRenderer.material = highlightMaterial;
        }
    }

    void OnMouseExit()
    {
        if (!Application.isPlaying) return;

        if (objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }
    }
}