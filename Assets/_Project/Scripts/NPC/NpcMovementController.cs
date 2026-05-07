using System;
using UnityEngine;
using UnityEngine.Events;

public class NpcMovementController : MonoBehaviour
{
    private enum MovementState
    {
        Idle,
        Entering,
        WaitingAtCenter,
        Exiting
    }

    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Animator npcAnimator;

    [Header("Speed")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 2.5f;

    [Header("Viewport Positions")]
    [SerializeField] private float startViewportX = -0.15f;
    [SerializeField] private float centerViewportX = 0.5f;
    [SerializeField] private float endViewportX = 1.15f;

    [Header("Behavior")]
    [SerializeField] private UnityEvent onArrivedCenter;
    
    [Header("Animation Parameters")]
    [SerializeField] private string movingBoolParameter = "IsMoving";

    private const float ArriveThresholdSqr = 0.0001f;
    private MovementState currentState = MovementState.Idle;
    private float fixedY;
    private float distanceFromCamera;
    private bool hasMovingBoolParameter;
    private int movingBoolHash;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("NpcMovementController precisa de uma Camera para converter viewport em mundo.", this);
            enabled = false;
            return;
        }

        CacheMovementPlane();
        CacheAnimatorSetup();
        SetMovingAnimation(false);
    }

    private void OnEnable()
    {
        NpcMovementEvents.NextNpc += HandleNextNpc;
        NpcMovementEvents.EndNpc += HandleEndNpc;
    }

    private void OnDisable()
    {
        NpcMovementEvents.NextNpc -= HandleNextNpc;
        NpcMovementEvents.EndNpc -= HandleEndNpc;
    }

    private void Update()
    {
        if (currentState == MovementState.Entering)
        {
            MoveTowardsTarget(BuildPoint(centerViewportX), OnReachedCenter);
            return;
        }

        if (currentState == MovementState.Exiting)
        {
            MoveTowardsTarget(BuildPoint(endViewportX), OnReachedExit);
        }
    }

    private void HandleNextNpc()
    {
        if (!enabled || currentState == MovementState.Entering)
        {
            return;
        }

        CacheMovementPlane();
        transform.position = BuildPoint(startViewportX);
        currentState = MovementState.Entering;
        SetMovingAnimation(true);
    }

    private void HandleEndNpc()
    {
        if (!enabled || currentState != MovementState.WaitingAtCenter)
        {
            return;
        }

        currentState = MovementState.Exiting;
        SetMovingAnimation(true);
    }

    private void OnReachedCenter()
    {
        currentState = MovementState.WaitingAtCenter;
        SetMovingAnimation(false);
        onArrivedCenter?.Invoke();
        NpcMovementEvents.RaiseNpcArrivedCenter();
    }

    private void OnReachedExit()
    {
        currentState = MovementState.Idle;
        SetMovingAnimation(false);
    }

    private void MoveTowardsTarget(Vector3 target, UnityAction onReachedTarget)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if ((transform.position - target).sqrMagnitude > ArriveThresholdSqr)
        {
            return;
        }

        transform.position = target;
        onReachedTarget?.Invoke();
    }

    private void CacheMovementPlane()
    {
        fixedY = transform.position.y;
        distanceFromCamera = Vector3.Dot(
            transform.position - targetCamera.transform.position,
            targetCamera.transform.forward
        );

        if (distanceFromCamera <= 0f)
        {
            distanceFromCamera = targetCamera.nearClipPlane + 0.1f;
        }
    }

    private Vector3 BuildPoint(float viewportX)
    {
        Vector3 viewportPoint = new Vector3(viewportX, 0.5f, distanceFromCamera);
        Vector3 worldPoint = targetCamera.ViewportToWorldPoint(viewportPoint);
        worldPoint.y = fixedY;
        worldPoint.z = transform.position.z;
        return worldPoint;
    }

    private void CacheAnimatorSetup()
    {
        if (npcAnimator == null)
        {
            npcAnimator = GetComponent<Animator>();
        }

        if (npcAnimator == null)
        {
            npcAnimator = GetComponentInChildren<Animator>(true);
        }

        if (npcAnimator == null)
        {
            Debug.LogWarning("NpcMovementController nao encontrou Animator no objeto nem nos filhos.", this);
            return;
        }

        hasMovingBoolParameter = TryResolveBoolParameterName(movingBoolParameter, out string resolvedParameterName);
        if (hasMovingBoolParameter)
        {
            movingBoolParameter = resolvedParameterName;
            movingBoolHash = Animator.StringToHash(movingBoolParameter);
            return;
        }

        Debug.LogWarning($"Parametro bool '{movingBoolParameter}' nao encontrado no Animator do NPC.", this);
    }

    private bool TryResolveBoolParameterName(string parameterName, out string resolvedParameterName)
    {
        resolvedParameterName = string.Empty;

        if (npcAnimator == null || string.IsNullOrWhiteSpace(parameterName))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in npcAnimator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool && parameter.name == parameterName)
            {
                resolvedParameterName = parameter.name;
                return true;
            }
        }

        foreach (AnimatorControllerParameter parameter in npcAnimator.parameters)
        {
            if (
                parameter.type == AnimatorControllerParameterType.Bool &&
                string.Equals(parameter.name, parameterName, StringComparison.OrdinalIgnoreCase)
            )
            {
                resolvedParameterName = parameter.name;
                return true;
            }
        }

        return false;
    }

    private void SetMovingAnimation(bool isMoving)
    {
        if (npcAnimator == null || !hasMovingBoolParameter)
        {
            return;
        }

        npcAnimator.SetBool(movingBoolHash, isMoving);
    }
}
