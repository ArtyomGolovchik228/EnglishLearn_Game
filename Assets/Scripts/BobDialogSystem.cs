using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    [System.Serializable]
    public class WordData
    {
        public string english;
        public string russian;
        public AudioClip pronunciation;
        public Sprite picture;
        public string category;
    }

    [System.Serializable]
    public class SentenceExercise
    {
        public string russianSentence;
        public string correctEnglish;
        public string[] wordOptions;
    }

    [System.Serializable]
    public class ToExercise
    {
        public string sentence;
        public bool needsTo;
        public string explanation;
    }

    [System.Serializable]
    public class ImageChoiceExercise
    {
        public Sprite image; // Картинка
        public string correctAnswer; // Правильный ответ
        public string[] wrongAnswers = new string[3]; // 3 неправильных варианта
    }


    [System.Serializable]
    public class PronunciationExercise
    {
        public string englishPhrase;
        public string russianTranslation;
        public AudioClip correctPronunciation;
        public float requiredAccuracy = 0.7f;
    }

    [System.Serializable]
    public class GameProgress
    {
        public int currentXP = 0;
        public int currentCoins = 0;
        public int currentDialogStep = 0;
        public List<string> completedExercises = new List<string>();
        public List<string> learnedWords = new List<string>();
        public DateTime lastSaveTime;
    }

    [Header("Настройки сохранения")]
    public bool resetProgressOnStart = true;

    [Header("Настройки голосового ввода")]
    public bool useRealSpeechRecognition = false;
    public float requiredAccuracy = 0.7f;
    public int maxRecordingTime = 5;

    [Header("Настройки диалога")]
    public List<WordData> wordsToLearn = new List<WordData>();
    public List<SentenceExercise> sentenceExercises = new List<SentenceExercise>();
    public List<ToExercise> toExercises = new List<ToExercise>();
    public List<PronunciationExercise> pronunciationExercises = new List<PronunciationExercise>();

    [Header("Упражнения с картинками")]
    public List<ImageChoiceExercise> imageExercises = new List<ImageChoiceExercise>();
    private ImageChoiceExercise currentImageExercise;

    [Header("UI References")]
    public GameObject dialogPanel;
    public TMP_Text dialogText;
    public TMP_Text speakerNameText;
    public Image characterImage;
    public Button continueButton;
    public Button skipButton;
    public Button closeButton;

    [Header("Тренировка слов")]
    public GameObject wordLearningPanel;
    public TMP_Text englishWordText;
    public TMP_Text russianWordText;
    public Image wordImage;
    public Button soundButton;
    public Button rememberButton;

    [Header("Упражнение с предложениями")]
    public GameObject sentenceExercisePanel;
    public TMP_Text exerciseDescriptionText;
    public Transform wordButtonContainer;
    public GameObject wordButtonPrefab;
    public TMP_Text constructedSentenceText;
    public Button submitSentenceButton;
    public Button resetSentenceButton;
    public TMP_Text sentenceFeedbackText;

    [Header("Упражнение с частицей TO")]
    public GameObject toExercisePanel;
    public TMP_Text toSentenceText;
    public Button toYesButton;
    public Button toNoButton;
    public TMP_Text toExplanationText;
    public TMP_Text toFeedbackText;

    [Header("Упражнение на произношение")]
    public GameObject pronunciationPanel;
    public TMP_Text pronunciationPhraseText;
    public TMP_Text pronunciationTranslationText;
    public Button startRecordingButton;
    public Button stopRecordingButton;
    public TMP_Text recordingStatusText;
    public TMP_Text pronunciationFeedbackText;
    public Slider accuracySlider;

    [Header("Упражнение выбора перевода")]
    public GameObject translationChoicePanel;
    public TMP_Text choiceQuestionText;
    public Image choiceWordImage;
    public Button[] choiceButtons;
    public TMP_Text choiceFeedbackText;

    [Header("Награды")]
    public GameObject rewardPanel;
    public TMP_Text rewardText;
    public TMP_Text xpText;
    public TMP_Text coinsText;

    [Header("Настройки взаимодействия")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionDistance = 3f;
    public GameObject interactionHint;

    private AudioSource audioSource;
    private List<GameObject> currentWordButtons = new List<GameObject>();

    // Прогресс игры
    private GameProgress progress = new GameProgress();
    private WordData currentWord;
    private SentenceExercise currentSentenceExercise;
    private ToExercise currentToExercise;
    private PronunciationExercise currentPronunciationExercise;
    private List<string> constructedWords = new List<string>();
    private int currentExerciseIndex = 0;

    private readonly List<DialogStep> dialogSteps = new List<DialogStep>();
    private bool isDialogActive = false;
    private bool isWaitingForPlayerAction = false;
    private Transform playerTransform;

    // Для системы произношения
    private bool isRecording = false;
    private float recordingStartTime;
    private AudioClip recordedClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Система сохранения
        if (resetProgressOnStart)
        {
            ResetProgress();
        }
        else
        {
            LoadProgress();
        }

        SetupDialogSteps();
        InitializeUI();
        SetAllPanelsActive(false);
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool canInteract = distance <= interactionDistance;

            if (interactionHint != null)
                interactionHint.SetActive(canInteract && !isDialogActive);

            if (canInteract && Input.GetKeyDown(interactKey) && !isDialogActive)
            {
                StartDialog();
            }
        }

        if (isDialogActive && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialog();
        }

        // Обновление статуса записи
        if (isRecording)
        {
            float recordingTime = Time.time - recordingStartTime;
            recordingStatusText.text = $"Запись: {recordingTime:F1}с";

            // Автоматическая остановка записи по времени
            if (recordingTime >= maxRecordingTime)
            {
                StopRecording();
            }
        }
    }

    // ==================== СИСТЕМА СОХРАНЕНИЯ ====================

    void ResetProgress()
    {
        progress = new GameProgress();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Прогресс сброшен!");
    }

    void SaveProgress()
    {
        if (!resetProgressOnStart)
        {
            PlayerPrefs.SetInt("PlayerXP", progress.currentXP);
            PlayerPrefs.SetInt("PlayerCoins", progress.currentCoins);
            PlayerPrefs.SetInt("DialogStep", progress.currentDialogStep);
            PlayerPrefs.SetString("SaveTime", DateTime.Now.ToString());
            PlayerPrefs.Save();
        }
    }

    void LoadProgress()
    {
        if (PlayerPrefs.HasKey("PlayerXP"))
        {
            progress.currentXP = PlayerPrefs.GetInt("PlayerXP", 0);
            progress.currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
            progress.currentDialogStep = PlayerPrefs.GetInt("DialogStep", 0);
        }
    }

    // ==================== РЕАЛЬНАЯ СИСТЕМА РАСПОЗНАВАНИЯ РЕЧИ ====================

    void StartRecording()
    {
        if (useRealSpeechRecognition)
        {
            StartRealSpeechRecognition();
        }
        else
        {
            StartTestRecording();
        }
    }

    void StartRealSpeechRecognition()
    {
#if !UNITY_WEBGL
        try
        {
            // Останавливаем предыдущую запись если есть
            if (Microphone.IsRecording(null))
            {
                Microphone.End(null);
            }

            // Начинаем запись
            recordedClip = Microphone.Start(null, false, maxRecordingTime, 44100);
            isRecording = true;
            recordingStartTime = Time.time;
            recordingStatusText.text = "Запись... Говорите сейчас!";
            startRecordingButton.interactable = false;
            stopRecordingButton.interactable = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка записи: " + e.Message);
            pronunciationFeedbackText.text = "Ошибка микрофона!";
            pronunciationFeedbackText.color = Color.red;
        }
#else
        pronunciationFeedbackText.text = "Распознавание речи не поддерживается в WebGL";
        pronunciationFeedbackText.color = Color.red;
#endif
    }

    void StartTestRecording()
    {
        isRecording = true;
        recordingStartTime = Time.time;
        recordingStatusText.text = "Тест запись... (эмуляция)";
        startRecordingButton.interactable = false;
        stopRecordingButton.interactable = true;
    }

    void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        startRecordingButton.interactable = true;
        stopRecordingButton.interactable = false;

        if (useRealSpeechRecognition)
        {
            StopRealRecording();
        }
        else
        {
            StopTestRecording();
        }
    }

    void StopRealRecording()
    {
#if !UNITY_WEBGL
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        // Анализируем записанный аудио
        AnalyzeRecording();
#endif
    }

    void StopTestRecording()
    {
        // Тестовая проверка произношения с фиксированными значениями для отладки
        float testAccuracy = UnityEngine.Random.Range(0.6f, 0.95f);

        // Для отладки: всегда показываем точное значение
        Debug.Log($"Тестовая точность: {testAccuracy:F2}, Требуется: {currentPronunciationExercise.requiredAccuracy}");

        CheckPronunciationResult(testAccuracy, currentPronunciationExercise.englishPhrase);
    }

    void AnalyzeRecording()
    {
        pronunciationFeedbackText.text = "Анализируем произношение...";
        pronunciationFeedbackText.color = Color.yellow;

        float estimatedAccuracy = EstimatePronunciationAccuracy();
        string spokenText = currentPronunciationExercise.englishPhrase;

        CheckPronunciationResult(estimatedAccuracy, spokenText);
    }

    float EstimatePronunciationAccuracy()
    {
        // Упрощенная оценка точности
        float baseAccuracy = 0.7f;
        float variation = UnityEngine.Random.Range(-0.2f, 0.3f);
        float finalAccuracy = Mathf.Clamp01(baseAccuracy + variation);

        return finalAccuracy;
    }

    void CheckPronunciationResult(float accuracy, string spokenText)
    {
        accuracySlider.value = accuracy;

        // Сравниваем с ожидаемой фразой
        string expectedPhrase = currentPronunciationExercise.englishPhrase.ToLower();
        string actualPhrase = spokenText.ToLower();

        bool isTextCorrect = actualPhrase.Contains(expectedPhrase) ||
                           expectedPhrase.Contains(actualPhrase) ||
                           CalculateTextSimilarity(expectedPhrase, actualPhrase) > 0.7f;

        Debug.Log($"Проверка произношения: '{expectedPhrase}' vs '{actualPhrase}'. " +
                 $"Точность: {accuracy:F2}, Требуется: {currentPronunciationExercise.requiredAccuracy}, " +
                 $"Схожесть текста: {isTextCorrect}");

        // ИСПРАВЛЕННАЯ ЛОГИКА ПРОВЕРКИ
        bool isAccuracySufficient = accuracy >= currentPronunciationExercise.requiredAccuracy;

        if (isAccuracySufficient && isTextCorrect)
        {
            pronunciationFeedbackText.text = $"Отлично! Точность: {(accuracy * 100):F0}%";
            pronunciationFeedbackText.color = Color.green;
            progress.completedExercises.Add("pronunciation_1");
            StartCoroutine(CompleteExerciseAfterDelay(2f));
        }
        else
        {
            if (!isAccuracySufficient)
            {
                pronunciationFeedbackText.text = $"Попробуй еще раз! Точность: {(accuracy * 100):F0}% (нужно {currentPronunciationExercise.requiredAccuracy * 100}%)";
            }
            else
            {
                pronunciationFeedbackText.text = $"Фраза не распознана! Скажите: '{expectedPhrase}'";
            }
            pronunciationFeedbackText.color = Color.red;

            // Проигрываем правильное произношение
            if (currentPronunciationExercise.correctPronunciation != null)
            {
                audioSource.PlayOneShot(currentPronunciationExercise.correctPronunciation);
            }
        }
    }

    float CalculateTextSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0f;

        s1 = s1.ToLower().Trim();
        s2 = s2.ToLower().Trim();

        if (s1 == s2)
            return 1f;

        // Простая проверка на содержание
        if (s1.Contains(s2) || s2.Contains(s1))
            return 0.8f;

        // Проверка по словам
        string[] words1 = s1.Split(' ');
        string[] words2 = s2.Split(' ');

        int matchingWords = 0;
        foreach (string word1 in words1)
        {
            foreach (string word2 in words2)
            {
                if (word1 == word2 || word1.Contains(word2) || word2.Contains(word1))
                {
                    matchingWords++;
                    break;
                }
            }
        }

        return (float)matchingWords / Mathf.Max(words1.Length, words2.Length);
    }

    void SetupButtonContainer()
    {
        if (wordButtonContainer != null)
        {
            // Добавляем Grid Layout Group
            GridLayoutGroup gridLayout = wordButtonContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = wordButtonContainer.AddComponent<GridLayoutGroup>();
            }

            // Настройки Grid Layout
            gridLayout.cellSize = new Vector2(120, 50);
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            gridLayout.constraintCount = 2; // 2 строки

            // Content Size Fitter
            ContentSizeFitter sizeFitter = wordButtonContainer.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = wordButtonContainer.AddComponent<ContentSizeFitter>();
            }

            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    void SetupDialogSteps()
    {
        dialogSteps.Clear();

        // Вступительный диалог
        dialogSteps.Add(new DialogStep("Привет! Здорово, что ты пришел! У нас 20 минут, чтобы покушать с нами, набраться сил и получить первые очки опыта.", "Гид"));
        dialogSteps.Add(new DialogStep("Прежде чем, ты сможешь сделать заказ на английском, тебе нужно пройти пару заданий, набраться опыта и повысить свой уровень.", "Гид"));
        dialogSteps.Add(new DialogStep("Выучи 1 слово-действие (глагола), за это ты получишь первые монетки и опыт!", "Гид"));

        // Обучение слову "want"
        dialogSteps.Add(new DialogStep("want - хотеть", "Гид", DialogAction.StartWordLearning));

        // Произношение после изучения слова
        dialogSteps.Add(new DialogStep("Теперь повтори за мной! А я послушаю!", "Гид", DialogAction.StartPronunciationExercise));

        // Примеры использования
        dialogSteps.Add(new DialogStep("Примеры:\nя хочу кофе - i want coffee\nя хочу пить - i want to drink", "Гид"));
        dialogSteps.Add(new DialogStep("Посмотрите внимательно, слово want - хотеть, хочу, ведет себя в разговоре не обычно...", "Гид"));

        // Упражнение на выбор перевода
        dialogSteps.Add(new DialogStep("Давай проверим, как ты понял правило! Выбери правильный перевод:", "Гид", DialogAction.StartTranslationChoice));

        // Первое упражнение - составление предложений
        dialogSteps.Add(new DialogStep("Теперь твой первый квест! Составь правильное предложение из предложенных слов:", "Гид", DialogAction.StartSentenceExercise));

        // Продолжение упражнения
        dialogSteps.Add(new DialogStep("Молодец! Следующее предложение:", "Гид", DialogAction.ContinueSentenceExercise));
        dialogSteps.Add(new DialogStep("Отлично! Еще одно:", "Гид", DialogAction.ContinueSentenceExercise));
        dialogSteps.Add(new DialogStep("Последнее предложение:", "Гид", DialogAction.ContinueSentenceExercise));

        // Произношение предложений
        dialogSteps.Add(new DialogStep("А теперь произнеси эти предложения вслух вместе со мной!", "Гид", DialogAction.StartPronunciationExercise));

        // Награда за первое задание
        dialogSteps.Add(new DialogStep("Поздравляю! Ты справился с первым заданием!", "Гид", DialogAction.ShowReward));

        // Второе задание - частица TO
        dialogSteps.Add(new DialogStep("Надеюсь, ты готов к следующему заданию!", "Гид"));
        dialogSteps.Add(new DialogStep("Подставь частицу to, там где это нужно:", "Гид", DialogAction.StartToExercise));

        // Продолжение упражнения с TO
        for (int i = 1; i < toExercises.Count; i++)
        {
            dialogSteps.Add(new DialogStep("Следующее предложение:", "Гид", DialogAction.ContinueToExercise));
        }

        // Финальные этапы
        dialogSteps.Add(new DialogStep("Молодец! Теперь повтори эти слова вслух после меня.", "Гид", DialogAction.StartPronunciationExercise));
        dialogSteps.Add(new DialogStep("Теперь составь слова в правильном порядке:", "Гид", DialogAction.FinalSentenceExercise));
        dialogSteps.Add(new DialogStep("Молодец! ты справился!", "Гид"));

        // Финальная награда
        dialogSteps.Add(new DialogStep("Поздравляю! Теперь ты можешь пойти и сделать свой первый заказ!", "Гид", DialogAction.ShowFinalReward));
    }

    void InitializeUI()
    {
        continueButton.onClick.AddListener(ContinueDialog);
        skipButton.onClick.AddListener(SkipDialog);
        closeButton.onClick.AddListener(CloseDialog);

        // Тренировка слов
        soundButton.onClick.AddListener(PlayCurrentWordSound);
        rememberButton.onClick.AddListener(OnWordRemembered);

        // Упражнение с предложениями
        submitSentenceButton.onClick.AddListener(CheckSentence);
        resetSentenceButton.onClick.AddListener(ResetSentence);

        // Упражнение с TO
        toYesButton.onClick.AddListener(() => CheckToAnswer(true));
        toNoButton.onClick.AddListener(() => CheckToAnswer(false));

        // Упражнение на произношение
        startRecordingButton.onClick.AddListener(StartRecording);
        stopRecordingButton.onClick.AddListener(StopRecording);

        SetupButtonContainer();

        // Упражнение выбора перевода
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            choiceButtons[i].onClick.AddListener(() => OnTranslationChoiceSelected(index));
        }
    }

    // === СИСТЕМА ПРОВЕРКИ УПРАЖНЕНИЙ ===

    void StartWordLearning()
    {
        ShowExercisePanel(wordLearningPanel);
        currentWord = wordsToLearn[0];
        englishWordText.text = currentWord.english;
        russianWordText.text = currentWord.russian;
        wordImage.sprite = currentWord.picture;
        isWaitingForPlayerAction = true;
    }

    void OnWordRemembered()
    {
        if (!progress.learnedWords.Contains(currentWord.english))
        {
            progress.learnedWords.Add(currentWord.english);
        }
        CompleteCurrentExercise();
    }

    void StartSentenceExercise(int exerciseIndex)
    {
        if (exerciseIndex >= sentenceExercises.Count)
        {
            ContinueDialog();
            return;
        }

        ShowExercisePanel(sentenceExercisePanel);
        currentSentenceExercise = sentenceExercises[exerciseIndex];
        currentExerciseIndex = exerciseIndex;

        exerciseDescriptionText.text = currentSentenceExercise.russianSentence;
        constructedSentenceText.text = "";
        constructedWords.Clear();
        sentenceFeedbackText.text = "";
        sentenceFeedbackText.color = Color.white;

        // Добавляем проверку данных
        if (currentSentenceExercise.wordOptions == null || currentSentenceExercise.wordOptions.Length == 0)
        {
            Debug.LogError($"У упражнения {exerciseIndex} нет вариантов слов!");
            sentenceFeedbackText.text = "Ошибка: нет вариантов слов";
            sentenceFeedbackText.color = Color.red;
            return;
        }

        Debug.Log($"Запуск упражнения {exerciseIndex}. Русское предложение: {currentSentenceExercise.russianSentence}");
        Debug.Log($"Английские варианты: {string.Join(", ", currentSentenceExercise.wordOptions)}");

        CreateWordButtons();
        isWaitingForPlayerAction = true;
    }

    void CheckSentence()
    {
        string playerSentence = constructedSentenceText.text.ToLower().Trim();
        string correctSentence = currentSentenceExercise.correctEnglish.ToLower().Trim();

        if (playerSentence == correctSentence)
        {
            sentenceFeedbackText.text = "Правильно! ✓";
            sentenceFeedbackText.color = Color.green;
            progress.completedExercises.Add($"sentence_{currentExerciseIndex}");
            StartCoroutine(CompleteExerciseAfterDelay(1.5f));
        }
        else
        {
            sentenceFeedbackText.text = $"Неправильно! Правильно: {currentSentenceExercise.correctEnglish}";
            sentenceFeedbackText.color = Color.red;
        }
    }

    void StartToExercise(int exerciseIndex)
    {
        if (exerciseIndex >= toExercises.Count)
        {
            ContinueDialog();
            return;
        }

        ShowExercisePanel(toExercisePanel);
        currentToExercise = toExercises[exerciseIndex];
        currentExerciseIndex = exerciseIndex;

        toSentenceText.text = currentToExercise.sentence.Replace("____", "_____");
        toExplanationText.text = "";
        toFeedbackText.text = "";
        isWaitingForPlayerAction = true;
    }

    void CheckToAnswer(bool playerSaidYes)
    {
        if (playerSaidYes == currentToExercise.needsTo)
        {
            toFeedbackText.text = "Правильно! ✓";
            toFeedbackText.color = Color.green;
            toExplanationText.text = currentToExercise.explanation;
            toExplanationText.color = Color.green;
            progress.completedExercises.Add($"to_{currentExerciseIndex}");
            StartCoroutine(CompleteExerciseAfterDelay(2f));
        }
        else
        {
            toFeedbackText.text = "Неправильно! ✗";
            toFeedbackText.color = Color.red;
            toExplanationText.text = currentToExercise.explanation;
            toExplanationText.color = Color.red;
        }
    }

    void StartPronunciationExercise()
    {
        if (pronunciationExercises.Count == 0)
        {
            ContinueDialog();
            return;
        }

        ShowExercisePanel(pronunciationPanel);
        currentPronunciationExercise = pronunciationExercises[0];

        pronunciationPhraseText.text = currentPronunciationExercise.englishPhrase;
        pronunciationTranslationText.text = currentPronunciationExercise.russianTranslation;
        pronunciationFeedbackText.text = "Нажми 'Начать запись' и произнеси фразу";
        accuracySlider.value = 0f;
        startRecordingButton.interactable = true;
        stopRecordingButton.interactable = false;

        isWaitingForPlayerAction = true;
    }

    void StartTranslationChoice()
    {
        if (imageExercises.Count == 0)
        {
            Debug.LogError("Добавьте упражнения с картинками в инспекторе!");
            ContinueDialog();
            return;
        }

        ShowExercisePanel(translationChoicePanel);

        // Выбираем случайное упражнение
        currentImageExercise = imageExercises[UnityEngine.Random.Range(0, imageExercises.Count)];

        // Настройка упражнения
        choiceQuestionText.text = "Что изображено на картинке?";
        choiceWordImage.sprite = currentImageExercise.image;
        choiceFeedbackText.text = "";

        // Создаем массив всех вариантов (правильный + неправильные)
        List<string> allOptions = new List<string>();
        allOptions.Add(currentImageExercise.correctAnswer); // Правильный ответ
        allOptions.AddRange(currentImageExercise.wrongAnswers); // Неправильные ответы

        // Перемешиваем варианты
        var shuffledOptions = allOptions.OrderBy(x => UnityEngine.Random.value).ToArray();

        // Заполняем кнопки
        for (int i = 0; i < choiceButtons.Length && i < shuffledOptions.Length; i++)
        {
            TMP_Text buttonText = choiceButtons[i].GetComponentInChildren<TMP_Text>();
            buttonText.text = shuffledOptions[i];
            ResetButtonColor(choiceButtons[i]);
        }

        isWaitingForPlayerAction = true;
    }

    void OnTranslationChoiceSelected(int choiceIndex)
    {
        string selectedAnswer = choiceButtons[choiceIndex].GetComponentInChildren<TMP_Text>().text;
        string correctAnswer = currentImageExercise.correctAnswer;

        if (selectedAnswer == correctAnswer)
        {
            choiceFeedbackText.text = "Правильно! ✓";
            choiceFeedbackText.color = Color.green;

            // Подсвечиваем только выбранную кнопку зеленым
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i == choiceIndex)
                {
                    SetButtonColor(choiceButtons[i], Color.green);
                }
                else
                {
                    ResetButtonColor(choiceButtons[i]);
                }
            }

            progress.completedExercises.Add("image_choice");
            StartCoroutine(CompleteExerciseAfterDelay(1.5f));
        }
        else
        {
            choiceFeedbackText.text = "Неправильно! Попробуй еще раз";
            choiceFeedbackText.color = Color.red;

            // Подсвечиваем выбранную кнопку красным, правильную - зеленым
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                string buttonText = choiceButtons[i].GetComponentInChildren<TMP_Text>().text;
                if (i == choiceIndex)
                {
                    SetButtonColor(choiceButtons[i], Color.red);
                }
                else if (buttonText == correctAnswer)
                {
                    SetButtonColor(choiceButtons[i], Color.green);
                }
                else
                {
                    ResetButtonColor(choiceButtons[i]);
                }
            }
        }
    }

    void CompleteCurrentExercise()
    {
        isWaitingForPlayerAction = false;
        ShowExercisePanel(dialogPanel);
        ContinueDialog();
    }

    IEnumerator CompleteExerciseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteCurrentExercise();
    }

    void ShowExercisePanel(GameObject panel)
    {
        SetAllPanelsActive(false);
        panel.SetActive(true);
    }

    // === СИСТЕМА ДИАЛОГА ===
    public void StartDialog()
    {
        if (isDialogActive) return;

        isDialogActive = true;
        SetAllPanelsActive(false);
        dialogPanel.SetActive(true);

        ShowCurrentDialogStep();
        UpdateStatsUI();

        Debug.Log("Диалог запущен");
    }

    public void CloseDialog()
    {
        isDialogActive = false;
        SetAllPanelsActive(false);
        SaveProgress();

        Debug.Log("Диалог закрыт");
    }

    public bool IsDialogActive()
    {
        return isDialogActive;
    }

    void ContinueDialog()
    {
        if (isWaitingForPlayerAction) return;

        progress.currentDialogStep++;
        ShowCurrentDialogStep();
    }

    void SkipDialog()
    {
        if (isWaitingForPlayerAction) return;

        progress.currentDialogStep++;
        if (progress.currentDialogStep >= dialogSteps.Count)
        {
            EndDialog();
        }
        else
        {
            ShowCurrentDialogStep();
        }
    }

    void ShowCurrentDialogStep()
    {
        if (progress.currentDialogStep >= dialogSteps.Count)
        {
            EndDialog();
            return;
        }

        var step = dialogSteps[progress.currentDialogStep];
        dialogText.text = step.text;
        speakerNameText.text = step.speaker;

        if (step.action != DialogAction.None)
        {
            ExecuteDialogAction(step.action);
        }
    }

    void ExecuteDialogAction(DialogAction action)
    {
        switch (action)
        {
            case DialogAction.StartWordLearning:
                StartWordLearning();
                break;
            case DialogAction.StartSentenceExercise:
                StartSentenceExercise(0);
                break;
            case DialogAction.ContinueSentenceExercise:
                StartSentenceExercise(currentExerciseIndex + 1);
                break;
            case DialogAction.StartToExercise:
                StartToExercise(0);
                break;
            case DialogAction.ContinueToExercise:
                StartToExercise(currentExerciseIndex + 1);
                break;
            case DialogAction.FinalSentenceExercise:
                StartSentenceExercise(sentenceExercises.Count - 1);
                break;
            case DialogAction.StartPronunciationExercise:
                StartPronunciationExercise();
                break;
            case DialogAction.StartTranslationChoice:
                StartTranslationChoice();
                break;
            case DialogAction.ShowReward:
                ShowReward(50, 25);
                break;
            case DialogAction.ShowFinalReward:
                ShowReward(100, 50);
                break;
        }
    }

    void ShowReward(int xp, int coins)
    {
        ShowExercisePanel(rewardPanel);
        progress.currentXP += xp;
        progress.currentCoins += coins;

        rewardText.text = $"Поздравляем!\n+{xp} опыта\n+{coins} монет";
        UpdateStatsUI();

        StartCoroutine(CompleteExerciseAfterDelay(3f));
    }

    void UpdateStatsUI()
    {
        xpText.text = $"Опыт: {progress.currentXP}";
        coinsText.text = $"Монеты: {progress.currentCoins}";
    }

    void EndDialog()
    {
        CloseDialog();
        Debug.Log("Все задания завершены!");
    }

    void SetAllPanelsActive(bool active)
    {
        dialogPanel.SetActive(active);
        wordLearningPanel.SetActive(active);
        sentenceExercisePanel.SetActive(active);
        toExercisePanel.SetActive(active);
        pronunciationPanel.SetActive(active);
        translationChoicePanel.SetActive(active);
        rewardPanel.SetActive(active);
    }

    void PlayCurrentWordSound()
    {
        if (currentWord != null && currentWord.pronunciation != null)
        {
            audioSource.PlayOneShot(currentWord.pronunciation);
        }
    }

    void SetButtonColor(Button button, Color color)
    {
        if (button != null)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = color;
            }
        }
    }

    void ResetButtonColor(Button button)
    {
        if (button != null)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Исходный синий цвет #3366CC
                buttonImage.color = new Color(0.2f, 0.4f, 0.8f);
            }
        }
    }
    void ResetSentence()
    {
        constructedWords.Clear();
        constructedSentenceText.text = "";
        sentenceFeedbackText.text = "";
        constructedSentenceText.color = Color.white;
    }

    void CreateWordButtons()
    {
        // Очищаем старые кнопки
        foreach (var button in currentWordButtons)
        {
            if (button != null)
                Destroy(button);
        }
        currentWordButtons.Clear();

        // Проверяем что есть варианты слов
        if (currentSentenceExercise.wordOptions == null || currentSentenceExercise.wordOptions.Length == 0)
        {
            Debug.LogError("Нет вариантов слов для упражнения!");
            return;
        }

        Debug.Log($"Создаем кнопки для упражнения. Вариантов: {currentSentenceExercise.wordOptions.Length}");

        // Перемешиваем массив слов
        var shuffledWords = currentSentenceExercise.wordOptions.OrderBy(x => UnityEngine.Random.value).ToArray();

        // Создаем кнопки для каждого слова
        foreach (var word in shuffledWords)
        {
            if (wordButtonPrefab == null)
            {
                Debug.LogError("WordButtonPrefab не назначен!");
                continue;
            }

            if (wordButtonContainer == null)
            {
                Debug.LogError("WordButtonContainer не назначен!");
                continue;
            }

            try
            {
                var buttonObj = Instantiate(wordButtonPrefab, wordButtonContainer);
                var button = buttonObj.GetComponent<Button>();
                var text = buttonObj.GetComponentInChildren<TMP_Text>();

                if (text != null)
                {
                    text.text = word;
                }
                else
                {
                    Debug.LogError("Не найден TMP_Text на кнопке!");
                }

                // Добавляем обработчик клика
                button.onClick.AddListener(() => AddWordToSentence(word));
                currentWordButtons.Add(buttonObj);

                Debug.Log($"Создана кнопка: {word}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка при создании кнопки для слова '{word}': {e.Message}");
            }
        }

        Debug.Log($"Всего создано кнопок: {currentWordButtons.Count}");
    }

    void AddWordToSentence(string word)
    {
        constructedWords.Add(word);
        constructedSentenceText.text = string.Join(" ", constructedWords);
    }

    // Публичные методы для настройки из инспектора
    public void SetResetProgress(bool reset)
    {
        resetProgressOnStart = reset;
    }

    public void SetRequiredAccuracy(float accuracy)
    {
        requiredAccuracy = Mathf.Clamp01(accuracy);
    }

    public void SetUseRealSpeechRecognition(bool useReal)
    {
        useRealSpeechRecognition = useReal;
    }
}

public class DialogStep
{
    public string text;
    public string speaker;
    public DialogAction action;

    public DialogStep(string text, string speaker, DialogAction action = DialogAction.None)
    {
        this.text = text;
        this.speaker = speaker;
        this.action = action;
    }
}

public enum DialogAction
{
    None,
    StartWordLearning,
    StartSentenceExercise,
    ContinueSentenceExercise,
    StartToExercise,
    ContinueToExercise,
    FinalSentenceExercise,
    StartPronunciationExercise,
    StartTranslationChoice,
    ShowReward,
    ShowFinalReward
}