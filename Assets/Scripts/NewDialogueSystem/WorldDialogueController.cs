using UnityEngine;
using System.Collections;

public class WorldDialogueController : MonoBehaviour
{
    public DialogueSequence sequence;
    public SpeechBubble3D bubble;
    public AudioSource audioSource;
    public Transform npcHead;

    int currentStep = 0;
    bool playing;

    void Start()
    {
        if (sequence != null) StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        playing = true;
        foreach (var step in sequence.steps)
        {
            yield return StartCoroutine(PlayStep(step));
        }
        playing = false;
    }

    IEnumerator PlayStep(DialogueStep step)
    {
        bubble.ShowText(step.text);
        if (step.voiceOver) audioSource.PlayOneShot(step.voiceOver);

        switch (step.type)
        {
            case DialogueStepType.Line:
                yield return new WaitForSeconds(6f);
                break;

            case DialogueStepType.WaitForSpeech:
                bubble.ShowText(step.text + "\n(Скажи это вслух!)");
                yield return new WaitForSeconds(3f);
                break;

            case DialogueStepType.WordLesson:
                bubble.ShowText(step.text + "\n📖 Посмотри на слово и повтори.");
                yield return new WaitForSeconds(6f);
                break;

            case DialogueStepType.Practice:
                bubble.ShowText(step.text + "\n🤔 Попробуй составить правильную фразу!");
                yield return new WaitForSeconds(7f);
                break;

            case DialogueStepType.Reward:
                bubble.ShowText("🎉 " + step.text);
                // Здесь можно вызвать QuestManager.AddXP();
                yield return new WaitForSeconds(5f);
                break;
        }

        bubble.Hide();
        yield return new WaitForSeconds(0.5f);
    }
}
