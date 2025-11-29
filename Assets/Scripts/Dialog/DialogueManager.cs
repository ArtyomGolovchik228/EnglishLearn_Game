using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Dialogue
{
    public string speaker;
    [TextArea(3, 10)]
    public string text;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public Button continueButton;

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.05f;
    public KeyCode continueKey = KeyCode.Space;

    private Dialogue[] currentDialogue;
    private int currentLine = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;

    void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueDialogue);
        }

        // Скрываем канвас при старте
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetKeyDown(continueKey))
        {
            ContinueDialogue();
        }
    }

    public void StartDialogue(string interactableName)
    {
        currentDialogue = GetDialogueForInteractable(interactableName);
        currentLine = 0;
        dialogueActive = true;

        gameObject.SetActive(true);

        if (currentDialogue != null && currentDialogue.Length > 0)
        {
            DisplayLine(currentDialogue[0]);
        }
    }

    private Dialogue[] GetDialogueForInteractable(string interactableName)
    {
        return new Dialogue[]
        {
            new Dialogue {
                speaker = interactableName,
                text = $"Hello my name is {interactableName}."
            },
            new Dialogue {
                speaker = interactableName,
                text = "This is a basic monologue."
            },
            new Dialogue {
                speaker = interactableName,
                text = "You can press SPACE or button to continue."
            }
        };
    }

    private void DisplayLine(Dialogue dialogue)
    {
        if (speakerNameText != null)
            speakerNameText.text = dialogue.speaker;

        StartCoroutine(TypeText(dialogue.text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            if (dialogueText != null)
                dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void ContinueDialogue()
    {
        if (isTyping)
        {
            // Пропустить анимацию печати
            StopAllCoroutines();
            if (dialogueText != null && currentLine < currentDialogue.Length)
                dialogueText.text = currentDialogue[currentLine].text;
            isTyping = false;
            return;
        }

        currentLine++;

        if (currentLine < currentDialogue.Length)
        {
            DisplayLine(currentDialogue[currentLine]);
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        gameObject.SetActive(false);
        Debug.Log("Dialogue ended");
    }
}