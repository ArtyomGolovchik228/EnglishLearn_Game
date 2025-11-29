using UnityEngine;
using Valve.VR;
using System.Collections;

public class NPCTeleportInteraction : MonoBehaviour
{
    [Header("VR References")]
    public Transform vrRig; // XR Origin или VR игрок
    public SteamVR_Action_Boolean interactAction;
    public SteamVR_Input_Sources handType;

    [Header("Teleport Settings")]
    public float teleportDistance = 2f;
    public float teleportDuration = 1f;
    public Transform teleportTarget; // Опциональная конкретная точка телепортации

    [Header("NPC Dialog System")]
    public NPCDialogSystem dialogSystem;
    public DialogScenario[] dialogScenarios;

    [Header("Editor Testing")]
    public bool enableEditorTesting = true;
    public KeyCode editorInteractKey = KeyCode.E;

    private bool isTeleporting = false;
    private bool isInteractable = true;
    private int currentScenarioIndex = 0;

    void Start()
    {
        // Автоматически находим ссылки если не установлены
        if (vrRig == null)
        {
            vrRig = FindObjectOfType<Camera>()?.transform;
            if (vrRig == null)
                vrRig = Camera.main.transform;
        }

        if (dialogSystem == null)
        {
            dialogSystem = GetComponent<NPCDialogSystem>();
        }
    }

    void Update()
    {
        // Обработка ввода для SteamVR
        if (interactAction != null && interactAction.GetStateDown(handType) && IsInVR())
        {
            TryInteractWithNPC();
        }

        // Обработка ввода для редактора
        if (enableEditorTesting && Input.GetKeyDown(editorInteractKey))
        {
            TryInteractWithNPC();
        }
    }

    // Вызывается когда игрок взаимодействует с NPC (через XR Interactable или клик)
    public void OnNPCInteracted()
    {
        if (!isTeleporting && isInteractable)
        {
            StartTeleportation();
        }
    }

