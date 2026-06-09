using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DocumentPrefabMapping
{
    public DocumentType documentType = DocumentType.Unknown;
    public DraggableDocument prefab;
}

[Serializable]
public class DocumentSpawnPointMapping
{
    public DocumentType documentType = DocumentType.Unknown;
    public RectTransform spawnPoint;
}

public class DocumentManager : MonoBehaviour
{
    [SerializeField] private Transform documentParent;
    [SerializeField] private DraggableDocument fallbackDocumentPrefab;
    [SerializeField] private List<DocumentPrefabMapping> documentPrefabs = new List<DocumentPrefabMapping>();
    [Tooltip("Pontos visuais por tipo de documento. Use esta lista para manter cada tipo sempre no mesmo lugar.")]
    [SerializeField] private List<DocumentSpawnPointMapping> documentSpawnPoints = new List<DocumentSpawnPointMapping>();
    [Tooltip("Pontos visuais por ordem de documento. Usado somente se nao existir um ponto por tipo.")]
    [SerializeField] private List<RectTransform> spawnPoints = new List<RectTransform>();
    [Tooltip("Posicoes antigas por ordem. Usado somente se nao existir ponto visual configurado.")]
    [SerializeField] private List<Vector2> spawnPositions = new List<Vector2>();
    [Header("Spawn Animation")]
    [SerializeField] private bool animateDocumentEntry = true;
    [SerializeField, Min(0f)] private float entryStaggerSeconds = 0.34f;
    [SerializeField, Min(0.05f)] private float entryDurationSeconds = 0.42f;
    [SerializeField] private Vector2 entryOffset = new Vector2(0f, 260f);
    [SerializeField] private float entryStartRotationZ = -5f;
    [SerializeField] private float entryEndScale = 1f;
    [SerializeField] private float entryStartScale = 0.97f;
    [Header("Visual Order")]
    [SerializeField] private bool randomizeDocumentOrderPerCase = true;

    private readonly List<DocumentRecord> currentDocuments = new List<DocumentRecord>();
    private readonly List<DraggableDocument> spawnedViews = new List<DraggableDocument>();
    private readonly List<Coroutine> runningAnimations = new List<Coroutine>();
    private RectTransform resolvedDocumentParentRect;

    public event Action DocumentsChanged;

    public IReadOnlyList<DocumentRecord> CurrentDocuments => currentDocuments;
    public IReadOnlyList<DraggableDocument> SpawnedViews => spawnedViews;

    private void Awake()
    {
        ResolveDocumentParentIfNeeded();
    }

    public void LoadCase(StudentCaseDefinition caseDefinition)
    {
        ClearDocuments();

        if (caseDefinition == null || caseDefinition.documents == null)
        {
            return;
        }

        var documentsToSpawn = new List<DocumentRecord>();

        for (var index = 0; index < caseDefinition.documents.Count; index++)
        {
            var sourceRecord = caseDefinition.documents[index];
            if (sourceRecord == null)
            {
                continue;
            }

            var clonedRecord = sourceRecord.Clone();
            documentsToSpawn.Add(clonedRecord);
        }

        if (randomizeDocumentOrderPerCase)
        {
            ShuffleDocuments(documentsToSpawn);
        }

        for (var index = 0; index < documentsToSpawn.Count; index++)
        {
            var record = documentsToSpawn[index];
            currentDocuments.Add(record);
            SpawnDocumentView(record, index);
        }

        if (currentDocuments.Count == 0)
        {
            Debug.LogWarning($"Caso '{caseDefinition.caseId}' carregou, mas nao possui documentos configurados.", this);
        }

        DocumentsChanged?.Invoke();
    }

    public DraggableDocument AddDocument(DocumentRecord record, bool animated = true)
    {
        if (record == null)
        {
            return null;
        }

        currentDocuments.Add(record);
        var spawnedView = SpawnDocumentView(record, currentDocuments.Count - 1, animated);
        DocumentsChanged?.Invoke();
        return spawnedView;
    }

    public void ClearDocuments()
    {
        StopRunningAnimations();
        currentDocuments.Clear();

        for (var index = 0; index < spawnedViews.Count; index++)
        {
            var view = spawnedViews[index];
            if (view != null)
            {
                Destroy(view.gameObject);
            }
        }

        spawnedViews.Clear();
        DocumentsChanged?.Invoke();
    }

