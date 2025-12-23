using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DialogTrigger : MonoBehaviour
{
    [Header("Ссылка на диалоговую систему")]
    public GameObject dialogSystemObject;

    [Header("Настройки взаимодействия")]
    public bool showInteractionHint = true;
    public GameObject interactionHint;
    public float hintShowDistance = 3f;

    [Header("Настройки ввода")]
    public KeyCode interactKey = KeyCode.E;
    public bool enableMouseClick = true;
    public bool enableKeyPress = true;

    [Header("XR Interaction Toolkit Настройки")]
    public XRSimpleInteractable xrInteractable;
    public float interactDistance = 2f;

    private DialogSystem dialogSystem;
    private bool isPlayerNear = false;
    private Transform playerTransform;
    private IXRInteractor currentInteractor;

    void Start()
    {
        // Получаем компонент DialogSystem
        if (dialogSystemObject != null)
        {
            dialogSystem = dialogSystemObject.GetComponent<DialogSystem>();
        }
        else
        {
            dialogSystem = FindObjectOfType<DialogSystem>();
        }

        if (dialogSystem == null)
        {
            Debug.LogError("DialogSystem не найден в сцене!");
        }

        // Находим игрока
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogWarning("Игрок с тегом 'Player' не найден!");
        }

        // Настраиваем XR Interaction Toolkit Interactable
        SetupXRInteractable();

        // Скрываем подсказку при старте
        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    void SetupXRInteractable()
    {
        // Если XR Interactable уже есть, используем его
        if (xrInteractable == null)
        {
            xrInteractable = GetComponent<XRSimpleInteractable>();
        }

        // Создаем новый если нет
        if (xrInteractable == null)
        {
            xrInteractable = gameObject.AddComponent<XRSimpleInteractable>();

            // Настройки взаимодействия
            xrInteractable.interactionLayers = InteractionLayerMask.GetMask("Default");
            xrInteractable.selectMode = InteractableSelectMode.Single;
        }

        // Добавляем коллайдер для взаимодействия если его нет
        // Для XR Interaction Toolkit:
        // - Ray Interactor может работать с триггерами (через raycast)
        // - Direct Interactor нужен обычный коллайдер (физический контакт)
        // Для PC взаимодействия нужен триггер коллайдер
        var collider = GetComponent<Collider>();
        if (collider == null)
        {
            // Создаем коллайдер как триггер для PC взаимодействия
            // XR Ray Interactor все равно сможет с ним работать
            var sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true; // Триггер для PC взаимодействия
            sphereCollider.radius = interactDistance;
        }
        // Если коллайдер уже есть, используем его как есть
        // XR Interaction Toolkit будет работать с любым коллайдером

        // Подписываемся на события XR Interaction Toolkit
        xrInteractable.hoverEntered.AddListener(OnXRHoverEnter);
        xrInteractable.hoverExited.AddListener(OnXRHoverExit);
        xrInteractable.selectEntered.AddListener(OnXRSelect);
        xrInteractable.selectExited.AddListener(OnXRSelectExit);
        // Добавляем обработку активации (кнопка активации на контроллере)
        xrInteractable.activated.AddListener(OnXRActivated);
    }

    void Update()
    {
        // Показ подсказки при приближении (для PC)
        if (showInteractionHint && interactionHint != null && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool shouldShowHint = distance <= hintShowDistance && !IsDialogActive();

            if (interactionHint.activeSelf != shouldShowHint)
                interactionHint.SetActive(shouldShowHint);
        }

        // Взаимодействие по клавише E (для PC)
        if (enableKeyPress && isPlayerNear && Input.GetKeyDown(interactKey) && !IsDialogActive())
        {
            StartDialog();
        }

        // Альтернативное взаимодействие - клавиша F
        if (enableKeyPress && isPlayerNear && Input.GetKeyDown(KeyCode.F) && !IsDialogActive())
        {
            StartDialog();
        }
    }

    // ==================== XR INTERACTION TOOLKIT ====================

    void OnXRHoverEnter(HoverEnterEventArgs args)
    {
        Debug.Log("XR контроллер приблизился к объекту");
        currentInteractor = args.interactorObject;

        if (showInteractionHint && interactionHint != null && !IsDialogActive())
        {
            interactionHint.SetActive(true);
        }
    }

    void OnXRHoverExit(HoverExitEventArgs args)
    {
        Debug.Log("XR контроллер убрался от объекта");
        currentInteractor = null;

        if (showInteractionHint && interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }

    void OnXRSelect(SelectEnterEventArgs args)
    {
        Debug.Log("XR взаимодействие вызвано (Select/Trigger нажата)");
        if (!IsDialogActive())
        {
            StartDialog();
        }
    }

    void OnXRSelectExit(SelectExitEventArgs args)
    {
        Debug.Log("XR Select завершено");
    }

    void OnXRActivated(ActivateEventArgs args)
    {
        Debug.Log("XR активация вызвана (Activate кнопка нажата)");
        if (!IsDialogActive())
        {
            StartDialog();
        }
    }

    // Универсальный метод для XR взаимодействия
    public void OnXRInteract()
    {
        Debug.Log("XR взаимодействие вызвано через публичный метод");
        if (!IsDialogActive())
        {
            StartDialog();
        }
    }


    // ==================== PC INTERACTION ====================

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Объект вошел в триггер: {other.name}, тег: {other.tag}");
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (showInteractionHint && interactionHint != null)
            {
                interactionHint.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"Объект вышел из триггера: {other.name}, тег: {other.tag}");
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (showInteractionHint && interactionHint != null)
            {
                interactionHint.SetActive(false);
            }
        }
    }

    // Клик мышью (для PC)
    void OnMouseDown()
    {
        Debug.Log("Клик мышью по объекту");
        if (enableMouseClick && !IsDialogActive())
        {
            StartDialog();
        }
    }

    // ==================== DIALOG MANAGEMENT ====================

    void StartDialog()
    {
        Debug.Log("Попытка запуска диалога");
        if (dialogSystem != null)
        {
            dialogSystem.StartDialog();

            // Скрываем подсказку когда диалог активен
            if (interactionHint != null)
                interactionHint.SetActive(false);

            Debug.Log("Диалог успешно запущен");
        }
        else
        {
            Debug.LogError("DialogSystem не найден!");
        }
    }

    bool IsDialogActive()
    {
        bool isActive = dialogSystem != null && dialogSystem.IsDialogActive();
        Debug.Log($"Диалог активен: {isActive}");
        return isActive;
    }

    // Публичный метод для принудительного запуска диалога
    public void ForceStartDialog()
    {
        if (dialogSystem != null)
        {
            dialogSystem.StartDialog();
        }
    }

    // ==================== EDITOR VISUALIZATION ====================

    // Отображение радиуса взаимодействия в редакторе
    void OnDrawGizmosSelected()
    {
        if (showInteractionHint)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, hintShowDistance);
        }

        // Отображение радиуса XR взаимодействия
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }

    void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        if (xrInteractable != null)
        {
            xrInteractable.hoverEntered.RemoveListener(OnXRHoverEnter);
            xrInteractable.hoverExited.RemoveListener(OnXRHoverExit);
            xrInteractable.selectEntered.RemoveListener(OnXRSelect);
            xrInteractable.selectExited.RemoveListener(OnXRSelectExit);
            xrInteractable.activated.RemoveListener(OnXRActivated);
        }
    }
}