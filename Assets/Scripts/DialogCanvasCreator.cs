using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogCanvasCreator : MonoBehaviour
{
    [Header("Canvas Settings")]
    public string canvasName = "NPCDialogCanvas";
    public Vector3 canvasPosition = new Vector3(0, 2, 3);
    public Vector3 canvasScale = new Vector3(0.01f, 0.01f, 0.01f);
    public int sortingOrder = 10;

    [ContextMenu("Create Dialog Canvas")]
    public void CreateDialogCanvas()
    {
        // Создаем Canvas
        GameObject canvasGO = new GameObject(canvasName);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();

        // Настраиваем Canvas
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = sortingOrder;
        canvasGO.transform.position = canvasPosition;
        canvasGO.transform.localScale = canvasScale;

        // Настраиваем Canvas Scaler
        scaler.dynamicPixelsPerUnit = 1;
        scaler.referencePixelsPerUnit = 100;

        // Создаем все UI элементы
        CreateBackground(canvasGO);
        CreateSpeakerText(canvasGO);
        CreateDialogText(canvasGO);
        CreateContinueButton(canvasGO);
        CreateWordChoicePanel(canvasGO);
        CreateSentenceBuilderPanel(canvasGO);
        CreateItemSlotUI(canvasGO);

        // Добавляем NPCDialogSystem компонент и настраиваем ссылки
        SetupDialogSystem(canvasGO);

        Debug.Log($"Dialog Canvas '{canvasName}' created successfully!");
    }

    private void CreateBackground(GameObject parent)
    {
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(parent.transform);
        bgGO.transform.localPosition = Vector3.zero;
        bgGO.transform.localScale = Vector3.one;

        Image image = bgGO.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        RectTransform rect = bgGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, 200);
    }

    private void CreateSpeakerText(GameObject parent)
    {
        GameObject speakerGO = new GameObject("SpeakerText");
        speakerGO.transform.SetParent(parent.transform);
        speakerGO.transform.localPosition = new Vector3(0, 70, 0);

        TextMeshProUGUI text = speakerGO.AddComponent<TextMeshProUGUI>();
        text.text = "Спикер";
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = speakerGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 40);
    }

    private void CreateDialogText(GameObject parent)
    {
        GameObject dialogGO = new GameObject("DialogText");
        dialogGO.transform.SetParent(parent.transform);
        dialogGO.transform.localPosition = new Vector3(0, 20, 0);

        TextMeshProUGUI text = dialogGO.AddComponent<TextMeshProUGUI>();
        text.text = "Текст диалога...";
        text.fontSize = 20;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;

        RectTransform rect = dialogGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(550, 100);
    }

    private void CreateContinueButton(GameObject parent)
    {
        GameObject buttonGO = new GameObject("ContinueButton");
        buttonGO.transform.SetParent(parent.transform);
        buttonGO.transform.localPosition = new Vector3(0, -70, 0);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.8f, 1f);

        Button button = buttonGO.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;

        // Настраиваем colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 1f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.6f, 1f);
        button.colors = colors;

        // Текст кнопки
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);
        textGO.transform.localPosition = Vector3.zero;

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = "Продолжить";
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150, 40);

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(150, 40);
    }

    private void CreateWordChoicePanel(GameObject parent)
    {
        GameObject panelGO = new GameObject("WordChoicePanel");
        panelGO.transform.SetParent(parent.transform);
        panelGO.transform.localPosition = new Vector3(0, -30, 0);
        panelGO.SetActive(false);

        Image image = panelGO.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);

        RectTransform rect = panelGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500, 80);

        // Создаем 4 кнопки выбора
        for (int i = 0; i < 4; i++)
        {
            CreateWordChoiceButton(panelGO, i);
        }
    }

    private void CreateWordChoiceButton(GameObject parent, int index)
    {
        float xPos = -150 + (index * 100);

        GameObject buttonGO = new GameObject($"WordChoiceButton_{index}");
        buttonGO.transform.SetParent(parent.transform);
        buttonGO.transform.localPosition = new Vector3(xPos, 0, 0);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.5f, 1f);

        Button button = buttonGO.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.5f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.7f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.4f, 1f);
        button.colors = colors;

        // Текст кнопки
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);
        textGO.transform.localPosition = Vector3.zero;

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = $"Вариант {index + 1}";
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 40);

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(80, 40);
    }

    private void CreateSentenceBuilderPanel(GameObject parent)
    {
        GameObject panelGO = new GameObject("SentenceBuilderPanel");
        panelGO.transform.SetParent(parent.transform);
        panelGO.transform.localPosition = new Vector3(0, -50, 0);
        panelGO.SetActive(false);

        Image image = panelGO.AddComponent<Image>();
        image.color = new Color(0.1f, 0.2f, 0.1f, 0.8f);

        RectTransform rect = panelGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(550, 150);

        // Создаем панель слов
        CreateWordButtonsPanel(panelGO);

        // Создаем панель предложения
        CreateSentenceSlotsPanel(panelGO);

        // Создаем кнопки управления
        CreateControlButtons(panelGO);
    }

    private void CreateWordButtonsPanel(GameObject parent)
    {
        GameObject wordsPanel = new GameObject("WordButtonsPanel");
        wordsPanel.transform.SetParent(parent.transform);
        wordsPanel.transform.localPosition = new Vector3(0, 40, 0);

        RectTransform rect = wordsPanel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500, 40);

        // Создаем 6 кнопок слов
        for (int i = 0; i < 6; i++)
        {
            CreateWordButton(wordsPanel, i);
        }
    }

    private void CreateWordButton(GameObject parent, int index)
    {
        float xPos = -250 + (index * 100);

        GameObject buttonGO = new GameObject($"WordButton_{index}");
        buttonGO.transform.SetParent(parent.transform);
        buttonGO.transform.localPosition = new Vector3(xPos, 0, 0);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.2f, 1f);

        Button button = buttonGO.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.4f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.6f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.1f, 1f);
        button.colors = colors;

        // Текст кнопки
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);
        textGO.transform.localPosition = Vector3.zero;

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = $"Слово {index + 1}";
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 30);
    }

    private void CreateSentenceSlotsPanel(GameObject parent)
    {
        GameObject slotsPanel = new GameObject("SentenceSlotsPanel");
        slotsPanel.transform.SetParent(parent.transform);
        slotsPanel.transform.localPosition = new Vector3(0, 0, 0);

        RectTransform rect = slotsPanel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500, 40);

        // Создаем 4 слота для предложения
        for (int i = 0; i < 4; i++)
        {
            CreateSentenceSlot(slotsPanel, i);
        }
    }

    private void CreateSentenceSlot(GameObject parent, int index)
    {
        float xPos = -150 + (index * 100);

        GameObject slotGO = new GameObject($"SentenceSlot_{index}");
        slotGO.transform.SetParent(parent.transform);
        slotGO.transform.localPosition = new Vector3(xPos, 0, 0);

        Image image = slotGO.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.5f, 0.5f);

        Button button = slotGO.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.5f, 0.5f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.7f, 0.7f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.4f, 0.7f);
        button.colors = colors;

        // Текст слота
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(slotGO.transform);
        textGO.transform.localPosition = Vector3.zero;

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = slotGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 30);
    }

    private void CreateControlButtons(GameObject parent)
    {
        GameObject controlsPanel = new GameObject("ControlButtons");
        controlsPanel.transform.SetParent(parent.transform);
        controlsPanel.transform.localPosition = new Vector3(0, -40, 0);

        RectTransform rect = controlsPanel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 40);

        // Кнопка проверки
        CreateSubmitButton(controlsPanel, -75);

        // Кнопка очистки
        CreateClearButton(controlsPanel, 75);
    }

    private void CreateSubmitButton(GameObject parent, float xPos)
    {
        GameObject buttonGO = new GameObject("SubmitButton");
        buttonGO.transform.SetParent(parent.transform);
        buttonGO.transform.localPosition = new Vector3(xPos, 0, 0);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 0.2f, 1f);

        Button button = buttonGO.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.1f, 1f);
        button.colors = colors;

        // Текст кнопки
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);
        textGO.transform.localPosition = Vector3.zero;

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = "Проверить";
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120, 30);
    }

    private void CreateClearButton(GameObject parent, float xPos)
    {
        GameObject buttonGO = new GameObject("ClearButton");
        buttonGO.transform.SetParent(parent.transform);
        buttonGO.transform.localPosition = new Vector3(xPos, 0, 0);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.6f, 0.2f, 0.2f, 1f);

        Button button = buttonGO.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.6f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.8f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.4f, 0.1f, 0.1f, 1f);
        button.colors = colors;

        // Текст кнопки
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);
        textGO.transform.localPosition = Vector3.zero;

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = "Очистить";
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120, 30);
    }

    private void CreateItemSlotUI(GameObject parent)
    {
        GameObject itemSlotGO = new GameObject("ItemSlotUI");
        itemSlotGO.transform.SetParent(parent.transform);
        itemSlotGO.transform.localPosition = new Vector3(200, 0, 0);
        itemSlotGO.SetActive(false);

        Image image = itemSlotGO.AddComponent<Image>();
        image.color = new Color(0.5f, 0.5f, 0.1f, 0.8f);

        RectTransform rect = itemSlotGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 80);

        // Иконка предмета
        GameObject iconGO = new GameObject("ItemIcon");
        iconGO.transform.SetParent(itemSlotGO.transform);
        iconGO.transform.localPosition = Vector3.zero;

        TextMeshProUGUI text = iconGO.AddComponent<TextMeshProUGUI>();
        text.text = "📦";
        text.fontSize = 32;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(80, 80);
    }

    private void SetupDialogSystem(GameObject canvasGO)
    {
        // Добавляем NPCDialogSystem к текущему объекту
        NPCDialogSystem dialogSystem = gameObject.AddComponent<NPCDialogSystem>();

        // Настраиваем все ссылки
        dialogSystem.dialogCanvas = canvasGO.GetComponent<Canvas>();
        dialogSystem.speakerText = FindComponent<TextMeshProUGUI>(canvasGO, "SpeakerText");
        dialogSystem.dialogText = FindComponent<TextMeshProUGUI>(canvasGO, "DialogText");
        dialogSystem.continueButton = FindComponent<Button>(canvasGO, "ContinueButton");

        dialogSystem.wordChoicePanel = FindGameObject(canvasGO, "WordChoicePanel");
        dialogSystem.wordChoiceButtons = FindComponentsInChildren<Button>(canvasGO, "WordChoiceButton");

        dialogSystem.sentenceBuilderPanel = FindGameObject(canvasGO, "SentenceBuilderPanel");
        dialogSystem.wordButtons = FindComponentsInChildren<Button>(canvasGO, "WordButton");
        dialogSystem.sentenceSlots = FindComponentsInChildren<Button>(canvasGO, "SentenceSlot");
        dialogSystem.submitSentenceButton = FindComponent<Button>(canvasGO, "SubmitButton");
        dialogSystem.clearSentenceButton = FindComponent<Button>(canvasGO, "ClearButton");

        dialogSystem.itemSlotUI = FindGameObject(canvasGO, "ItemSlotUI");

        Debug.Log("NPCDialogSystem configured with all UI references!");
    }

    private T FindComponent<T>(GameObject parent, string name) where T : Component
    {
        Transform child = parent.transform.Find(name);
        return child != null ? child.GetComponent<T>() : null;
    }

    private GameObject FindGameObject(GameObject parent, string name)
    {
        Transform child = parent.transform.Find(name);
        return child != null ? child.gameObject : null;
    }

    private Button[] FindComponentsInChildren<Button>(GameObject parent, string namePrefix)
    {
        List<Button> buttons = new List<Button>();
        foreach (Transform child in parent.transform)
        {
            if (child.name.StartsWith(namePrefix))
            {
                Button button = child.GetComponent<Button>();
                if (button != null) buttons.Add(button);
            }
        }
        return buttons.ToArray();
    }
}