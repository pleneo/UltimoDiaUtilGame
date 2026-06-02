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
    [Tooltip("SpriteRenderer que mostra o NPC na cena. Pode deixar vazio se ele estiver no mesmo objeto ou em um filho.")]
    [SerializeField] private SpriteRenderer npcSpriteRenderer;
    [Tooltip("Visual usado quando a cena/teste chama o NPC direto, sem passar por um caso da fila.")]
    [SerializeField] private NpcDefinition defaultNpcDefinition;

    [Header("Speed")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 2.5f;

    [Header("Viewport Positions")]
    [SerializeField] private float startViewportX = -0.15f;
    [SerializeField] private float centerViewportX = 0.5f;
    [SerializeField] private float endViewportX = 1.15f;

    [Header("Behavior")]
    [SerializeField] private UnityEvent onArrivedCenter;

    [Header("Static Sprites")]
    [Tooltip("Sprite de lado/perfil usado enquanto o NPC entra e sai.")]
    [SerializeField] private Sprite sideSprite;
    [Tooltip("Sprite de frente usado quando o NPC para no meio da tela.")]
    [SerializeField] private Sprite frontSprite;
    [Tooltip("Desliga o Animator antigo quando Front Sprite está configurado, evitando que a animação sobrescreva os sprites estáticos.")]
    [SerializeField] private bool disableAnimatorWhenUsingStaticSprites = true;
    
    [Header("Animation Parameters")]
    [SerializeField] private string movingBoolParameter = "IsMoving";

    private const float ArriveThresholdSqr = 0.0001f;
    private MovementState currentState = MovementState.Idle;
    private float fixedY;
    private float distanceFromCamera;
    private bool hasMovingBoolParameter;
    private int movingBoolHash;
    private Sprite fallbackSideSprite;
    private Sprite fallbackFrontSprite;

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
        CacheSpriteRendererSetup();
        ApplyDefaultNpcDefinitionIfConfigured();
        CacheFallbackSprites();
        CacheAnimatorSetup();
        SetSidePose(false);
    }

    private void OnValidate()
    {
        CacheSpriteRendererSetup();
        ApplyDefaultNpcDefinitionIfConfigured();

        if (npcSpriteRenderer != null && sideSprite != null)
        {
            npcSpriteRenderer.sprite = sideSprite;
        }
    }

    public void ApplyNpcDefinition(NpcDefinition npcDefinition)
    {
        CacheSpriteRendererSetup();

        sideSprite = npcDefinition != null && npcDefinition.sideSprite != null
            ? npcDefinition.sideSprite
            : fallbackSideSprite;

        frontSprite = npcDefinition != null && npcDefinition.frontSprite != null
            ? npcDefinition.frontSprite
            : fallbackFrontSprite;

        DisableAnimatorForStaticSpritesIfNeeded();
        SetSidePose(false);
    }

    private void ApplyDefaultNpcDefinitionIfConfigured()
    {
        if (defaultNpcDefinition == null)
        {
            return;
        }

        if (defaultNpcDefinition.sideSprite != null)
        {
            sideSprite = defaultNpcDefinition.sideSprite;
        }

        if (defaultNpcDefinition.frontSprite != null)
        {
            frontSprite = defaultNpcDefinition.frontSprite;
        }
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
        SetSidePose(true);
    }

    private void HandleEndNpc()
    {
        if (!enabled || currentState != MovementState.WaitingAtCenter)
        {
            return;
        }

        currentState = MovementState.Exiting;
        SetSidePose(true);
    }

    private void OnReachedCenter()
    {
        currentState = MovementState.WaitingAtCenter;
        SetFrontPose();
        onArrivedCenter?.Invoke();
        NpcMovementEvents.RaiseNpcArrivedCenter();
    }

    private void OnReachedExit()
    {
        currentState = MovementState.Idle;
        SetSidePose(false);
        NpcMovementEvents.RaiseNpcExited();
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

    private void CacheSpriteRendererSetup()
    {
        if (npcSpriteRenderer == null)
        {
            npcSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (npcSpriteRenderer == null)
        {
            npcSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        if (sideSprite == null && npcSpriteRenderer != null)
        {
            sideSprite = npcSpriteRenderer.sprite;
        }
    }

    private void CacheFallbackSprites()
    {
        fallbackSideSprite = sideSprite;
        fallbackFrontSprite = frontSprite;
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
            if (!HasStaticSpriteSetup())
            {
                Debug.LogWarning("NpcMovementController nao encontrou Animator nem sprites estaticos configurados.", this);
            }

            return;
        }

        if (HasStaticSpriteSetup() && disableAnimatorWhenUsingStaticSprites)
        {
            DisableAnimatorForStaticSpritesIfNeeded();
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
        if (npcAnimator == null || !npcAnimator.enabled || !hasMovingBoolParameter)
        {
            return;
        }

        npcAnimator.SetBool(movingBoolHash, isMoving);
    }

    private void SetSidePose(bool isMoving)
    {
        SetMovingAnimation(isMoving);
        SetSprite(sideSprite);
    }

    private void SetFrontPose()
    {
        SetMovingAnimation(false);
        SetSprite(frontSprite);
    }

    private void SetSprite(Sprite sprite)
    {
        if (npcSpriteRenderer == null || sprite == null)
        {
            return;
        }

        npcSpriteRenderer.sprite = sprite;
    }

    private void DisableAnimatorForStaticSpritesIfNeeded()
    {
        if (npcAnimator == null || !disableAnimatorWhenUsingStaticSprites || !HasStaticSpriteSetup())
        {
            return;
        }

        npcAnimator.enabled = false;
    }

    private bool HasStaticSpriteSetup()
    {
        return npcSpriteRenderer != null && frontSprite != null;
    }
}
