// Добавьте этот скрипт на ваш Canvas для лучшего billboard эффекта
using UnityEngine;

public class CanvasBillboard : MonoBehaviour
{
    public Camera playerCamera;
    public bool faceToFace = true;

    void LateUpdate()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        if (faceToFace)
        {
            // Зеркальный поворот для эффекта "лицом к лицу"
            transform.rotation = Quaternion.LookRotation(
                transform.position - playerCamera.transform.position,
                Vector3.up
            );
        }
        else
        {
            // Обычный billboard
            transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward,
                playerCamera.transform.rotation * Vector3.up);
        }
    }
}