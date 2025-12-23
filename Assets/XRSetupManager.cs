using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class XRSetupManager : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActions;

    [Header("Настройки передвижения")]
    public float moveSpeed = 2f;
    public float turnSpeed = 60f;
    public float snapTurnAngle = 45f;

    [Header("Контроллеры")]
    public GameObject leftController;
    public GameObject rightController;

    private XROrigin xrOrigin;
    private ActionBasedController leftControllerComp;
    private ActionBasedController rightControllerComp;

    void Start()
    {
        SetupXR();
        SetupControllers();
        SetupInputActions();
        SetupLocomotion();
        Debug.Log("XR настройка завершена!");
    }

    void SetupXR()
    {
        // Находим XR Origin
        xrOrigin = GetComponent<XROrigin>();
        if (xrOrigin == null)
        {
            xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("XR Origin не найден в сцене!");
                return;
            }
        }

        // Настройка Origin
        xrOrigin.CameraFloorOffsetObject = xrOrigin.transform.Find("Camera Offset")?.gameObject;

        // Убедимся что есть Camera
        if (xrOrigin.Camera == null)
        {
            var camera = xrOrigin.GetComponentInChildren<Camera>();
            xrOrigin.Camera = camera;
        }
    }

    void SetupControllers()
    {
        // Ищем контроллеры
        ActionBasedController[] controllers = GetComponentsInChildren<ActionBasedController>();

        foreach (var controller in controllers)
        {
            if (controller.name.Contains("Left") || controller.tag == "LeftController")
            {
                leftControllerComp = controller;
                leftController = controller.gameObject;
                SetupControllerInteractor(leftController, true);
            }
            else if (controller.name.Contains("Right") || controller.tag == "RightController")
            {
                rightControllerComp = controller;
                rightController = controller.gameObject;
                SetupControllerInteractor(rightController, false);
            }
        }

        if (leftControllerComp == null || rightControllerComp == null)
        {
            Debug.LogWarning("Контроллеры не найдены, создаем автоматически...");
            CreateDefaultControllers();
        }
    }

    void SetupControllerInteractor(GameObject controller, bool isLeft)
    {
        // Добавляем Ray Interactor если нет
        XRRayInteractor rayInteractor = controller.GetComponent<XRRayInteractor>();
        if (rayInteractor == null)
        {
            rayInteractor = controller.AddComponent<XRRayInteractor>();

            // Настройки Ray Interactor
            rayInteractor.maxRaycastDistance = 10f;
            rayInteractor.raycastMask = LayerMask.GetMask("Default", "UI", "Interactable");
            rayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
            rayInteractor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.StateChange;

            // Визуальная линия
            LineRenderer lineRenderer = controller.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = controller.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.005f;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.cyan;
                lineRenderer.endColor = Color.blue;
            }
        }

        // Добавляем Direct Interactor для прямого взаимодействия
        XRDirectInteractor directInteractor = controller.GetComponent<XRDirectInteractor>();
        if (directInteractor == null)
        {
            directInteractor = controller.AddComponent<XRDirectInteractor>();
        }

        // Добавляем или находим XR Controller
        ActionBasedController abController = controller.GetComponent<ActionBasedController>();
        if (abController == null)
        {
            abController = controller.AddComponent<ActionBasedController>();
        }

        // Настраиваем кнопки
        SetupControllerInput(abController, isLeft);
    }

    void SetupControllerInput(ActionBasedController controller, bool isLeft)
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Action Asset не назначен!");
            return;
        }

        string handPrefix = isLeft ? "Left" : "Right";

        // Настраиваем Input Actions
        var positionAction = inputActions.FindAction($"XRI {handPrefix}Hand/Position");
        var rotationAction = inputActions.FindAction($"XRI {handPrefix}Hand/Rotation");
        var selectAction = inputActions.FindAction($"XRI {handPrefix}Hand/Select");
        var activateAction = inputActions.FindAction($"XRI {handPrefix}Hand/Activate");
        var uiAction = inputActions.FindAction($"XRI {handPrefix}Hand/UI Press");

        if (positionAction != null) controller.positionAction = new InputActionProperty(positionAction);
        if (rotationAction != null) controller.rotationAction = new InputActionProperty(rotationAction);
        if (selectAction != null) controller.selectAction = new InputActionProperty(selectAction);
        if (activateAction != null) controller.activateAction = new InputActionProperty(activateAction);
        if (uiAction != null) controller.uiPressAction = new InputActionProperty(uiAction);
    }

    void SetupInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Action Asset не назначен!");
            return;
        }

        // Включаем все Action Maps
        foreach (var actionMap in inputActions.actionMaps)
        {
            actionMap.Enable();
        }
    }

    void SetupLocomotion()
    {
        // Continuous Move Provider (плавное передвижение джойстиком)
        var continuousMove = GetComponent<ActionBasedContinuousMoveProvider>();
        if (continuousMove == null)
        {
            continuousMove = gameObject.AddComponent<ActionBasedContinuousMoveProvider>();
        }

        // Настройка передвижения
        continuousMove.moveSpeed = moveSpeed;
        continuousMove.enableStrafe = true;
        continuousMove.useGravity = true;

        // Привязываем Input Action для движения (левая рука)
        if (inputActions != null)
        {
            var moveAction = inputActions.FindAction("XRI LeftHand/Move");
            if (moveAction != null)
            {
                continuousMove.leftHandMoveAction = new InputActionProperty(moveAction);
            }
        }

        // Continuous Turn Provider (плавный поворот)
        var continuousTurn = GetComponent<ActionBasedContinuousTurnProvider>();
        if (continuousTurn == null)
        {
            continuousTurn = gameObject.AddComponent<ActionBasedContinuousTurnProvider>();
        }

        continuousTurn.turnSpeed = turnSpeed;

        // Привязываем Input Action для поворота (правая рука)
        if (inputActions != null)
        {
            var turnAction = inputActions.FindAction("XRI RightHand/Move");
            if (turnAction != null)
            {
                continuousTurn.rightHandTurnAction = new InputActionProperty(turnAction);
            }
        }

        // Snap Turn Provider (резкий поворот)
        var snapTurn = GetComponent<ActionBasedSnapTurnProvider>();
        if (snapTurn == null)
        {
            snapTurn = gameObject.AddComponent<ActionBasedSnapTurnProvider>();
        }

        snapTurn.turnAmount = snapTurnAngle;

        // Привязываем Input Action для Snap Turn
        if (inputActions != null)
        {
            var snapTurnAction = inputActions.FindAction("XRI RightHand/Primary");
            if (snapTurnAction != null)
            {
                snapTurn.rightHandSnapTurnAction = new InputActionProperty(snapTurnAction);
            }
        }
    }

    void CreateDefaultControllers()
    {
        // Создаем Camera Offset если нет
        Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
        if (cameraOffset == null)
        {
            cameraOffset = new GameObject("Camera Offset").transform;
            cameraOffset.SetParent(xrOrigin.transform);
            cameraOffset.localPosition = Vector3.zero;
        }

        // Создаем левый контроллер
        if (leftController == null)
        {
            leftController = new GameObject("LeftHand Controller");
            leftController.transform.SetParent(cameraOffset);
            leftController.transform.localPosition = new Vector3(-0.2f, 0, 0);
            leftController.AddComponent<ActionBasedController>();
            SetupControllerInteractor(leftController, true);
        }

        // Создаем правый контроллер
        if (rightController == null)
        {
            rightController = new GameObject("RightHand Controller");
            rightController.transform.SetParent(cameraOffset);
            rightController.transform.localPosition = new Vector3(0.2f, 0, 0);
            rightController.AddComponent<ActionBasedController>();
            SetupControllerInteractor(rightController, false);
        }
    }

    void Update()
    {
        // Отладочная информация
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DebugXRStatus();
        }
    }

    void DebugXRStatus()
    {
        Debug.Log("=== XR STATUS ===");
        Debug.Log($"XR Origin: {xrOrigin != null}");
        Debug.Log($"Left Controller: {leftControllerComp != null}");
        Debug.Log($"Right Controller: {rightControllerComp != null}");
        Debug.Log($"Input Actions: {inputActions != null}");

        if (leftControllerComp != null)
        {
            Debug.Log($"Left Select Action: {leftControllerComp.selectAction.action?.name}");
        }
    }
}