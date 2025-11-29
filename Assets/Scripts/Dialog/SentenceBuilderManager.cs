using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SentenceBuilderManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public GameObject wordSelectionPanel;
    public GameObject sentenceConstructionPanel;
    public Button[] wordButtons;
    public Button[] sentenceSlotButtons;
    public Button submitButton;
    public Button clearButton;
    public Image feedbackImage;
    public TextMeshProUGUI feedbackText;

    [Header("Task Settings")]
    public string correctSentence = "I love programming";
    public string taskDescription = "Составьте правильное предложение:";
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public int maxAttempts = 3;

    [Header("Audio")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip wordPlaceSound;
    public AudioClip wordRemoveSound;

    private List<string> availableWords = new List<string>();
    private List<string> currentSentenceWords = new List<string>();
    private string[] correctWords;
    private int attemptsLeft;
    private bool taskCompleted = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        InitializeUI();
        correctWords = correctSentence.Split(' ');
    }

    private void InitializeUI()
    {
        // Назначаем обработчики для кнопок слов
        for (int i = 0; i < wordButtons.Length; i++)
        {
            int index = i;
            wordButtons[i].onClick.AddListener(() => OnWordSelected(index));
        }

        // Назначаем обработчики для слотов предложения
        for (int i = 0; i < sentenceSlotButtons.Length; i++)
        {
            int index = i;
            sentenceSlotButtons[i].onClick.AddListener(() => OnSentenceWordClicked(index));
        }

        submitButton.onClick.AddListener(OnSubmitSentence);
        clearButton.onClick.AddListener(ClearSentence);

        wordSelectionPanel.SetActive(true);
        sentenceConstructionPanel.SetActive(true);
        feedbackImage.gameObject.SetActive(false);
    }

    public void StartSentenceBuilder(string speakerName, string[] words)
    {
        speakerNameText.text = speakerName;
        dialogueText.text = taskDescription;
        attemptsLeft = maxAttempts;
        taskCompleted = false;

        availableWords = new List<string>(words);
        currentSentenceWords = new List<string>();
        correctWords = correctSentence.Split(' ');

        // Заполняем кнопки слов
        UpdateWordButtons();
        UpdateSentenceSlots();

        wordSelectionPanel.SetActive(true);
        sentenceConstructionPanel.SetActive(true);
        feedbackImage.gameObject.SetActive(false);
        gameObject.SetActive(true);
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
        for (int i = 0; i < sentenceSlotButtons.Length; i++)
        {
            TextMeshProUGUI slotText = sentenceSlotButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            if (i < currentSentenceWords.Count)
            {
                sentenceSlotButtons[i].gameObject.SetActive(true);
                slotText.text = currentSentenceWords[i];
            }
            else
            {
                sentenceSlotButtons[i].gameObject.SetActive(false);
            }
        }

        // Обновляем отображаемое предложение
        string currentSentence = string.Join(" ", currentSentenceWords);
        dialogueText.text = $"{taskDescription}\n<size=120%>{currentSentence}</size>";
    }

    private void OnWordSelected(int wordIndex)
    {
        if (taskCompleted || wordIndex >= availableWords.Count) return;

        string selectedWord = availableWords[wordIndex];

        // Добавляем слово в предложение
        currentSentenceWords.Add(selectedWord);

        // Убираем слово из доступных
        availableWords.RemoveAt(wordIndex);

        UpdateWordButtons();
        UpdateSentenceSlots();

        if (wordPlaceSound != null)
            audioSource.PlayOneShot(wordPlaceSound);
    }

    private void OnSentenceWordClicked(int slotIndex)
    {
        if (taskCompleted || slotIndex >= currentSentenceWords.Count) return;

        string removedWord = currentSentenceWords[slotIndex];

        // Возвращаем слово в доступные
        availableWords.Add(removedWord);

        // Убираем слово из предложения
        currentSentenceWords.RemoveAt(slotIndex);

        UpdateWordButtons();
        UpdateSentenceSlots();

        if (wordRemoveSound != null)
            audioSource.PlayOneShot(wordRemoveSound);
    }

    private void OnSubmitSentence()
    {
        if (taskCompleted || currentSentenceWords.Count == 0) return;

        string userSentence = string.Join(" ", currentSentenceWords);
        bool isCorrect = userSentence.ToLower() == correctSentence.ToLower();

        // Показываем обратную связь
        ShowFeedback(isCorrect, userSentence);

        if (isCorrect)
        {
            taskCompleted = true;
            StartCoroutine(CompleteTask(true));
        }
        else
        {
            attemptsLeft--;
            if (attemptsLeft <= 0)
            {
                StartCoroutine(CompleteTask(false));
            }
            else
            {
                // Даем возможность исправить
                StartCoroutine(ShowRetryMessage());
            }
        }
    }

    private void ShowFeedback(bool isCorrect, string sentence)
    {
        feedbackImage.gameObject.SetActive(true);
        feedbackImage.color = isCorrect ? correctColor : incorrectColor;

        string coloredSentence = isCorrect ?
            $"<color=green>{sentence}</color>" :
            $"<color=red>{sentence}</color>";

        feedbackText.text = isCorrect ?
            "Правильно! Отличная работа!" :
            $"Неправильно! Осталось попыток: {attemptsLeft}\nВаш ответ: {coloredSentence}";

        // Проигрываем звук
        if (isCorrect && correctSound != null)
            audioSource.PlayOneShot(correctSound);
        else if (!isCorrect && incorrectSound != null)
            audioSource.PlayOneShot(incorrectSound);
    }

    private IEnumerator ShowRetryMessage()
    {
        yield return new WaitForSeconds(2f);
        feedbackImage.gameObject.SetActive(false);
    }

    private IEnumerator CompleteTask(bool success)
    {
        yield return new WaitForSeconds(2f);

        if (success)
        {
            feedbackText.text = "Задание выполнено успешно!";
            dialogueText.text = $"<color=green>{correctSentence}</color>";
        }
        else
        {
            feedbackText.text = "Попытки закончились!";
            dialogueText.text = $"Правильный ответ: <color=green>{correctSentence}</color>";
        }

        // Блокируем взаимодействие
        wordSelectionPanel.SetActive(false);
        submitButton.interactable = false;
        clearButton.interactable = false;

        yield return new WaitForSeconds(3f);

        EndDialogue();
    }

    private void ClearSentence()
    {
        if (taskCompleted) return;

        // Возвращаем все слова в доступные
        availableWords.AddRange(currentSentenceWords);
        currentSentenceWords.Clear();

        UpdateWordButtons();
        UpdateSentenceSlots();
    }

    private void EndDialogue()
    {
        gameObject.SetActive(false);
        wordSelectionPanel.SetActive(false);
        sentenceConstructionPanel.SetActive(false);
    }

    // Пример использования
    public void TestSentenceBuilder()
    {
        string[] words = { "love", "I", "games", "programming", "hate", "playing" };
        StartSentenceBuilder("Учитель", words);
    }
}