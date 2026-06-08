using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
    [Tooltip("Image usada quando o NPC fica dentro do Canvas da cena Game.")]
    [SerializeField] private Image npcImage;
    [SerializeField] private RectTransform npcRectTransform;
    [Tooltip("Visual usado quando a cena/teste chama o NPC direto, sem passar por um caso da fila.")]
    [SerializeField] private NpcDefinition defaultNpcDefinition;

    [Header("Render Order")]
    [Tooltip("Quando o NPC e uma Image dentro do Canvas, controla a ordem pela hierarquia do Canvas.")]
    [SerializeField] private bool applyCanvasSiblingIndex = true;
    [Tooltip("0 fica atras de tudo no Canvas. Na cena Game, 1 deixa o NPC acima do fundo e abaixo da mesa/UI.")]
    [SerializeField, Min(0)] private int canvasSiblingIndex = 1;
    [Tooltip("Use apenas quando o NPC for SpriteRenderer fora do Canvas.")]
    [SerializeField] private bool applySpriteSortingOrder;
    [SerializeField] private string spriteSortingLayerName;
    [SerializeField] private int spriteSortingOrder;

    [Header("Speed")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 2.5f;

    [Header("Step Bounce")]
    [SerializeField] private bool enableStepBounce = true;
    [Tooltip("Altura do passo quando o NPC e uma Image dentro do Canvas, em pixels.")]
    [SerializeField, Min(0f)] private float canvasStepAmplitude = 10f;
    [Tooltip("Altura do passo quando o NPC e SpriteRenderer no mundo, em unidades de mundo.")]
    [SerializeField, Min(0f)] private float worldStepAmplitude = 0.08f;
    [Tooltip("Quantidade de sobe/desce por segundo enquanto o NPC anda.")]
    [SerializeField, Min(0.1f)] private float stepFrequency = 3f;

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
    private RectTransform npcParentRectTransform;
    private float fixedAnchoredY;
    private bool usesCanvasMovement;
    private float stepAnimationTime;

    private void Awake()
    {
        CacheVisualReferences();

        if (!usesCanvasMovement && targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (!usesCanvasMovement && targetCamera == null)
        {
            Debug.LogError("NpcMovementController precisa de uma Camera para converter viewport em mundo.", this);
            enabled = false;
            return;
        }

        ApplyDefaultNpcDefinitionIfConfigured();
        CacheMovementPlane();
        CacheFallbackSprites();
        CacheAnimatorSetup();
        ApplyRenderOrder();
        SetSidePose(false);
    }

    private void OnValidate()
    {
        CacheVisualReferences();
        ApplyDefaultNpcDefinitionIfConfigured();

        if (npcSpriteRenderer != null && sideSprite != null)
        {
            npcSpriteRenderer.sprite = sideSprite;
        }

        if (npcImage != null && sideSprite != null)
        {
            npcImage.sprite = sideSprite;
        }

        ApplyRenderOrder();
    }

    public void ApplyNpcDefinition(NpcDefinition npcDefinition)
    {
        CacheVisualReferences();
        ApplyRenderOrder();

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
            MoveTowardsActiveTarget(centerViewportX, OnReachedCenter);
            return;
        }

        if (currentState == MovementState.Exiting)
        {
            MoveTowardsActiveTarget(endViewportX, OnReachedExit);
        }
    }

    private void HandleNextNpc()
    {
        if (!enabled || currentState == MovementState.Entering)
        {
            return;
        }

        CacheMovementPlane();
        ApplyRenderOrder();

        if (usesCanvasMovement)
        {
            npcRectTransform.anchoredPosition = BuildAnchoredPoint(startViewportX);
        }
        else
        {
            transform.position = BuildPoint(startViewportX);
        }

        currentState = MovementState.Entering;
        stepAnimationTime = 0f;
        SetSidePose(true);
    }

    private void HandleEndNpc()
    {
        if (!enabled)
        {
            return;
        }

        if (currentState == MovementState.Idle)
        {
            NpcMovementEvents.RaiseNpcExited();
            return;
        }

        if (currentState == MovementState.Exiting)
        {
            return;
        }

        currentState = MovementState.Exiting;
        stepAnimationTime = 0f;
        SetSidePose(true);
    }

    private void OnReachedCenter()
    {
        currentState = MovementState.WaitingAtCenter;
        ResetStepOffset();
        SetFrontPose();
        onArrivedCenter?.Invoke();
        NpcMovementEvents.RaiseNpcArrivedCenter();
    }

    private void OnReachedExit()
    {
        currentState = MovementState.Idle;
        ResetStepOffset();
        SetSidePose(false);
        NpcMovementEvents.RaiseNpcExited();
    }

    private void MoveTowardsTarget(Vector3 target, UnityAction onReachedTarget)
    {
        var currentBasePosition = transform.position;
        currentBasePosition.y = fixedY;

        var nextBasePosition = Vector3.MoveTowards(
            currentBasePosition,
            target,
            moveSpeed * Time.deltaTime
        );

        if ((nextBasePosition - target).sqrMagnitude > ArriveThresholdSqr)
        {
            nextBasePosition.y = fixedY + GetStepOffset(worldStepAmplitude);
            transform.position = nextBasePosition;
            return;
        }

        transform.position = target;
        onReachedTarget?.Invoke();
    }

    private void MoveTowardsCanvasTarget(Vector2 target, UnityAction onReachedTarget)
    {
        var currentBasePosition = npcRectTransform.anchoredPosition;
        currentBasePosition.y = fixedAnchoredY;

        var nextBasePosition = Vector2.MoveTowards(
            currentBasePosition,
            target,
            moveSpeed * 100f * Time.deltaTime
        );

        if ((nextBasePosition - target).sqrMagnitude > ArriveThresholdSqr)
        {
            nextBasePosition.y = fixedAnchoredY + GetStepOffset(canvasStepAmplitude);
            npcRectTransform.anchoredPosition = nextBasePosition;
            return;
        }

        npcRectTransform.anchoredPosition = target;
        onReachedTarget?.Invoke();
    }

    private void MoveTowardsActiveTarget(float viewportX, UnityAction onReachedTarget)
    {
        if (usesCanvasMovement)
        {
            MoveTowardsCanvasTarget(BuildAnchoredPoint(viewportX), onReachedTarget);
            return;
        }

        MoveTowardsTarget(BuildPoint(viewportX), onReachedTarget);
    }

    private void CacheMovementPlane()
    {
        if (usesCanvasMovement)
        {
            fixedAnchoredY = npcRectTransform.anchoredPosition.y;
            npcParentRectTransform = npcRectTransform.parent as RectTransform;
            return;
        }

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

    private Vector2 BuildAnchoredPoint(float viewportX)
    {
        var parentWidth = npcParentRectTransform != null
            ? npcParentRectTransform.rect.width
            : Screen.width;

        var parentPivotX = npcParentRectTransform != null
            ? npcParentRectTransform.pivot.x
            : 0.5f;

        if (parentWidth <= 0f)
        {
            parentWidth = Screen.width;
        }

        return new Vector2((viewportX - parentPivotX) * parentWidth, fixedAnchoredY);
    }

    private float GetStepOffset(float amplitude)
    {
        if (!enableStepBounce || amplitude <= 0f)
        {
            return 0f;
        }

        stepAnimationTime += Time.deltaTime;
        return Mathf.Sin(stepAnimationTime * stepFrequency * Mathf.PI * 2f) * amplitude;
    }

    private void ResetStepOffset()
    {
        stepAnimationTime = 0f;

        if (usesCanvasMovement && npcRectTransform != null)
        {
            var position = npcRectTransform.anchoredPosition;
            position.y = fixedAnchoredY;
            npcRectTransform.anchoredPosition = position;
            return;
        }

        var worldPosition = transform.position;
        worldPosition.y = fixedY;
        transform.position = worldPosition;
    }

    private void CacheVisualReferences()
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

        if (npcImage == null)
        {
            npcImage = GetComponent<Image>();
        }

        if (npcImage == null)
        {
            npcImage = GetComponentInChildren<Image>(true);
        }

        if (npcRectTransform == null)
        {
            npcRectTransform = GetComponent<RectTransform>();
        }

        usesCanvasMovement = npcImage != null && npcRectTransform != null;

        if (sideSprite == null && npcImage != null)
        {
            sideSprite = npcImage.sprite;
        }
    }

    private void ApplyRenderOrder()
    {
        if (usesCanvasMovement)
        {
            ApplyCanvasRenderOrder();
            return;
        }

        ApplySpriteRenderOrder();
    }

    private void ApplyCanvasRenderOrder()
    {
        if (!applyCanvasSiblingIndex || npcRectTransform == null || npcRectTransform.parent == null)
        {
            return;
        }

        var maxSiblingIndex = Mathf.Max(0, npcRectTransform.parent.childCount - 1);
        npcRectTransform.SetSiblingIndex(Mathf.Clamp(canvasSiblingIndex, 0, maxSiblingIndex));
    }

    private void ApplySpriteRenderOrder()
    {
        if (!applySpriteSortingOrder || npcSpriteRenderer == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(spriteSortingLayerName))
        {
            npcSpriteRenderer.sortingLayerName = spriteSortingLayerName;
        }

        npcSpriteRenderer.sortingOrder = spriteSortingOrder;
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
        if (sprite == null)
        {
            return;
        }

        if (npcSpriteRenderer != null)
        {
            npcSpriteRenderer.sprite = sprite;
        }

        if (npcImage != null)
        {
            npcImage.sprite = sprite;
        }
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
        return (npcSpriteRenderer != null || npcImage != null) && frontSprite != null;
    }
}
