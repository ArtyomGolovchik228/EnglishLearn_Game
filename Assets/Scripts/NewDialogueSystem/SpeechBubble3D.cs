using UnityEngine;
using TMPro;

public class SpeechBubble3D : MonoBehaviour
{
    public TextMeshPro textMesh;
    public CanvasGroup cg;
    public Transform followTarget;
    public float height = 2f;
    public float fadeSpeed = 4f;
    Camera cam;
    float targetAlpha;

    void Start() { cam = Camera.main; HideImmediate(); }

    void LateUpdate()
    {
        if (!followTarget) return;
        transform.position = followTarget.position + Vector3.up * height;
        if (cam) transform.forward = (transform.position - cam.transform.position).normalized;
        cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }

    public void ShowText(string t)
    {
        textMesh.text = t;
        targetAlpha = 1;
    }

    public void Hide() => targetAlpha = 0;
    public void HideImmediate() { cg.alpha = 0; targetAlpha = 0; }
}
