using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AutoCanvasCreator : EditorWindow
{
    private static string canvasName = "DialogCanvas";
    private static Color dialogPanelColor = new Color(0.1f, 0.1f, 0.4f, 0.9f);
    private static Color exercisePanelColor = new Color(0.1f, 0.4f, 0.1f, 0.9f);
    private static Color rewardPanelColor = new Color(0.4f, 0.3f, 0.1f, 0.9f);

    [MenuItem("Tools/Диалог Система/Создать все Canvas")]
    public static void ShowWindow()
    {
        GetWindow<AutoCanvasCreator>("Создатель Canvas");
    }

    void OnGUI()
    {
        GUILayout.Label("Автоматическое создание UI для диалоговой системы", EditorStyles.boldLabel);
        GUILayout.Space(10);

        canvasName = EditorGUILayout.TextField("Имя Canvas:", canvasName);

        GUILayout.Space(20);

        if (GUILayout.Button("Создать все элементы UI", GUILayout.Height(40)))
        {
            CreateAllUIElements();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Создать только Canvas"))
        {
            CreateMainCanvas();
        }

        if (GUILayout.Button("Создать все панели"))
        {
            CreateAllPanels();
        }

        if (GUILayout.Button("Создать префаб кнопки"))
        {
            CreateWordButtonPrefab();
        }

        GUILayout.Space(20);
        EditorGUILayout.HelpBox("После создания подключите ссылки в компоненте DialogSystem", MessageType.Info);
    }

    [MenuItem("GameObject/UI/Диалог Система/Создать всю систему", false, 10)]
    static void CreateCompleteSystemMenu()
    {
        CreateAllUIElements();
    }

    public static void CreateAllUIElements()
    {
        // Создаем основной Canvas
        GameObject canvas = CreateMainCanvas();

        // Создаем все панели
        CreateAllPanels(canvas);

        // Создаем префаб кнопки
        CreateWordButtonPrefab();

        UnityEngine.Debug.Log("✅ Вся UI система создана успешно!");
    }

    static GameObject CreateMainCanvas()
    {
        // Создаем Canvas
        GameObject canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Настройка Canvas Scaler
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Создаем EventSystem если его нет
        CreateEventSystem();

        return canvasObj;
    }

    static void CreateEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    static void CreateAllPanels(GameObject canvas = null)
    {
        if (canvas == null)
            canvas = GameObject.Find(canvasName) ?? CreateMainCanvas();

        CreateDialogPanel(canvas);
        CreateWordLearningPanel(canvas);
        CreateSentenceExercisePanel(canvas);
        CreateToExercisePanel(canvas);
        CreatePronunciationPanel(canvas);
        CreateTranslationChoicePanel(canvas);
        CreateRewardPanel(canvas);

        // Скрываем все панели кроме диалога
        HideAllPanels(canvas);
    }

    static GameObject CreateDialogPanel(GameObject parent)
    {
        GameObject panel = CreatePanel("DialogPanel", new Vector2(800, 300), dialogPanelColor, parent);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = new Vector2(0, -200);

        // Speaker Name Text
        CreateTMPText("SpeakerNameText", new Vector2(200, 30), new Vector2(-250, 120),
            "Гид", 24, TextAlignmentOptions.Left, panel);

        // Dialog Text
        TMP_Text dialogText = CreateTMPText("DialogText", new Vector2(700, 150), new Vector2(0, 30),
            "Привет! Здорово, что ты пришел!", 20, TextAlignmentOptions.Left, panel);
        dialogText.enableWordWrapping = true;
        dialogText.overflowMode = TextOverflowModes.Overflow;

        // Кнопки
        CreateButton("ContinueButton", new Vector2(150, 40), new Vector2(300, -110),
            "Продолжить →", panel);

        CreateButton("SkipButton", new Vector2(120, 40), new Vector2(-300, -110),
            "Пропустить", panel);

        CreateButton("CloseButton", new Vector2(40, 40), new Vector2(370, 110),
            "X", panel);

        return panel;
    }

    static GameObject CreateWordLearningPanel(GameObject parent)
    {
        GameObject panel = CreatePanel("WordLearningPanel", new Vector2(600, 400), exercisePanelColor, parent);

        // Английское слово
        TMP_Text englishText = CreateTMPText("EnglishWordText", new Vector2(200, 50), new Vector2(0, 140),
            "want", 32, TextAlignmentOptions.Center, panel);
        englishText.color = Color.blue;

        // Русское слово
        CreateTMPText("RussianWordText", new Vector2(200, 40), new Vector2(0, 80),
            "хотеть", 28, TextAlignmentOptions.Center, panel);

        // Картинка слова
        CreateUIImage("WordImage", new Vector2(150, 150), new Vector2(-180, 0), panel);

        // Кнопки
        CreateButton("SoundButton", new Vector2(180, 50), new Vector2(180, 0),
            "🔊 Произношение", panel);

        CreateButton("RememberButton", new Vector2(150, 50), new Vector2(0, -140),
            "Запомнил!", panel);

        return panel;
    }

    static GameObject CreateSentenceExercisePanel(GameObject parent)
    {
        GameObject panel = CreatePanel("SentenceExercisePanel", new Vector2(700, 500), exercisePanelColor, parent);

        // Описание упражнения
        CreateTMPText("ExerciseDescriptionText", new Vector2(600, 50), new Vector2(0, 200),
            "Составь: я хочу кушать (eat)", 22, TextAlignmentOptions.Center, panel);

        // Контейнер для кнопок слов
        CreateEmptyObject("WordButtonContainer", new Vector2(500, 150), new Vector2(0, 50), panel);

        // Поле составленного предложения
        GameObject sentenceBg = CreateUIImage("ConstructedSentenceText", new Vector2(500, 60), new Vector2(0, -50), panel);
        sentenceBg.GetComponent<UnityEngine.UI.Image>().color = new Color(0.3f, 0.3f, 0.3f);

        TMP_Text sentenceText = CreateTMPText("ConstructedSentenceText_Text", new Vector2(480, 50), new Vector2(0, -50),
            "", 24, TextAlignmentOptions.Center, panel);

        // Текст обратной связи
        CreateTMPText("SentenceFeedbackText", new Vector2(500, 40), new Vector2(0, -120),
            "", 20, TextAlignmentOptions.Center, panel);

        // Кнопки действий
        CreateButton("SubmitSentenceButton", new Vector2(120, 40), new Vector2(200, -200),
            "Проверить", panel);

        CreateButton("ResetSentenceButton", new Vector2(120, 40), new Vector2(-200, -200),
            "Сбросить", panel);

        return panel;
    }

    static GameObject CreateToExercisePanel(GameObject parent)
    {
        GameObject panel = CreatePanel("ToExercisePanel", new Vector2(600, 400), exercisePanelColor, parent);

        // Предложение с пропуском
        CreateTMPText("ToSentenceText", new Vector2(500, 60), new Vector2(0, 120),
            "i want _____ sleep", 28, TextAlignmentOptions.Center, panel);

        // Объяснение
        CreateTMPText("ToExplanationText", new Vector2(500, 80), new Vector2(0, 40),
            "", 18, TextAlignmentOptions.Center, panel);

        // Обратная связь
        CreateTMPText("ToFeedbackText", new Vector2(500, 40), new Vector2(0, -40),
            "", 20, TextAlignmentOptions.Center, panel);

        // Кнопки выбора
        Button yesBtn = CreateButton("ToYesButton", new Vector2(150, 50), new Vector2(-120, -120),
            "НУЖНА to", panel);
        yesBtn.image.color = Color.yellow;

        Button noBtn = CreateButton("ToNoButton", new Vector2(150, 50), new Vector2(120, -120),
            "НЕ нужна to", panel);
        noBtn.image.color = Color.yellow;

        return panel;
    }

    static GameObject CreatePronunciationPanel(GameObject parent)
    {
        GameObject panel = CreatePanel("PronunciationPanel", new Vector2(600, 450), exercisePanelColor, parent);

        // Английская фраза
        CreateTMPText("PronunciationPhraseText", new Vector2(500, 50), new Vector2(0, 160),
            "I want coffee", 26, TextAlignmentOptions.Center, panel);

        // Перевод
        TMP_Text translationText = CreateTMPText("PronunciationTranslationText", new Vector2(500, 40), new Vector2(0, 100),
            "Я хочу кофе", 20, TextAlignmentOptions.Center, panel);
        translationText.color = Color.gray;

        // Статус записи
        CreateTMPText("RecordingStatusText", new Vector2(500, 30), new Vector2(0, 40),
            "Готов к записи", 18, TextAlignmentOptions.Center, panel);

        // Слайдер точности
        CreateSlider("AccuracySlider", new Vector2(400, 30), new Vector2(0, 0), panel);

        // Обратная связь
        CreateTMPText("PronunciationFeedbackText", new Vector2(500, 40), new Vector2(0, -40),
            "Нажми 'Начать запись'", 16, TextAlignmentOptions.Center, panel);

        // Кнопки записи
        Button startBtn = CreateButton("StartRecordingButton", new Vector2(160, 50), new Vector2(-120, -120),
            "🎤 Начать запись", panel);
        startBtn.image.color = Color.red;

        Button stopBtn = CreateButton("StopRecordingButton", new Vector2(120, 50), new Vector2(120, -120),
            "⏹ Стоп", panel);
        stopBtn.image.color = Color.green;
        stopBtn.interactable = false;

        return panel;
    }

    static GameObject CreateTranslationChoicePanel(GameObject parent)
    {
        GameObject panel = CreatePanel("TranslationChoicePanel", new Vector2(600, 500), exercisePanelColor, parent);

        // Вопрос
        CreateTMPText("ChoiceQuestionText", new Vector2(500, 50), new Vector2(0, 200),
            "Выбери правильный перевод для:", 22, TextAlignmentOptions.Center, panel);

        // Картинка слова
        CreateUIImage("ChoiceWordImage", new Vector2(100, 100), new Vector2(0, 100), panel);

        // Кнопки выбора (4 штуки)
        CreateButton("ChoiceButton1", new Vector2(200, 60), new Vector2(-150, 0), "to want", panel);
        CreateButton("ChoiceButton2", new Vector2(200, 60), new Vector2(150, 0), "want", panel);
        CreateButton("ChoiceButton3", new Vector2(200, 60), new Vector2(-150, -80), "wanted", panel);
        CreateButton("ChoiceButton4", new Vector2(200, 60), new Vector2(150, -80), "wanting", panel);

        // Обратная связь
        CreateTMPText("ChoiceFeedbackText", new Vector2(500, 40), new Vector2(0, -180),
            "", 20, TextAlignmentOptions.Center, panel);

        return panel;
    }

    static GameObject CreateRewardPanel(GameObject parent)
    {
        GameObject panel = CreatePanel("RewardPanel", new Vector2(500, 300), rewardPanelColor, parent);

        // Текст награды
        CreateTMPText("RewardText", new Vector2(450, 120), new Vector2(0, 50),
            "Поздравляем!\n+50 опыта\n+25 монет", 28, TextAlignmentOptions.Center, panel);

        // Опыт
        TMP_Text xpText = CreateTMPText("XPText", new Vector2(200, 30), new Vector2(-150, -100),
            "Опыт: 150", 20, TextAlignmentOptions.Left, panel);
        xpText.color = Color.green;

        // Монеты
        TMP_Text coinsText = CreateTMPText("CoinsText", new Vector2(200, 30), new Vector2(150, -100),
            "Монеты: 75", 20, TextAlignmentOptions.Right, panel);
        coinsText.color = Color.yellow;

        return panel;
    }

    static void CreateWordButtonPrefab()
    {
        // Создаем временный Canvas для префаба
        GameObject tempCanvas = new GameObject("TempCanvas");
        Canvas canvas = tempCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Создаем кнопку для префаба - исправленная строка
        Button button = CreateButton("WordButton", new Vector2(120, 50), Vector2.zero, "word", tempCanvas);
        GameObject buttonObj = button.gameObject; // Получаем GameObject из Button

        // Сохраняем как префаб
        string path = "Assets/WordButtonPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(buttonObj, path);
        DestroyImmediate(tempCanvas);

        UnityEngine.Debug.Log("✅ Префаб кнопки создан: " + path);
    }

    #region Helper Methods

    static GameObject CreatePanel(string name, Vector2 size, Color color, GameObject parent)
    {
        GameObject panel = new GameObject(name);
        RectTransform rect = panel.AddComponent<RectTransform>();
        panel.AddComponent<CanvasRenderer>();
        UnityEngine.UI.Image image = panel.AddComponent<UnityEngine.UI.Image>();

        image.color = color;
        rect.sizeDelta = size;

        if (parent != null)
            panel.transform.SetParent(parent.transform, false);

        return panel;
    }

    static TMP_Text CreateTMPText(string name, Vector2 size, Vector2 position, string text, int fontSize,
        TextAlignmentOptions alignment, GameObject parent)
    {
        GameObject textObj = new GameObject(name);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        textObj.AddComponent<CanvasRenderer>();

        TMP_Text tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = alignment;
        tmpText.color = Color.white;
        tmpText.fontStyle = FontStyles.Normal;

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        if (parent != null)
            textObj.transform.SetParent(parent.transform, false);

        return tmpText;
    }

    static Button CreateButton(string name, Vector2 size, Vector2 position, string buttonText, GameObject parent)
    {
        GameObject buttonObj = new GameObject(name);
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        buttonObj.AddComponent<CanvasRenderer>();
        UnityEngine.UI.Image image = buttonObj.AddComponent<UnityEngine.UI.Image>();
        Button button = buttonObj.AddComponent<Button>();

        // Настройка внешнего вида кнопки
        image.color = new Color(0.2f, 0.4f, 0.8f); // Синий цвет

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        // Добавляем текст если нужно
        if (!string.IsNullOrEmpty(buttonText))
        {
            GameObject textObj = new GameObject("Text");
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textObj.AddComponent<CanvasRenderer>();
            TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();

            text.text = buttonText;
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            textRect.sizeDelta = size;
            textRect.anchoredPosition = Vector2.zero;

            textObj.transform.SetParent(buttonObj.transform, false);
        }

        if (parent != null)
            buttonObj.transform.SetParent(parent.transform, false);

        return button;
    }

    static GameObject CreateUIImage(string name, Vector2 size, Vector2 position, GameObject parent)
    {
        GameObject imageObj = new GameObject(name);
        RectTransform rect = imageObj.AddComponent<RectTransform>();
        imageObj.AddComponent<CanvasRenderer>();
        UnityEngine.UI.Image image = imageObj.AddComponent<UnityEngine.UI.Image>();

        image.color = Color.white;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        if (parent != null)
            imageObj.transform.SetParent(parent.transform, false);

        return imageObj;
    }

    static Slider CreateSlider(string name, Vector2 size, Vector2 position, GameObject parent)
    {
        GameObject sliderObj = new GameObject(name);
        RectTransform rect = sliderObj.AddComponent<RectTransform>();
        sliderObj.AddComponent<CanvasRenderer>();
        Slider slider = sliderObj.AddComponent<Slider>();
        UnityEngine.UI.Image bg = sliderObj.AddComponent<UnityEngine.UI.Image>();

        bg.color = new Color(0.2f, 0.2f, 0.2f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        // Настройка слайдера
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;

        // Создаем Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.sizeDelta = new Vector2(size.x - 10, size.y - 10);
        fillArea.transform.SetParent(sliderObj.transform, false);

        // Создаем Fill
        GameObject fill = new GameObject("Fill");
        UnityEngine.UI.Image fillImage = fill.AddComponent<UnityEngine.UI.Image>();
        fillImage.color = Color.green;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.sizeDelta = new Vector2(10, 0);

        fill.transform.SetParent(fillArea.transform, false);

        if (parent != null)
            sliderObj.transform.SetParent(parent.transform, false);

        return slider;
    }

    static GameObject CreateEmptyObject(string name, Vector2 size, Vector2 position, GameObject parent)
    {
        GameObject obj = new GameObject(name);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        if (parent != null)
            obj.transform.SetParent(parent.transform, false);

        return obj;
    }

    static void HideAllPanels(GameObject canvas)
    {
        foreach (Transform child in canvas.transform)
        {
            if (child.name.EndsWith("Panel") && child.name != "DialogPanel")
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    #endregion
}