    private void TryInteractWithNPC()
    {
        if (isTeleporting || !isInteractable) return;

        Camera cameraToUse = IsInVR() ?
            (vrRig != null ? vrRig.GetComponent<Camera>() : null) :
            Camera.main;

        if (cameraToUse == null) return;

        // Бросок луча для выбора NPC
        Ray ray = IsInVR() ?
            new Ray(vrRig.position, vrRig.forward) :
            cameraToUse.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            NPCTeleportInteraction npc = hit.collider.GetComponent<NPCTeleportInteraction>();
            if (npc != null && npc == this) // Проверяем что это именно этот NPC
            {
                npc.OnNPCInteracted();
            }
        }
    }

    private void StartTeleportation()
    {
        StartCoroutine(TeleportAndStartDialog());
    }

    private IEnumerator TeleportAndStartDialog()
    {
        isTeleporting = true;

        // Вычисляем позицию телепортации
        Vector3 targetPosition = CalculateTeleportPosition();
        Quaternion targetRotation = CalculateTeleportRotation();

        float elapsedTime = 0f;
        Vector3 startPosition = vrRig.position;
        Quaternion startRotation = vrRig.rotation;

        // Плавная телепортация
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

        // Запускаем диалог после телепортации
        StartDialog();

        isTeleporting = false;
    }

    private Vector3 CalculateTeleportPosition()
    {
        if (teleportTarget != null)
        {
            // Используем конкретную точку телепортации
            return teleportTarget.position;
        }
        else
        {
            // Стандартная позиция перед NPC
            Vector3 targetPos = transform.position + transform.forward * teleportDistance;
            targetPos.y = vrRig.position.y; // Сохраняем высоту игрока
            return targetPos;
        }
    }

    private Quaternion CalculateTeleportRotation()
    {
        // Поворачиваем игрока лицом к NPC
        Vector3 lookDirection = (transform.position - vrRig.position).normalized;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            return Quaternion.LookRotation(lookDirection);
        }

        return vrRig.rotation;
    }

    private void StartDialog()
    {
        if (dialogSystem != null && dialogScenarios != null && dialogScenarios.Length > 0)
        {
            // Выбираем следующий сценарий (можно сделать логику выбора)
            DialogScenario scenario = dialogScenarios[currentScenarioIndex];
            dialogSystem.DialogShow(scenario);

            // Переходим к следующему сценарию (циклично)
            currentScenarioIndex = (currentScenarioIndex + 1) % dialogScenarios.Length;
        }
        else
        {
            Debug.LogWarning("Dialog system or scenarios not set up!");
        }
    }

    // Публичные методы для управления из других скриптов
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
    }

    public void ForceStartDialog(DialogScenario scenario)
    {
        if (dialogSystem != null)
        {
            dialogSystem.DialogShow(scenario);
        }
    }

    public void AddDialogScenario(DialogScenario scenario)
    {
        // Добавляем новый сценарий в массив
        System.Array.Resize(ref dialogScenarios, dialogScenarios.Length + 1);
        dialogScenarios[dialogScenarios.Length - 1] = scenario;
    }

    private bool IsInVR()
    {
        return SteamVR.initializedState != SteamVR.InitializedStates.None && SteamVR.active;
    }

    // Методы для быстрого создания сценариев через инспектор
    [ContextMenu("Add Test Monologue Scenario")]
    public void AddTestMonologue()
    {
        DialogScenario scenario = new DialogScenario
        {
            type = DialogType.Monologue,
            speakerName = gameObject.name,
            monologueLines = new string[]
            {
                "Привет! Я " + gameObject.name + ".",
                "Рад тебя видеть в нашем мире!",
                "Надеюсь, тебе здесь понравится."
            }
        };
        AddDialogScenario(scenario);
        Debug.Log("Test monologue scenario added!");
    }

    [ContextMenu("Add Test Word Choice Scenario")]
    public void AddTestWordChoice()
    {
        DialogScenario scenario = new DialogScenario
        {
            type = DialogType.WordChoice,
            speakerName = gameObject.name,
            wordChoiceSentence = "Меня зовут ___",
            correctWord = "Джон",
            wordOptions = new string[] { "Джон", "Майк", "Том", "Сэм" }
        };
        AddDialogScenario(scenario);
        Debug.Log("Test word choice scenario added!");
    }

    [ContextMenu("Add Test Item Delivery Scenario")]
    public void AddTestItemDelivery()
    {
        DialogScenario scenario = new DialogScenario
        {
            type = DialogType.ItemDelivery,
            speakerName = gameObject.name,
            requiredItem = "Sword",
            requestLines = new string[]
            {
                "Привет! Не мог бы ты помочь мне?",
                "Мне нужен новый меч для работы.",
                "Принеси мне меч и положи его на площадку."
            },
            successLines = new string[]
            {
                "Отлично! Именно этот меч мне нужен!",
                "Спасибо тебе большое!"
            },
            failureLines = new string[]
            {
                "Это не меч...",
                "Мне нужен именно меч."
            }
        };
        AddDialogScenario(scenario);
        Debug.Log("Test item delivery scenario added!");
    }

    [ContextMenu("Test Teleportation")]
    public void TestTeleportation()
    {
        if (!isTeleporting)
        {
            StartTeleportation();
        }
    }

    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        // Показываем зону телепортации
        Gizmos.color = Color.blue;
        Vector3 teleportPos = CalculateTeleportPosition();
        Gizmos.DrawWireSphere(teleportPos, 0.3f);
        Gizmos.DrawLine(transform.position, teleportPos);

        // Показываем направление взгляда после телепортации
        Gizmos.color = Color.green;
        Vector3 lookDirection = (transform.position - teleportPos).normalized;
        Gizmos.DrawRay(teleportPos, lookDirection * 2f);
    }
}