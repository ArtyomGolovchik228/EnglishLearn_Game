using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class DialogCanvasGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/Диалоги XR/Создать систему диалогов (оптимизированную)", false, 10)]
    public static void CreateFullDialogSystem()
    {
        // Создаем основной объект
        GameObject dialogSystem = new GameObject("DialogSystem_XR");
        var dialogScript = dialogSystem.AddComponent<DialogSystem>();

        // Создаем и настраиваем канвасы с реалистичным масштабом для VR
        CreateMainCanvas(dialogSystem, dialogScript);
        CreateWordLearningCanvas(dialogSystem, dialogScript);
        CreateSentenceExerciseCanvas(dialogSystem, dialogScript);
        CreateToExerciseCanvas(dialogSystem, dialogScript);
        CreatePronunciationCanvas(dialogSystem, dialogScript);
        CreateTranslationChoiceCanvas(dialogSystem, dialogScript);
        CreateRewardCanvas(dialogSystem, dialogScript);

        // Заполняем списки примерами
        SetupExampleData(dialogScript);

        // Добавляем подсказку для взаимодействия
        CreateInteractionHint(dialogSystem, dialogScript);

        Selection.activeGameObject = dialogSystem;
        Debug.Log("Оптимизированная система диалогов для XR создана!");
    }

    static void CreateMainCanvas(GameObject parent, DialogSystem script)
    {
        // Создаем канвас с реалистичным масштабом
        GameObject canvasObj = new GameObject("MainDialogCanvas");
        canvasObj.transform.SetParent(parent.transform);
        canvasObj.transform.localPosition = new Vector3(0, 1.2f, 1.5f); // 1.5 метра от игрока
        canvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f); // Реалистичный масштаб

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

        canvas.renderMode = RenderMode.WorldSpace;
        scaler.dynamicPixelsPerUnit = 10;

        // Фон панели - 1.5м х 0.5м (3:1) в реальном мире
        GameObject panel = CreatePanel(canvasObj.transform, "DialogPanel");
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(750, 250); // В пикселях (соответствует ~1.5м x 0.5м)

        // Заголовок с именем говорящего
        GameObject namePanel = CreatePanel(panel.transform, "NamePanel");
        RectTransform nameRT = namePanel.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 0.8f);
        nameRT.anchorMax = new Vector2(1, 1);
        nameRT.anchoredPosition = Vector2.zero;
        nameRT.sizeDelta = Vector2.zero;
        namePanel.GetComponent<Image>().color = new Color(0.3f, 0.5f, 0.8f, 1);

        GameObject nameTextObj = new GameObject("SpeakerName");
        nameTextObj.transform.SetParent(namePanel.transform);
        TMP_Text nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        RectTransform nameTextRT = nameTextObj.GetComponent<RectTransform>();
        SetupRectTransform(nameTextRT, Vector2.zero, Vector2.one, Vector2.zero);
        nameText.text = "Гид";
        nameText.fontSize = 32; // Оптимальный размер для VR
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.verticalAlignment = VerticalAlignmentOptions.Middle;

        // Текст диалога
        GameObject dialogTextObj = new GameObject("DialogText");
        dialogTextObj.transform.SetParent(panel.transform);
        TMP_Text dialogText = dialogTextObj.AddComponent<TextMeshProUGUI>();
        RectTransform dialogTextRT = dialogTextObj.GetComponent<RectTransform>();
        dialogTextRT.anchorMin = new Vector2(0.05f, 0.15f);
        dialogTextRT.anchorMax = new Vector2(0.95f, 0.75f);
        dialogTextRT.anchoredPosition = Vector2.zero;
        dialogTextRT.sizeDelta = Vector2.zero;
        dialogText.text = "Привет! Готов учить английский?";
        dialogText.fontSize = 28; // Читаемый размер
        dialogText.fontStyle = FontStyles.Bold;
        dialogText.color = Color.black;
        dialogText.alignment = TextAlignmentOptions.Left;
        dialogText.verticalAlignment = VerticalAlignmentOptions.Top;
        dialogText.enableWordWrapping = true;
        dialogText.lineSpacing = 15;

        // Кнопки
        GameObject buttonsPanel = CreatePanel(panel.transform, "ButtonsPanel");
        buttonsPanel.GetComponent<Image>().color = new Color(1, 1, 1, 0);
        RectTransform buttonsRT = buttonsPanel.GetComponent<RectTransform>();
        buttonsRT.anchorMin = new Vector2(0, 0);
        buttonsRT.anchorMax = new Vector2(1, 0.15f);
        buttonsRT.anchoredPosition = Vector2.zero;
        buttonsRT.sizeDelta = Vector2.zero;

        // Кнопка продолжить
        GameObject continueBtn = CreateButton(buttonsPanel.transform, "ContinueButton");
        RectTransform continueRT = continueBtn.GetComponent<RectTransform>();
        continueRT.anchorMin = new Vector2(0.7f, 0.1f);
        continueRT.anchorMax = new Vector2(0.9f, 0.9f);
        continueRT.anchoredPosition = Vector2.zero;
        continueRT.sizeDelta = Vector2.zero;
        TMP_Text continueText = continueBtn.GetComponentInChildren<TMP_Text>();
        continueText.text = "Далее";
        continueText.fontSize = 24;
        continueText.fontStyle = FontStyles.Bold;

        // Кнопка пропустить
        GameObject skipBtn = CreateButton(buttonsPanel.transform, "SkipButton");
        RectTransform skipRT = skipBtn.GetComponent<RectTransform>();
        skipRT.anchorMin = new Vector2(0.45f, 0.1f);
        skipRT.anchorMax = new Vector2(0.65f, 0.9f);
        skipRT.anchoredPosition = Vector2.zero;
        skipRT.sizeDelta = Vector2.zero;
        TMP_Text skipText = skipBtn.GetComponentInChildren<TMP_Text>();
        skipText.text = "Пропустить";
        skipText.fontSize = 22;

        // Кнопка закрыть
        GameObject closeBtn = CreateButton(buttonsPanel.transform, "CloseButton");
        RectTransform closeRT = closeBtn.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(0.1f, 0.1f);
        closeRT.anchorMax = new Vector2(0.3f, 0.9f);
        closeRT.anchoredPosition = Vector2.zero;
        closeRT.sizeDelta = Vector2.zero;
        TMP_Text closeText = closeBtn.GetComponentInChildren<TMP_Text>();
        closeText.text = "Закрыть";
        closeText.fontSize = 24;
        closeText.fontStyle = FontStyles.Bold;

        // Изображение персонажа
        GameObject characterImageObj = new GameObject("CharacterImage");
        characterImageObj.transform.SetParent(panel.transform);
        Image characterImage = characterImageObj.AddComponent<Image>();
        RectTransform charRT = characterImageObj.GetComponent<RectTransform>();
        charRT.anchorMin = new Vector2(0.75f, 0.2f);
        charRT.anchorMax = new Vector2(0.95f, 0.75f);
        charRT.anchoredPosition = Vector2.zero;
        charRT.sizeDelta = Vector2.zero;
        characterImage.color = new Color(0.9f, 0.9f, 0.9f, 1);

        // Подключаем к скрипту
        script.dialogPanel = panel;
        script.dialogText = dialogText;
        script.speakerNameText = nameText;
        script.characterImage = characterImage;
        script.continueButton = continueBtn.GetComponent<Button>();
        script.skipButton = skipBtn.GetComponent<Button>();
        script.closeButton = closeBtn.GetComponent<Button>();
    }

    static void CreateWordLearningCanvas(GameObject parent, DialogSystem script)
    {
        GameObject canvasObj = new GameObject("WordLearningCanvas");
        canvasObj.transform.SetParent(parent.transform);
        canvasObj.transform.localPosition = new Vector3(1.2f, 1.2f, 1.5f);
        canvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        SetupCanvas(canvasObj);

        GameObject panel = CreatePanel(canvasObj.transform, "WordLearningPanel");
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(600, 500);

        // Английское слово
        GameObject engWordObj = CreateText(panel.transform, "EnglishWord",
            "want", 48, new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.9f));
        engWordObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
        engWordObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

        // Русское слово
        GameObject rusWordObj = CreateText(panel.transform, "RussianWord",
            "хотеть", 36, new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.7f));
        rusWordObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
        rusWordObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

        // Изображение слова
        GameObject wordImageObj = new GameObject("WordImage");
        wordImageObj.transform.SetParent(panel.transform);
        Image wordImage = wordImageObj.AddComponent<Image>();
        RectTransform imageRT = wordImageObj.GetComponent<RectTransform>();
        imageRT.anchorMin = new Vector2(0.2f, 0.1f);
        imageRT.anchorMax = new Vector2(0.8f, 0.45f);
        imageRT.anchoredPosition = Vector2.zero;
        imageRT.sizeDelta = Vector2.zero;
        wordImage.color = new Color(0.8f, 0.9f, 1, 1);

        // Кнопка звука
        GameObject soundBtn = CreateButton(panel.transform, "SoundButton");
        TMP_Text soundText = soundBtn.GetComponentInChildren<TMP_Text>();
        soundText.text = "🔊 Звук";
        soundText.fontSize = 20;
        RectTransform soundRT = soundBtn.GetComponent<RectTransform>();
        soundRT.anchorMin = new Vector2(0.1f, 0.05f);
        soundRT.anchorMax = new Vector2(0.4f, 0.15f);
        soundRT.sizeDelta = new Vector2(0, 40);

        // Кнопка "Запомнил"
        GameObject rememberBtn = CreateButton(panel.transform, "RememberButton");
        TMP_Text rememberText = rememberBtn.GetComponentInChildren<TMP_Text>();
        rememberText.text = "Запомнил";
        rememberText.fontSize = 22;
        rememberText.fontStyle = FontStyles.Bold;
        RectTransform rememberRT = rememberBtn.GetComponent<RectTransform>();
        rememberRT.anchorMin = new Vector2(0.5f, 0.05f);
        rememberRT.anchorMax = new Vector2(0.9f, 0.15f);
        rememberRT.sizeDelta = new Vector2(0, 40);

        script.wordLearningPanel = panel;
        script.englishWordText = engWordObj.GetComponent<TMP_Text>();
        script.russianWordText = rusWordObj.GetComponent<TMP_Text>();
        script.wordImage = wordImage;
        script.soundButton = soundBtn.GetComponent<Button>();
        script.rememberButton = rememberBtn.GetComponent<Button>();
    }

    static void CreateSentenceExerciseCanvas(GameObject parent, DialogSystem script)
    {
        GameObject canvasObj = new GameObject("SentenceExerciseCanvas");
        canvasObj.transform.SetParent(parent.transform);
        canvasObj.transform.localPosition = new Vector3(0, 1.2f, 1.0f);
        canvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        SetupCanvas(canvasObj);

        GameObject panel = CreatePanel(canvasObj.transform, "SentenceExercisePanel");
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(800, 600);

        // Описание упражнения
        GameObject descObj = CreateText(panel.transform, "ExerciseDescription",
            "Составь предложение:", 32, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.95f));
        descObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
        descObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

        // Контейнер для кнопок слов
        GameObject containerObj = new GameObject("WordButtonContainer");
        containerObj.transform.SetParent(panel.transform);
        RectTransform containerRT = containerObj.AddComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0.05f, 0.4f);
        containerRT.anchorMax = new Vector2(0.95f, 0.8f);
        containerRT.anchoredPosition = Vector2.zero;
        containerRT.sizeDelta = Vector2.zero;

        Image containerImage = containerObj.AddComponent<Image>();
        containerImage.color = new Color(0.95f, 0.95f, 0.95f, 1);

        // Grid Layout для кнопок
        GridLayoutGroup grid = containerObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(120, 60);
        grid.spacing = new Vector2(10, 10);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.Flexible;

        // Собранное предложение
        GameObject sentenceObj = CreateText(panel.transform, "ConstructedSentence",
            "", 28, new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.35f));
        sentenceObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        sentenceObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Текст обратной связи
        GameObject feedbackObj = CreateText(panel.transform, "SentenceFeedback",
            "", 26, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.2f));
        feedbackObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

        // Кнопки управления
        GameObject submitBtn = CreateButton(panel.transform, "SubmitButton");
        TMP_Text submitText = submitBtn.GetComponentInChildren<TMP_Text>();
        submitText.text = "Проверить";
        submitText.fontSize = 20;
        RectTransform submitRT = submitBtn.GetComponent<RectTransform>();
        submitRT.anchorMin = new Vector2(0.6f, 0.05f);
        submitRT.anchorMax = new Vector2(0.8f, 0.12f);
        submitRT.sizeDelta = new Vector2(0, 40);

        GameObject resetBtn = CreateButton(panel.transform, "ResetButton");
        TMP_Text resetText = resetBtn.GetComponentInChildren<TMP_Text>();
        resetText.text = "Сбросить";
        resetText.fontSize = 20;
        RectTransform resetRT = resetBtn.GetComponent<RectTransform>();
        resetRT.anchorMin = new Vector2(0.8f, 0.05f);
        resetRT.anchorMax = new Vector2(0.95f, 0.12f);
        resetRT.sizeDelta = new Vector2(0, 40);

        // Префаб для кнопок слов
        GameObject wordButtonPrefab = new GameObject("WordButtonPrefab");
        wordButtonPrefab.SetActive(false);
        wordButtonPrefab.transform.SetParent(parent.transform);

        Button prefabButton = wordButtonPrefab.AddComponent<Button>();
        Image prefabImage = wordButtonPrefab.AddComponent<Image>();
        prefabImage.color = new Color(0.2f, 0.4f, 0.8f);

        GameObject prefabTextObj = new GameObject("Text");
        prefabTextObj.transform.SetParent(wordButtonPrefab.transform);
        TMP_Text prefabText = prefabTextObj.AddComponent<TextMeshProUGUI>();
        RectTransform prefabTextRT = prefabTextObj.GetComponent<RectTransform>();
        SetupRectTransform(prefabTextRT, Vector2.zero, Vector2.one, Vector2.zero);
        prefabText.text = "Word";
        prefabText.color = Color.white;
        prefabText.fontSize = 18;
        prefabText.alignment = TextAlignmentOptions.Center;

        // Сохраняем префаб
        script.wordButtonPrefab = wordButtonPrefab;

        script.sentenceExercisePanel = panel;
        script.exerciseDescriptionText = descObj.GetComponent<TMP_Text>();
        script.wordButtonContainer = containerObj.GetComponent<RectTransform>();
        script.constructedSentenceText = sentenceObj.GetComponent<TMP_Text>();
        script.sentenceFeedbackText = feedbackObj.GetComponent<TMP_Text>();
        script.submitSentenceButton = submitBtn.GetComponent<Button>();
        script.resetSentenceButton = resetBtn.GetComponent<Button>();
    }

    static void CreateToExerciseCanvas(GameObject parent, DialogSystem script)
    {
        GameObject canvasObj = new GameObject("ToExerciseCanvas");
        canvasObj.transform.SetParent(parent.transform);
        canvasObj.transform.localPosition = new Vector3(-1.2f, 1.2f, 1.5f);
        canvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        SetupCanvas(canvasObj);

        GameObject panel = CreatePanel(canvasObj.transform, "ToExercisePanel");
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(600, 400);

        // Предложение
        GameObject sentenceObj = CreateText(panel.transform, "ToSentence",
            "I want ____ drink coffee", 32, new Vector2(0.05f, 0.6f), new Vector2(0.95f, 0.8f));
        sentenceObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        sentenceObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Объяснение
        GameObject explanationObj = CreateText(panel.transform, "ToExplanation",
            "", 24, new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.55f));
        explanationObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

        // Обратная связь
        GameObject feedbackObj = CreateText(panel.transform, "ToFeedback",
            "", 28, new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.35f));
        feedbackObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        feedbackObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Кнопки
        GameObject yesBtn = CreateButton(panel.transform, "ToYesButton");
        TMP_Text yesText = yesBtn.GetComponentInChildren<TMP_Text>();
        yesText.text = "Нужно TO";
        yesText.fontSize = 22;
        RectTransform yesRT = yesBtn.GetComponent<RectTransform>();
        yesRT.anchorMin = new Vector2(0.1f, 0.1f);
        yesRT.anchorMax = new Vector2(0.4f, 0.2f);
        yesRT.sizeDelta = new Vector2(0, 50);

        GameObject noBtn = CreateButton(panel.transform, "ToNoButton");
        TMP_Text noText = noBtn.GetComponentInChildren<TMP_Text>();
        noText.text = "Не нужно TO";
        noText.fontSize = 22;
        RectTransform noRT = noBtn.GetComponent<RectTransform>();
        noRT.anchorMin = new Vector2(0.5f, 0.1f);
        noRT.anchorMax = new Vector2(0.9f, 0.2f);
        noRT.sizeDelta = new Vector2(0, 50);

        script.toExercisePanel = panel;
        script.toSentenceText = sentenceObj.GetComponent<TMP_Text>();
        script.toExplanationText = explanationObj.GetComponent<TMP_Text>();
        script.toFeedbackText = feedbackObj.GetComponent<TMP_Text>();
        script.toYesButton = yesBtn.GetComponent<Button>();
        script.toNoButton = noBtn.GetComponent<Button>();
    }

    static void CreatePronunciationCanvas(GameObject parent, DialogSystem script)
    {
        GameObject canvasObj = new GameObject("PronunciationCanvas");
        canvasObj.transform.SetParent(parent.transform);
        canvasObj.transform.localPosition = new Vector3(1.2f, 1.2f, 0.8f);
        canvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        SetupCanvas(canvasObj);

        GameObject panel = CreatePanel(canvasObj.transform, "PronunciationPanel");
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(700, 500);

        // Английская фраза
        GameObject phraseObj = CreateText(panel.transform, "PronunciationPhrase",
            "I want coffee", 40, new Vector2(0.05f, 0.7f), new Vector2(0.95f, 0.85f));
        phraseObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        phraseObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Русский перевод
        GameObject translationObj = CreateText(panel.transform, "PronunciationTranslation",
            "Я хочу кофе", 32, new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.7f));
        translationObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        translationObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Статус записи
        GameObject statusObj = CreateText(panel.transform, "RecordingStatus",
            "Готов к записи", 26, new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.5f));
        statusObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

        // Обратная связь
        GameObject feedbackObj = CreateText(panel.transform, "PronunciationFeedback",
            "Нажмите 'Начать запись'", 28, new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.35f));
        feedbackObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        feedbackObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Слайдер точности
        GameObject sliderObj = new GameObject("AccuracySlider");
        sliderObj.transform.SetParent(panel.transform);
        Slider slider = sliderObj.AddComponent<Slider>();
        RectTransform sliderRT = sliderObj.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.1f, 0.15f);
        sliderRT.anchorMax = new Vector2(0.9f, 0.2f);
        sliderRT.anchoredPosition = Vector2.zero;
        sliderRT.sizeDelta = new Vector2(0, 20);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        SetupRectTransform(fillAreaRT, new Vector2(0, 0.25f), new Vector2(1, 0.75f), Vector2.zero);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        SetupRectTransform(fillRT, Vector2.zero, Vector2.one, Vector2.zero);
        fillImage.color = Color.green;

        slider.fillRect = fillRT;

        // Кнопки
        GameObject startBtn = CreateButton(panel.transform, "StartRecordingButton");
        TMP_Text startText = startBtn.GetComponentInChildren<TMP_Text>();
        startText.text = "Начать запись";
        startText.fontSize = 22;
        RectTransform startRT = startBtn.GetComponent<RectTransform>();
        startRT.anchorMin = new Vector2(0.1f, 0.05f);
        startRT.anchorMax = new Vector2(0.4f, 0.12f);
        startRT.sizeDelta = new Vector2(0, 40);

        GameObject stopBtn = CreateButton(panel.transform, "StopRecordingButton");
        TMP_Text stopText = stopBtn.GetComponentInChildren<TMP_Text>();
        stopText.text = "Остановить";
        stopText.fontSize = 22;
        RectTransform stopRT = stopBtn.GetComponent<RectTransform>();
        stopRT.anchorMin = new Vector2(0.5f, 0.05f);
        stopRT.anchorMax = new Vector2(0.9f, 0.12f);
        stopRT.sizeDelta = new Vector2(0, 40);
        stopBtn.GetComponent<Button>().interactable = false;

        script.pronunciationPanel = panel;
        script.pronunciationPhraseText = phraseObj.GetComponent<TMP_Text>();
        script.pronunciationTranslationText = translationObj.GetComponent<TMP_Text>();
        script.recordingStatusText = statusObj.GetComponent<TMP_Text>();
        script.pronunciationFeedbackText = feedbackObj.GetComponent<TMP_Text>();
        script.accuracySlider = slider;
        script.startRecordingButton = startBtn.GetComponent<Button>();
        script.stopRecordingButton = stopBtn.GetComponent<Button>();
    }

    static void CreateTranslationChoiceCanvas(GameObject parent, DialogSystem script)
    {
        GameObject canvasObj = new GameObject("TranslationChoiceCanvas");
        canvasObj.transform.SetParent(parent.transform);
        canvasObj.transform.localPosition = new Vector3(-1.2f, 1.2f, 0.8f);
        canvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        SetupCanvas(canvasObj);

        GameObject panel = CreatePanel(canvasObj.transform, "TranslationChoicePanel");
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(600, 500);

        // Вопрос
        GameObject questionObj = CreateText(panel.transform, "ChoiceQuestion",
            "Что изображено на картинке?", 30, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.95f));
        questionObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        questionObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Изображение
        GameObject imageObj = new GameObject("ChoiceWordImage");
        imageObj.transform.SetParent(panel.transform);
        Image choiceImage = imageObj.AddComponent<Image>();
        RectTransform imageRT = imageObj.GetComponent<RectTransform>();
        imageRT.anchorMin = new Vector2(0.2f, 0.55f);
        imageRT.anchorMax = new Vector2(0.8f, 0.8f);
        imageRT.anchoredPosition = Vector2.zero;
        imageRT.sizeDelta = Vector2.zero;
        choiceImage.color = new Color(0.9f, 0.9f, 0.8f, 1);

        // Обратная связь
        GameObject feedbackObj = CreateText(panel.transform, "ChoiceFeedback",
            "", 28, new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.45f));
        feedbackObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        feedbackObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Кнопки выбора (4 штуки)
        Button[] choiceButtons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject btn = CreateButton(panel.transform, $"ChoiceButton_{i}");
            TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
            btnText.text = $"Вариант {i + 1}";
            btnText.fontSize = 18;
            RectTransform btnRT = btn.GetComponent<RectTransform>();

            float yMin = 0.2f + (i * 0.08f);
            float yMax = 0.28f + (i * 0.08f);

            btnRT.anchorMin = new Vector2(0.1f, yMin);
            btnRT.anchorMax = new Vector2(0.9f, yMax);
            btnRT.sizeDelta = new Vector2(0, 35);

            choiceButtons[i] = btn.GetComponent<Button>();
        }

        script.translationChoicePanel = panel;
        script.choiceQuestionText = questionObj.GetComponent<TMP_Text>();
        script.choiceWordImage = choiceImage;
        script.choiceButtons = choiceButtons;
        script.choiceFeedbackText = feedbackObj.GetComponent<TMP_Text>();
    }

    static void CreateRewardCanvas(GameObject parent, DialogSystem script)
    {
        GameObject canvasObj = new GameObject("RewardCanvas");
        canvasObj.transform.SetParent(parent.transform);
        canvasObj.transform.localPosition = new Vector3(0, 1.2f, 1.2f);
        canvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        SetupCanvas(canvasObj);

        GameObject panel = CreatePanel(canvasObj.transform, "RewardPanel");
        panel.GetComponent<Image>().color = new Color(0.9f, 0.95f, 0.8f, 0.95f);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(500, 350);

        // Текст награды
        GameObject rewardObj = CreateText(panel.transform, "RewardText",
            "Поздравляем!\n+100 опыта\n+50 монет", 40, new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.9f));
        TMP_Text rewardText = rewardObj.GetComponent<TMP_Text>();
        rewardText.alignment = TextAlignmentOptions.Center;
        rewardText.fontStyle = FontStyles.Bold;
        rewardText.lineSpacing = 25;

        // Текст опыта
        GameObject xpObj = CreateText(panel.transform, "XPText",
            "Опыт: 0", 24, new Vector2(0.05f, 0.3f), new Vector2(0.45f, 0.4f));
        xpObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Текст монет
        GameObject coinsObj = CreateText(panel.transform, "CoinsText",
            "Монеты: 0", 24, new Vector2(0.55f, 0.3f), new Vector2(0.95f, 0.4f));
        coinsObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        script.rewardPanel = panel;
        script.rewardText = rewardText;
        script.xpText = xpObj.GetComponent<TMP_Text>();
        script.coinsText = coinsObj.GetComponent<TMP_Text>();
    }

    static void CreateInteractionHint(GameObject parent, DialogSystem script)
    {
        // Создаем канвас для подсказки
        GameObject hintCanvasObj = new GameObject("InteractionHintCanvas");
        hintCanvasObj.transform.SetParent(parent.transform);
        hintCanvasObj.transform.localPosition = new Vector3(0, 0.5f, 1.5f);
        hintCanvasObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        Canvas hintCanvas = hintCanvasObj.AddComponent<Canvas>();
        CanvasScaler hintScaler = hintCanvasObj.AddComponent<CanvasScaler>();
        GraphicRaycaster hintRaycaster = hintCanvasObj.AddComponent<GraphicRaycaster>();

        hintCanvas.renderMode = RenderMode.WorldSpace;
        hintScaler.dynamicPixelsPerUnit = 20;

        // Текст подсказки
        GameObject hintObj = new GameObject("InteractionHint");
        hintObj.transform.SetParent(hintCanvasObj.transform);
        TMP_Text hintText = hintObj.AddComponent<TextMeshProUGUI>();
        RectTransform hintRT = hintObj.GetComponent<RectTransform>();
        hintRT.sizeDelta = new Vector2(300, 50);
        hintText.text = "Нажмите E для взаимодействия";
        hintText.fontSize = 16;
        hintText.color = Color.yellow;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.fontStyle = FontStyles.Bold;

        // Подключаем к скрипту
        script.interactionHint = hintObj;
    }

    // Вспомогательные методы
    static void SetupCanvas(GameObject canvasObj)
    {
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

        canvas.renderMode = RenderMode.WorldSpace;
        scaler.dynamicPixelsPerUnit = 10;
    }

    static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        RectTransform rt = panel.AddComponent<RectTransform>();
        SetupRectTransform(rt, Vector2.zero, Vector2.one, Vector2.zero);

        Image image = panel.AddComponent<Image>();
        image.color = new Color(1, 1, 1, 0.95f);

        return panel;
    }

    static GameObject CreateButton(Transform parent, string name)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent);
        RectTransform rt = button.AddComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(100, 40); // Нормальный размер для VR

        Image image = button.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.8f);

        Button btn = button.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform);
        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        SetupRectTransform(textRT, Vector2.zero, Vector2.one, Vector2.zero);
        text.text = "Кнопка";
        text.color = Color.white;
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;

        return button;
    }

    static GameObject CreateText(Transform parent, string name, string content, int fontSize,
                               Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.Left;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        text.enableWordWrapping = true;

        return textObj;
    }

    static void SetupRectTransform(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = Vector2.zero;
    }

    static void SetupExampleData(DialogSystem script)
    {
        // Пример слова
        DialogSystem.WordData word = new DialogSystem.WordData();
        word.english = "want";
        word.russian = "хотеть";
        word.category = "verbs";
        script.wordsToLearn.Add(word);

        // Пример упражнения с предложением
        DialogSystem.SentenceExercise sentenceEx = new DialogSystem.SentenceExercise();
        sentenceEx.russianSentence = "Я хочу кофе";
        sentenceEx.correctEnglish = "I want coffee";
        sentenceEx.wordOptions = new string[] { "I", "want", "coffee", "tea", "drink" };
        script.sentenceExercises.Add(sentenceEx);

        // Пример упражнения с TO
        DialogSystem.ToExercise toEx = new DialogSystem.ToExercise();
        toEx.sentence = "I want ____ drink coffee";
        toEx.needsTo = true;
        toEx.explanation = "После want нужно ставить to перед глаголом";
        script.toExercises.Add(toEx);

        // Пример упражнения на произношение
        DialogSystem.PronunciationExercise pronEx = new DialogSystem.PronunciationExercise();
        pronEx.englishPhrase = "I want coffee";
        pronEx.russianTranslation = "Я хочу кофе";
        pronEx.requiredAccuracy = 0.7f;
        script.pronunciationExercises.Add(pronEx);

        // Пример упражнения с картинками
        DialogSystem.ImageChoiceExercise imgEx = new DialogSystem.ImageChoiceExercise();
        imgEx.correctAnswer = "coffee";
        imgEx.wrongAnswers = new string[] { "tea", "water", "juice" };
        script.imageExercises.Add(imgEx);

        Debug.Log("Примерные данные добавлены в систему!");
    }
#endif
}