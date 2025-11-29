using UnityEngine;

public class WordPractice : MonoBehaviour
{
    public string sentenceTemplate = "i want ____ eat";
    public string correctWord = "to";
    public ChoiceButton3D[] options;

    void Start()
    {
        for (int i = 0; i < options.Length; i++)
        {
            int id = i;
            options[i].onClick = () => Check(options[id]);
        }
    }

    void Check(ChoiceButton3D btn)
    {
        if (btn.label.text.Trim().ToLower() == correctWord)
        {
            btn.MarkCorrect();
        }
        else
        {
            btn.MarkWrong();
        }
    }
}
