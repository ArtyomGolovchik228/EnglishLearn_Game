using UnityEngine;

public enum DialogueStepType
{
    Line,            // простая реплика
    WaitForSpeech,   // ученик должен произнести слово
    WordLesson,      // карточка слова
    Practice,        // упражнение (составь, подставь)
    Reward           // награда (монетки, опыт)
}

[System.Serializable]
public class DialogueStep
{
    [TextArea(2, 4)] public string text;
    public AudioClip voiceOver;
    public DialogueStepType type;
}

[CreateAssetMenu(menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    public string sequenceName;
    public DialogueStep[] steps;
}
