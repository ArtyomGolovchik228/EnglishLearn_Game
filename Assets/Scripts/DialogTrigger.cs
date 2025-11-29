using UnityEngine;
using Valve.VR.InteractionSystem;

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

    private DialogSystem dialogSystem;
    private bool isPlayerNear = false;
    private Transform playerTransform;

    void Start()
    {
        // Получаем компонент DialogSystem
        if (dialogSystemObject != null)
        {
            dialogSystem = dialogSystemObject.GetComponent<DialogSystem>();
        }
        else
        {
            // Ищем автоматически в сцене
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

        // Настраиваем SteamVR Interactable
        SetupSteamVRInteractable();

        // Скрываем подсказку при старте
        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    void SetupSteamVRInteractable()
    {
        // Добавляем компоненты SteamVR Interactable
        var interactable = GetComponent<Interactable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<Interactable>();
        }

        // Добавляем коллайдер для взаимодействия если его нет
        var collider = GetComponent<Collider>();
        if (collider == null)
        {
            var boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = Vector3.one * 2f;
        }
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

    // ==================== STEAMVR INTERACTION ====================

    // Вызывается когда рука приближается к объекту
    void OnHandHoverBegin(Hand hand)
    {
        Debug.Log("Рука приблизилась к объекту");
        if (showInteractionHint && interactionHint != null)
        {
            interactionHint.SetActive(true);
        }
    }

    // Вызывается когда рука убирается от объекта
    void OnHandHoverEnd(Hand hand)
    {
        Debug.Log("Рука убралась от объекта");
        if (showInteractionHint && interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }

    // Вызывается при нажатии кнопки на объекте в VR
    void HandHoverUpdate(Hand hand)
    {

    }

    // Универсальный метод для VR взаимодействия
    public void OnVRInteract()
    {
        Debug.Log("VR взаимодействие вызвано");
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
            Debug.LogError("DialogSystem не найден! Прикрепите скрипт DialogSystem к объекту.");
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
    }

    // Отладочная информация в редакторе
    void OnGUI()
    {
#if UNITY_EDITOR
        if (isPlayerNear)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "Игрок рядом с объектом");
            GUI.Label(new Rect(10, 30, 300, 20), $"Нажми {interactKey} для взаимодействия");
        }
#endif
    }
}