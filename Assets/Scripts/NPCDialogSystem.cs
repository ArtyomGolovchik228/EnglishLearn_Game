using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum DialogType
{
    Monologue,          // Простой монолог
    WordChoice,         // Выбор правильного слова
    SentenceBuilder,    // Составление предложения
    ItemDelivery        // Доставка предмета
}

[System.Serializable]
public class DialogScenario
{
    public DialogType type;
    public string speakerName;

    [Header("Monologue Settings")]
    public string[] monologueLines;

    [Header("Word Choice Settings")]
    public string wordChoiceSentence;
    public string correctWord;
    public string[] wordOptions;

    [Header("Sentence Builder Settings")]
    public string correctSentence;
    public string[] availableWords;

    [Header("Item Delivery Settings")]
    public string requiredItem;
    public string[] requestLines;
    public string[] successLines;
    public string[] failureLines;
}

public class NPCDialogSystem : MonoBehaviour
{
    [Header("UI References")]
    public Canvas dialogCanvas;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogText;
    public Button continueButton;

    [Header("Word Choice UI")]
    public GameObject wordChoicePanel;
    public Button[] wordChoiceButtons;

    [Header("Sentence Builder UI")]
    public GameObject sentenceBuilderPanel;
    public Button[] wordButtons;
    public Button[] sentenceSlots;
    public Button submitSentenceButton;
    public Button clearSentenceButton;

    [Header("Item Delivery System")]
    //public ItemDeliveryZone deliveryZone;
    public GameObject itemSlotUI;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public KeyCode continueKey = KeyCode.Space;

    // Приватные переменные
    private DialogScenario currentScenario;
    private int currentLine = 0;
    private bool isTyping = false;
    private bool dialogActive = false;

    // Word Choice переменные
    private string currentSentence;
    private string correctWord;

    // Sentence Builder переменные
    private List<string> currentWords = new List<string>();
    private List<string> availableWords = new List<string>();
    private string targetSentence;

    // Item Delivery переменные
    private bool waitingForItem = false;

    void Start()
    {
        // Скрываем все UI элементы
        dialogCanvas.gameObject.SetActive(false);
        wordChoicePanel.SetActive(false);
        sentenceBuilderPanel.SetActive(false);
        if (itemSlotUI != null) itemSlotUI.SetActive(false);

        // Настраиваем кнопки
        continueButton.onClick.AddListener(ContinueDialog);
        SetupWordChoiceButtons();
        SetupSentenceBuilderButtons();
    }

    void Update()
    {
        if (!dialogActive) return;

        if (Input.GetKeyDown(continueKey))
        {
            ContinueDialog();
        }
    }

    // 📢 ОСНОВНОЙ МЕТОД ДЛЯ ЗАПУСКА ДИАЛОГА
    public void DialogShow(DialogType type, string speakerName, params object[] args)
    {
        currentScenario = CreateScenario(type, speakerName, args);
        StartDialog(currentScenario);
    }

    // 📢 Упрощенный метод с готовым сценарием
    public void DialogShow(DialogScenario scenario)
    {
        currentScenario = scenario;
        StartDialog(scenario);
    }

    private void StartDialog(DialogScenario scenario)
    {
        currentLine = 0;
        dialogActive = true;

        // Активируем основной Canvas
        dialogCanvas.gameObject.SetActive(true);

        // Скрываем все специализированные панели
        wordChoicePanel.SetActive(false);
        sentenceBuilderPanel.SetActive(false);
        if (itemSlotUI != null) itemSlotUI.SetActive(false);

        switch (scenario.type)
        {
            case DialogType.Monologue:
                StartMonologue();
                break;
            case DialogType.WordChoice:
                StartWordChoice();
                break;
            case DialogType.SentenceBuilder:
                StartSentenceBuilder();
                break;
            case DialogType.ItemDelivery:
                StartItemDelivery();
                break;
        }
    }

    #region 🎭 MONOLOGUE
    private void StartMonologue()
    {
        speakerText.text = currentScenario.speakerName;
        DisplayLine(currentScenario.monologueLines[0]);
    }
    #endregion

