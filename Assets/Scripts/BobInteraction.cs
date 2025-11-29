using UnityEngine;
using Valve.VR.InteractionSystem;

public class BobInteraction : MonoBehaviour
{

    [Header("Testing Settings")]
    public bool enableMouseTesting = true;


    private void Update()
    {
        // Обработка клика мыши для тестирования
        if (enableMouseTesting && Input.GetMouseButtonDown(0))
        {
            // Проверяем, кликнули ли по этому объекту
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    OnInteractableClicked(null);
                }
            }
        }
    }

    private void OnHandHoverBegin(Hand hand)
    {
        Debug.Log("Рука навелась на объект: " + hand.name);
    }

    private void OnHandHoverEnd(Hand hand)
    {
        Debug.Log("Рука ушла с объекта: " + hand.name);
    }

    private void OnInteractableClicked(Hand hand)
    {
        Debug.Log("Объект нажат! Рука: " + (hand != null ? hand.name : "Mouse"));
    }
}