    private DraggableDocument SpawnDocumentView(DocumentRecord record, int index, bool animated = true)
    {
        if (record == null)
        {
            return null;
        }

        ResolveDocumentParentIfNeeded();

        DraggableDocument view = null;
        var prefab = ResolvePrefab(record.GetDocumentType());
        var parentTransform = GetDocumentParentTransform();

        if (prefab != null)
        {
            view = Instantiate(prefab, parentTransform, false);
        }
        else
        {
            var documentObject = new GameObject("Document", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(DraggableDocument));
            documentObject.transform.SetParent(parentTransform, false);
            ConfigureGeneratedDocumentObject(documentObject);
            view = documentObject.GetComponent<DraggableDocument>();
        }

        if (view != null)
        {
            ApplySpawnPosition(view, index, record.GetDocumentType());
            view.Bind(record);
            spawnedViews.Add(view);
            StartEntryAnimation(view, index, animated);
        }

        return view;
    }

    private void StartEntryAnimation(DraggableDocument view, int index, bool animated)
    {
        if (view == null)
        {
            return;
        }

        if (!animateDocumentEntry || !animated)
        {
            view.SetInteractionEnabled(true);
            view.SetVisualAlpha(1f);
            return;
        }

        view.SetInteractionEnabled(false);
        view.SetVisualAlpha(0f);

        Coroutine animation = null;
        animation = StartCoroutine(AnimateDocumentEntry(view, index, () => runningAnimations.Remove(animation)));
        runningAnimations.Add(animation);
    }

    private IEnumerator AnimateDocumentEntry(DraggableDocument view, int index, Action onCompleted)
    {
        if (view == null)
        {
            onCompleted?.Invoke();
            yield break;
        }

        if (entryStaggerSeconds > 0f && index > 0)
        {
            yield return new WaitForSeconds(entryStaggerSeconds * index);
        }

        var rectTransform = view.RectTransform;
        var targetPosition = rectTransform.anchoredPosition;
        var targetRotation = rectTransform.localRotation;
        var targetScale = Vector3.one * entryEndScale;
        var startPosition = targetPosition + entryOffset;
        var startRotation = Quaternion.Euler(0f, 0f, entryStartRotationZ);
        var startScale = Vector3.one * entryStartScale;

        rectTransform.anchoredPosition = startPosition;
        rectTransform.localRotation = startRotation;
        rectTransform.localScale = startScale;

        var elapsed = 0f;
        while (elapsed < entryDurationSeconds)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / entryDurationSeconds);
            var eased = EaseInOutCubic(t);

            rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, eased);
            rectTransform.localRotation = Quaternion.LerpUnclamped(startRotation, targetRotation, eased);
            rectTransform.localScale = Vector3.LerpUnclamped(startScale, targetScale, eased);
            view.SetVisualAlpha(eased);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        rectTransform.localRotation = targetRotation;
        rectTransform.localScale = targetScale;
        view.SetVisualAlpha(1f);
        view.SetInteractionEnabled(true);
        onCompleted?.Invoke();
    }

    private void StopRunningAnimations()
    {
        for (var index = 0; index < runningAnimations.Count; index++)
        {
            var animation = runningAnimations[index];
            if (animation != null)
            {
                StopCoroutine(animation);
            }
        }

        runningAnimations.Clear();
    }

    private static float EaseInOutCubic(float value)
    {
        return value < 0.5f
            ? 4f * value * value * value
            : 1f - Mathf.Pow(-2f * value + 2f, 3f) / 2f;
    }

    private static void ShuffleDocuments(List<DocumentRecord> documents)
    {
        if (documents == null || documents.Count <= 1)
        {
            return;
        }

        for (var index = documents.Count - 1; index > 0; index--)
        {
            var randomIndex = UnityEngine.Random.Range(0, index + 1);
            (documents[index], documents[randomIndex]) = (documents[randomIndex], documents[index]);
        }
    }

    private void ResolveDocumentParentIfNeeded()
    {
        if (resolvedDocumentParentRect != null)
        {
            return;
        }

        var documentSpawnRoot = GameObject.Find("DocumentSpawnRoot");
        if (documentSpawnRoot != null)
        {
            documentParent = documentSpawnRoot.transform;
        }

        resolvedDocumentParentRect = documentParent as RectTransform;
        if (resolvedDocumentParentRect == null && documentParent != null)
        {
            resolvedDocumentParentRect = documentParent.parent as RectTransform;
        }

        if (resolvedDocumentParentRect == null)
        {
            resolvedDocumentParentRect = GetComponentInParent<Canvas>() != null
                ? GetComponentInParent<Canvas>().transform as RectTransform
                : null;
        }

        if (resolvedDocumentParentRect == null)
        {
            Debug.LogWarning("DocumentManager nao encontrou um RectTransform valido para spawn de documentos. Os documentos podem aparecer desalinhados.", this);
        }
    }

    private static void ConfigureGeneratedDocumentObject(GameObject documentObject)
    {
        var rectTransform = documentObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(220f, 300f);
        rectTransform.anchoredPosition = Vector2.zero;

        var image = documentObject.GetComponent<Image>();
        image.color = new Color(0.98f, 0.96f, 0.82f, 1f);
    }

    private DraggableDocument ResolvePrefab(DocumentType documentType)
    {
        if (documentPrefabs != null)
        {
            for (var index = 0; index < documentPrefabs.Count; index++)
            {
                var mapping = documentPrefabs[index];
                if (mapping == null || mapping.prefab == null)
                {
                    continue;
                }

                if (mapping.documentType == documentType)
                {
                    return mapping.prefab;
                }
            }
        }

        return fallbackDocumentPrefab;
    }

    private void ApplySpawnPosition(DraggableDocument view, int index, DocumentType documentType)
    {
        if (view == null)
        {
            return;
        }

        var rectTransform = view.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            return;
        }

        if (TryApplyTypedSpawnPoint(rectTransform, documentType))
        {
            return;
        }

        if (TryApplySpawnPoint(rectTransform, index))
        {
            return;
        }

        rectTransform.anchoredPosition = ResolveSpawnPosition(index);
    }

    private bool TryApplyTypedSpawnPoint(RectTransform documentRectTransform, DocumentType documentType)
    {
        if (documentSpawnPoints == null)
        {
            return false;
        }

        for (var index = 0; index < documentSpawnPoints.Count; index++)
        {
            var mapping = documentSpawnPoints[index];
            if (mapping == null || mapping.spawnPoint == null)
            {
                continue;
            }

            if (!mapping.spawnPoint.gameObject.scene.IsValid())
            {
                continue;
            }

            if (mapping.documentType != documentType)
            {
                continue;
            }

            ApplySpawnPointTransform(documentRectTransform, mapping.spawnPoint);
            return true;
        }

        return false;
    }

    private bool TryApplySpawnPoint(RectTransform documentRectTransform, int index)
    {
        if (spawnPoints == null || index < 0 || index >= spawnPoints.Count)
        {
            return false;
        }

        var spawnPoint = spawnPoints[index];
        if (spawnPoint == null)
        {
            return false;
        }

        if (!spawnPoint.gameObject.scene.IsValid())
        {
            return false;
        }

        ApplySpawnPointTransform(documentRectTransform, spawnPoint);
        return true;
    }

    private static void ApplySpawnPointTransform(RectTransform documentRectTransform, RectTransform spawnPoint)
    {
        if (documentRectTransform == null || spawnPoint == null)
        {
            return;
        }

        var parentRect = documentRectTransform.parent as RectTransform;
        if (parentRect != null)
        {
            var canvas = parentRect.GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, RectTransformUtility.WorldToScreenPoint(camera, spawnPoint.position), camera, out var localPoint))
            {
                documentRectTransform.anchoredPosition = localPoint;
            }
            else
            {
                documentRectTransform.anchoredPosition = spawnPoint.anchoredPosition;
            }
        }
        else
        {
            documentRectTransform.position = spawnPoint.position;
        }

        documentRectTransform.rotation = spawnPoint.rotation;
    }

    private Vector2 ResolveSpawnPosition(int index)
    {
        if (spawnPositions != null && index >= 0 && index < spawnPositions.Count)
        {
            return spawnPositions[index];
        }

        if (spawnPositions != null && spawnPositions.Count > 0)
        {
            var lastPosition = spawnPositions[spawnPositions.Count - 1];
            var extraIndex = Mathf.Max(0, index - spawnPositions.Count + 1);
            return lastPosition + new Vector2(24f * extraIndex, -24f * extraIndex);
        }

        return new Vector2(32f * index, -32f * index);
    }

    private Transform GetDocumentParentTransform()
    {
        if (resolvedDocumentParentRect != null)
        {
            return resolvedDocumentParentRect;
        }

        if (documentParent != null)
        {
            return documentParent;
        }

        return transform;
    }
}