    #region 🔤 WORD CHOICE
    private void StartWordChoice()
    {
        speakerText.text = currentScenario.speakerName;
        currentSentence = currentScenario.wordChoiceSentence;
        correctWord = currentScenario.correctWord;

        // Настраиваем кнопки выбора
        for (int i = 0; i < wordChoiceButtons.Length; i++)
        {
            if (i < currentScenario.wordOptions.Length)
            {
                wordChoiceButtons[i].gameObject.SetActive(true);
                wordChoiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentScenario.wordOptions[i];
            }
            else
            {
                wordChoiceButtons[i].gameObject.SetActive(false);
            }
        }

        // Показываем базовый текст
        dialogText.text = currentSentence.Replace("___", "_____");
        wordChoicePanel.SetActive(true);
    }

    private void SetupWordChoiceButtons()
    {
        for (int i = 0; i < wordChoiceButtons.Length; i++)
        {
            int index = i;
            wordChoiceButtons[i].onClick.AddListener(() => OnWordSelected(index));
        }
    }

    private void OnWordSelected(int wordIndex)
    {
        string selectedWord = wordChoiceButtons[wordIndex].GetComponentInChildren<TextMeshProUGUI>().text;
        bool isCorrect = selectedWord == correctWord;

        // Показываем результат
        string resultSentence = currentSentence.Replace("___",
            $"<color={(isCorrect ? "green" : "red")}>{selectedWord}</color>");
        dialogText.text = resultSentence;

        // Блокируем кнопки
        foreach (var button in wordChoiceButtons)
        {
            button.interactable = false;
        }

        StartCoroutine(CompleteWordChoice(isCorrect));
    }

    private IEnumerator CompleteWordChoice(bool success)
    {
        yield return new WaitForSeconds(2f);
        EndDialog();
    }
    #endregion

    #region 🧩 SENTENCE BUILDER
    private void StartSentenceBuilder()
    {
        speakerText.text = currentScenario.speakerName;
        targetSentence = currentScenario.correctSentence;

        // Инициализируем слова
        availableWords = new List<string>(currentScenario.availableWords);
        currentWords.Clear();

        UpdateWordButtons();
        UpdateSentenceSlots();

        sentenceBuilderPanel.SetActive(true);
        dialogText.text = "Составьте правильное предложение:";
    }

    private void SetupSentenceBuilderButtons()
    {
        // Кнопки слов
        for (int i = 0; i < wordButtons.Length; i++)
        {
            int index = i;
            wordButtons[i].onClick.AddListener(() => OnWordButtonClick(index));
        }

        // Слоты предложения
        for (int i = 0; i < sentenceSlots.Length; i++)
        {
            int index = i;
            sentenceSlots[i].onClick.AddListener(() => OnSentenceSlotClick(index));
        }

        submitSentenceButton.onClick.AddListener(CheckSentence);
        clearSentenceButton.onClick.AddListener(ClearSentence);
    }

    private void OnWordButtonClick(int index)
    {
        if (index >= availableWords.Count) return;

        string word = availableWords[index];
        currentWords.Add(word);
        availableWords.RemoveAt(index);

        UpdateWordButtons();
        UpdateSentenceSlots();
    }

    private void OnSentenceSlotClick(int index)
    {
        if (index >= currentWords.Count) return;

        string word = currentWords[index];
        availableWords.Add(word);
        currentWords.RemoveAt(index);

        UpdateWordButtons();
        UpdateSentenceSlots();
    }

    private void CheckSentence()
    {
        string userSentence = string.Join(" ", currentWords);
        bool isCorrect = userSentence.ToLower() == targetSentence.ToLower();

        dialogText.text = isCorrect ?
            "<color=green>Правильно! Отличная работа!</color>" :
            "<color=red>Неправильно! Попробуйте еще раз.</color>";

        if (isCorrect)
        {
            StartCoroutine(CompleteSentenceBuilder());
        }
    }

    private void ClearSentence()
    {
        availableWords.AddRange(currentWords);
        currentWords.Clear();
        UpdateWordButtons();
        UpdateSentenceSlots();
    }

