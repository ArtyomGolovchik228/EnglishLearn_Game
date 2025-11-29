using UnityEngine;
using TMPro;

public class HologramDisplay : MonoBehaviour
{
    [Header("Hologram Settings")]
    public Transform playerTransform;
    public float rotationSpeed = 5f;
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 1f;
    
    [Header("Face To Face Settings")]
    public bool faceToFace = true;
    public Vector3 faceOffset = Vector3.zero;
    public bool maintainUpright = true;
    public float maxTiltAngle = 15f;
    
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI infoText;
    public CanvasGroup canvasGroup;
    
    [Header("Visual Effects")]
    public Renderer hologramRenderer;
    public Material hologramMaterial;
    public Color hologramColor = Color.cyan;
    
    [Header("Billboard Settings")]
    public bool useBillboard = true;
    public BillboardType billboardType = BillboardType.FaceToCamera;
    
    private Vector3 startPosition;
    private Material originalMaterial;
    private Camera playerCamera;

    public enum BillboardType
    {
        FaceToCamera,       // Просто смотреть на камеру
        FaceToFace,         // Лицом к лицу (зеркально)
        VerticalBillboard,  // Только по вертикали
        FullBillboard       // Полное отслеживание
    }

    void Start()
    {
        // Сохраняем начальную позицию для парения
        startPosition = transform.position;
        
        // Находим игрока и камеру
        FindPlayerAndCamera();
        
        // Сохраняем оригинальный материал
        if (hologramRenderer != null)
        {
            originalMaterial = hologramRenderer.material;
        }
        
        InitializeHologram();
    }

    void Update()
    {
        // Поворачиваемся к игроку
        FacePlayer();
        
        // Эффект парения
        FloatEffect();
        
        // Пульсация прозрачности
        PulseEffect();
    }

    void LateUpdate()
    {
        // Дополнительный поворот в LateUpdate для плавности
        if (useBillboard && playerCamera != null)
        {
            ApplyBillboard();
        }
    }

    private void FindPlayerAndCamera()
    {
        // Ищем VR камеру или основную камеру
        GameObject vrCamera = GameObject.Find("VR Camera") ?? GameObject.Find("Camera");
        if (vrCamera != null)
        {
            playerTransform = vrCamera.transform;
            playerCamera = vrCamera.GetComponent<Camera>();
        }
        else
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerTransform = mainCamera.transform;
                playerCamera = mainCamera;
            }
        }
        
        // Если камеру не нашли, пытаемся найти на playerTransform
        if (playerCamera == null && playerTransform != null)
        {
            playerCamera = playerTransform.GetComponent<Camera>();
        }
        
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform not found! Please assign manually.");
        }
    }

    private void FacePlayer()
    {
        if (playerTransform == null) return;
        
        switch (billboardType)
        {
            case BillboardType.FaceToCamera:
                FaceToCamera();
                break;
            case BillboardType.FaceToFace:
                FaceToFace();
                break;
            case BillboardType.VerticalBillboard:
                VerticalBillboard();
                break;
            case BillboardType.FullBillboard:
                FullBillboard();
                break;
        }
    }

    private void FaceToCamera()
    {
        // Простой поворот к камере
        Vector3 direction = playerTransform.position - transform.position;
        if (maintainUpright)
        {
            direction.y = 0; // Игнорируем разницу по высоте
        }

        if (direction != Vector3.zero)
        {
            // УБРАЛ МИНУС - теперь смотрим НА камеру, а не ОТ камеры
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }

    private void FaceToFace()
    {
        if (playerCamera == null) return;

        Vector3 direction = playerTransform.position - transform.position;

        if (maintainUpright)
        {
            direction.y = 0;
        }

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        // ОТДЕЛЬНО поворачиваем Canvas чтобы он всегда смотрел на камеру
        if (canvasGroup != null)
        {
            Vector3 toCamera = playerCamera.transform.position - canvasGroup.transform.position;
            canvasGroup.transform.rotation = Quaternion.LookRotation(toCamera);
        }
    }

    private void VerticalBillboard()
    {
        // Billboard только по вертикальной оси
        if (playerCamera == null) return;

        Vector3 targetPos = playerCamera.transform.position;
        targetPos.y = transform.position.y; // Сохраняем одинаковую высоту

        Vector3 direction = targetPos - transform.position;

        if (direction != Vector3.zero)
        {
            // УБРАЛ МИНУС
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }

    private void FullBillboard()
    {
        // Полный billboard - всегда прямо к камере
        if (playerCamera == null) return;

        transform.rotation = Quaternion.Slerp(transform.rotation, 
            playerCamera.transform.rotation, 
            rotationSpeed * Time.deltaTime);
    }

    private void ApplyBillboard()
    {
        if (playerCamera == null) return;

        if (canvasGroup != null)
        {
            Transform canvasTransform = canvasGroup.transform;

            // Canvas всегда смотрит прямо на камеру, компенсируя поворот родителя
            canvasTransform.LookAt(playerCamera.transform);

            // Дополнительный поворот на 180 градусов чтобы убрать зеркальность
            canvasTransform.Rotate(0, 180f, 0);
        }
    }

    private void FloatEffect()
    {
        // Эффект плавного парения
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void PulseEffect()
    {
        if (canvasGroup != null)
        {
            // Пульсация прозрачности
            float alpha = 0.7f + Mathf.Sin(Time.time * 2f) * 0.3f;
            canvasGroup.alpha = alpha;
        }
    }

    private void InitializeHologram()
    {
        // Настраиваем материал голограммы
        if (hologramMaterial != null && hologramRenderer != null)
        {
            hologramRenderer.material = hologramMaterial;
            hologramRenderer.material.color = hologramColor;
        }
        
        // Устанавливаем начальный текст
        if (titleText != null)
            titleText.text = "ГОЛОГРАММА";
            
        if (infoText != null)
            infoText.text = "Информационная панель\nСтатус: АКТИВНА";
    }

    // Публичные методы для управления голограммой
    public void SetTitle(string newTitle)
    {
        if (titleText != null)
            titleText.text = newTitle;
    }

    public void SetInfo(string newInfo)
    {
        if (infoText != null)
            infoText.text = newInfo;
    }

    public void SetHologramColor(Color newColor)
    {
        hologramColor = newColor;
        if (hologramRenderer != null)
        {
            hologramRenderer.material.color = hologramColor;
        }
    }

    public void SetBillboardType(BillboardType type)
    {
        billboardType = type;
    }

    public void SetFaceToFace(bool enable)
    {
        faceToFace = enable;
        if (enable)
        {
            billboardType = BillboardType.FaceToFace;
        }
    }

    public void ShowHologram()
    {
        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void HideHologram()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        gameObject.SetActive(false);
    }

    // Метод для принудительного обновления поворота
    public void ForceUpdateRotation()
    {
        FacePlayer();
        ApplyBillboard();
    }

    // Для отладки в редакторе
    [ContextMenu("Test Face To Face")]
    public void TestFaceToFace()
    {
        SetFaceToFace(true);
        ForceUpdateRotation();
    }

    [ContextMenu("Test Standard Billboard")]
    public void TestStandardBillboard()
    {
        SetBillboardType(BillboardType.FaceToCamera);
        ForceUpdateRotation();
    }

    [ContextMenu("Find Player Camera")]
    public void FindCamera()
    {
        FindPlayerAndCamera();
        if (playerCamera != null)
        {
            Debug.Log($"Found camera: {playerCamera.name}");
        }
        else
        {
            Debug.LogWarning("Camera not found!");
        }
    }
}