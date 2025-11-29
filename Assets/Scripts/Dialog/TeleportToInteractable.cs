using UnityEngine;
using Valve.VR;
using System.Collections;

public class TeleportToInteractable : MonoBehaviour
{
    [Header("SteamVR References")]
    public Transform vrRig; // VR камера или игрок
    public SteamVR_Action_Boolean interactAction;
    public SteamVR_Input_Sources handType;

    [Header("Dialogue System")]
    public GameObject dialogueCanvas;
    public DialogueManager dialogueManager;

    [Header("Teleport Settings")]
    public float teleportDistance = 2f;
    public float teleportDuration = 1f;

    [Header("Editor Testing")]
    public bool enableEditorTesting = true;
    public KeyCode editorInteractKey = KeyCode.E;

    private bool isTeleporting = false;
    private bool isInteractable = true;

    public ChoiceDialogueManager choiceDialogueManager;
    public SentenceBuilderManager sentenceBuilderManager;

    void Start()
    {
        // Автоматически находим ссылки если не установлены
        if (vrRig == null)
        {
            vrRig = FindObjectOfType<SteamVR_Camera>()?.transform;
            if (vrRig == null)
                vrRig = Camera.main.transform;
        }

        if (dialogueManager == null && dialogueCanvas != null)
            dialogueManager = dialogueCanvas.GetComponent<DialogueManager>();
    }

    void Update()
    {
        // Обработка ввода для SteamVR
        if (interactAction != null && interactAction.GetStateDown(handType) && IsInVR())
        {
            TryInteractWithObject();
        }

        // Обработка ввода для редактора
        if (enableEditorTesting && Input.GetKeyDown(editorInteractKey))
        {
            TryInteractWithObject();
        }
    }

    private void TryInteractWithObject()
    {
        if (isTeleporting || !isInteractable) return;

        Camera cameraToUse = IsInVR() ?
            (vrRig != null ? vrRig.GetComponent<Camera>() : null) :
            Camera.main;

        if (cameraToUse == null) return;

        // Бросок луча из камеры или контроллера
        Ray ray = IsInVR() ?
            new Ray(vrRig.position, vrRig.forward) :
            cameraToUse.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            SteamVRInteractable interactable = hit.collider.GetComponent<SteamVRInteractable>();
            if (interactable != null)
            {
                StartTeleportation(interactable.transform);
            }
        }
    }

    public void OnInteractableClicked(Transform targetObject)
    {
        if (!isTeleporting)
        {
            StartTeleportation(targetObject);
        }
    }

    private void StartTeleportation(Transform targetObject)
    {
        StartCoroutine(TeleportToObject(targetObject));
    }

    private IEnumerator TeleportToObject(Transform targetObject)
    {
        isTeleporting = true;

        Vector3 targetPosition = CalculateTargetPosition(targetObject);
        Quaternion targetRotation = CalculateTargetRotation(targetObject);

        float elapsedTime = 0f;
        Vector3 startPosition = vrRig.position;
        Quaternion startRotation = vrRig.rotation;

        // Визуальная обратная связь перед телепортацией
        OnTeleportStart(targetObject);

        while (elapsedTime < teleportDuration)
        {
            float t = elapsedTime / teleportDuration;
            vrRig.position = Vector3.Lerp(startPosition, targetPosition, t);
            vrRig.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        vrRig.position = targetPosition;
        vrRig.rotation = targetRotation;

        StartDialogue(targetObject.name);
        isTeleporting = false;

        OnTeleportComplete(targetObject);
    }

    private Vector3 CalculateTargetPosition(Transform targetObject)
    {
        Vector3 targetPos = targetObject.position + targetObject.forward * teleportDistance;
        // Сохраняем текущую высоту VR камеры относительно пола
        if (IsInVR())
        {
            targetPos.y = vrRig.position.y;
        }
        else
        {
            // В редакторе используем высоту объекта + небольшое смещение
            targetPos.y = targetObject.position.y + 0.5f;
        }
        return targetPos;
    }

    private Quaternion CalculateTargetRotation(Transform targetObject)
    {
        Vector3 lookDirection = (targetObject.position - vrRig.position).normalized;
        lookDirection.y = 0;

        if (lookDirection == Vector3.zero)
            return vrRig.rotation;

        return Quaternion.LookRotation(lookDirection);
    }

    private void StartDialogue(string objectName)
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(true);
        }

        // В зависимости от объекта запускаем разные типы диалогов
        switch (objectName)
        {

            case "ChoiceNPC":
                if (choiceDialogueManager != null)
                    choiceDialogueManager.StartChoiceDialogue("NPC", new string[] { "John", "Victor", "Mike", "Alex" });
                    Debug.Log($"[START-DIALOGUE] object: {objectName}");
                break;
            case "SentenceNPC":
                if (sentenceBuilderManager != null)
                    sentenceBuilderManager.StartSentenceBuilder("Учитель", new string[] { "love", "I", "games", "programming" });
                break;
            default:
                if (dialogueManager != null)
                    dialogueManager.StartDialogue(objectName);
                break;
        }
    }

    private void OnTeleportStart(Transform targetObject)
    {
        // Можно добавить визуальные эффекты, звуки и т.д.
        Debug.Log($"Starting teleport to {targetObject.name}");
    }

    private void OnTeleportComplete(Transform targetObject)
    {
        // Можно добавить завершающие эффекты
        Debug.Log($"Teleport to {targetObject.name} complete");
    }

    public bool IsInVR()
    {
        return SteamVR.initializedState != SteamVR.InitializedStates.None &&
               SteamVR.active;
    }
}