    private void UpdateWordButtons()
    {
        for (int i = 0; i < wordButtons.Length; i++)
        {
            if (i < availableWords.Count)
            {
                wordButtons[i].gameObject.SetActive(true);
                wordButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = availableWords[i];
            }
            else
            {
                wordButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSentenceSlots()
    {
        for (int i = 0; i < sentenceSlots.Length; i++)
        {
            if (i < currentWords.Count)
            {
                sentenceSlots[i].gameObject.SetActive(true);
                sentenceSlots[i].GetComponentInChildren<TextMeshProUGUI>().text = currentWords[i];
            }
            else
            {
                sentenceSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator CompleteSentenceBuilder()
    {
        yield return new WaitForSeconds(2f);
        EndDialog();
    }
    #endregion

    #region 📦 ITEM DELIVERY
    private void StartItemDelivery()
    {
        speakerText.text = currentScenario.speakerName;

        // Показываем запрос предмета
        DisplayLine(currentScenario.requestLines[0]);

        // Активируем зону доставки
        //if (deliveryZone != null)
        //{
        //    deliveryZone.StartWaitingForItem(currentScenario.requiredItem, this);
        //}

        if (itemSlotUI != null) itemSlotUI.SetActive(true);
    }

    public void OnItemDelivered(string itemName, bool isCorrect)
    {
        if (!waitingForItem) return;

        waitingForItem = false;

        if (isCorrect)
        {
            // Предмет правильный
            currentLine = 0;
            DisplayLines(currentScenario.successLines);
        }
        else
        {
            // Предмет неправильный
            currentLine = 0;
            DisplayLines(currentScenario.failureLines);
        }

        if (itemSlotUI != null) itemSlotUI.SetActive(false);
    }

    private void DisplayLines(string[] lines)
    {
        StartCoroutine(DisplayLinesCoroutine(lines));
    }

    private IEnumerator DisplayLinesCoroutine(string[] lines)
    {
        foreach (string line in lines)
        {
            speakerText.text = currentScenario.speakerName;
            yield return StartCoroutine(TypeText(line));
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(2f);
        EndDialog();
    }
    #endregion

    #region 🛠️ ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    private DialogScenario CreateScenario(DialogType type, string speakerName, object[] args)
    {
        var scenario = new DialogScenario { type = type, speakerName = speakerName };

        switch (type)
        {
            case DialogType.Monologue:
                scenario.monologueLines = (string[])args[0];
                break;
            case DialogType.WordChoice:
                scenario.wordChoiceSentence = (string)args[0];
                scenario.correctWord = (string)args[1];
                scenario.wordOptions = (string[])args[2];
                break;
            case DialogType.SentenceBuilder:
                scenario.correctSentence = (string)args[0];
                scenario.availableWords = (string[])args[1];
                break;
            case DialogType.ItemDelivery:
                scenario.requiredItem = (string)args[0];
                scenario.requestLines = (string[])args[1];
                scenario.successLines = (string[])args[2];
                scenario.failureLines = (string[])args[3];
                break;
        }

        return scenario;
    }

    private void DisplayLine(string line)
    {
        StartCoroutine(TypeText(line));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void ContinueDialog()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogText.text = GetCurrentLine();
            isTyping = false;
            return;
        }

        currentLine++;

        if (currentScenario.type == DialogType.Monologue)
        {
            if (currentLine < currentScenario.monologueLines.Length)
            {
                DisplayLine(currentScenario.monologueLines[currentLine]);
            }
            else
            {
                EndDialog();
            }
        }
    }

    private string GetCurrentLine()
    {
        return currentScenario.type switch
        {
            DialogType.Monologue => currentScenario.monologueLines[currentLine],
            _ => dialogText.text
        };
    }

    private void EndDialog()
    {
        dialogActive = false;
        dialogCanvas.gameObject.SetActive(false);

        // Отключаем зону доставки
        //if (deliveryZone != null)
        //{
        //    deliveryZone.StopWaiting();
        //}
    }
    #endregion

    #region 🎯 ПРИМЕРЫ ИСПОЛЬЗОВАНИЯ
    [ContextMenu("Test Monologue")]
    public void TestMonologue()
    {
        string[] lines = {
            "Привет! Я тестовый NPC.",
            "Это простой монолог.",
            "Нажми SPACE для продолжения."
        };
        DialogShow(DialogType.Monologue, "Тестовый NPC", lines);
    }

    [ContextMenu("Test Word Choice")]
    public void TestWordChoice()
    {
        string sentence = "Меня зовут ___";
        string correct = "Джон";
        string[] options = { "Джон", "Майк", "Том", "Сэм" };
        DialogShow(DialogType.WordChoice, "Учитель", sentence, correct, options);
    }

    [ContextMenu("Test Sentence Builder")]
    public void TestSentenceBuilder()
    {
        string correct = "Я люблю программирование";
        string[] words = { "люблю", "Я", "игры", "программирование", "ненавижу" };
        DialogShow(DialogType.SentenceBuilder, "Лингвист", correct, words);
    }
    #endregion
}