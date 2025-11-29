using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class ChoiceButton3D : MonoBehaviour
{
    public TextMeshPro label;
    public Renderer plate;
    public Color normal = Color.white, hover = Color.cyan, correct = Color.green, wrong = Color.red;
    public System.Action onClick;

    bool isLocked;

    void OnMouseEnter() { if (!isLocked) plate.material.color = hover; }
    void OnMouseExit() { if (!isLocked) plate.material.color = normal; }
    void OnMouseDown() { if (!isLocked) onClick?.Invoke(); }

    public void MarkCorrect() { plate.material.color = correct; isLocked = true; }
    public void MarkWrong() { plate.material.color = wrong; isLocked = true; }
    public void ResetButton() { isLocked = false; plate.material.color = normal; }
}
