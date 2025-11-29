using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ChoiceDialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public GameObject choicesPanel;
    public Button[] choiceButtons;
    public TextMeshProUGUI[] choiceTexts;
    public Image feedbackImage;
    public TextMeshProUGUI feedbackText;

    [Header("Task Settings")]
    public string correctAnswer = "Victor";
    public string baseSentence = "My name is ___";
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public int maxAttempts = 3;

    [Header("Audio")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;

    private string currentSentence;
    private int attemptsLeft;
    private bool taskCompleted = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Скрываем панель выбора при старте
        choicesPanel.SetActive(false);
        feedbackImage.gameObject.SetActive(false);

        // Назначаем обработчики для кнопок
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i; // Важно для замыкания
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
        }
    }

    public void StartChoiceDialogue(string speakerName, string[] choices)
    {
        speakerNameText.text = speakerName;
        currentSentence = baseSentence;
        dialogueText.text = currentSentence;
        attemptsLeft = maxAttempts;
        taskCompleted = false;

        // Заполняем варианты ответов
        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (i < choices.Length)
            {
                choiceTexts[i].text = choices[i];
                choiceButtons[i].gameObject.SetActive(true);
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        choicesPanel.SetActive(true);
        feedbackImage.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (taskCompleted) return;

        string selectedAnswer = choiceTexts[choiceIndex].text;
        bool isCorrect = selectedAnswer == correctAnswer;

        // Обновляем предложение
        string completedSentence = baseSentence.Replace("___", $"<color={(isCorrect ? "green" : "red")}>{selectedAnswer}</color>");
        dialogueText.text = completedSentence;

        // Показываем обратную связь
        ShowFeedback(isCorrect, selectedAnswer);

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
        }
    }

    private void ShowFeedback(bool isCorrect, string answer)
    {
        feedbackImage.gameObject.SetActive(true);
        feedbackImage.color = isCorrect ? correctColor : incorrectColor;
        feedbackText.text = isCorrect ? "Правильно!" : $"Неправильно! Осталось попыток: {attemptsLeft}";

        // Проигрываем звук
        if (isCorrect && correctSound != null)
            audioSource.PlayOneShot(correctSound);
        else if (!isCorrect && incorrectSound != null)
            audioSource.PlayOneShot(incorrectSound);
    }

    private IEnumerator CompleteTask(bool success)
    {
        yield return new WaitForSeconds(2f);

        if (success)
        {
            dialogueText.text = $"<color=green>My name is {correctAnswer}</color>";
            feedbackText.text = "Отлично! Задание выполнено.";
        }
        else
        {
            dialogueText.text = $"Правильный ответ: <color=green>My name is {correctAnswer}</color>";
            feedbackText.text = "Попытки закончились!";
        }

        yield return new WaitForSeconds(3f);

        // Завершаем диалог
        EndDialogue();
    }

    private void EndDialogue()
    {
        gameObject.SetActive(false);
        choicesPanel.SetActive(false);
    }

    // Пример использования
    public void TestChoiceDialogue()
    {
        string[] choices = { "John", "Victor", "Michael", "Alex" };
        StartChoiceDialogue("NPC", choices);
    }
}