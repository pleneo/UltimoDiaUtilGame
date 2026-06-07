using System;
using System.Collections.Generic;
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

    private readonly List<DocumentRecord> currentDocuments = new List<DocumentRecord>();
    private readonly List<DraggableDocument> spawnedViews = new List<DraggableDocument>();

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

        for (var index = 0; index < caseDefinition.documents.Count; index++)
        {
            var sourceRecord = caseDefinition.documents[index];
            if (sourceRecord == null)
            {
                continue;
            }

            var clonedRecord = sourceRecord.Clone();
            currentDocuments.Add(clonedRecord);
            SpawnDocumentView(clonedRecord, currentDocuments.Count - 1);
        }

        if (currentDocuments.Count == 0)
        {
            Debug.LogWarning($"Caso '{caseDefinition.caseId}' carregou, mas nao possui documentos configurados.", this);
        }

        DocumentsChanged?.Invoke();
    }

    public void ClearDocuments()
    {
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

    private void SpawnDocumentView(DocumentRecord record, int index)
    {
        if (record == null)
        {
            return;
        }

        ResolveDocumentParentIfNeeded();

        DraggableDocument view = null;
        var prefab = ResolvePrefab(record.GetDocumentType());

        if (prefab != null)
        {
            view = Instantiate(prefab, documentParent != null ? documentParent : transform);
        }
        else
        {
            var documentObject = new GameObject("Document", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(DraggableDocument));
            documentObject.transform.SetParent(documentParent != null ? documentParent : transform, false);
            ConfigureGeneratedDocumentObject(documentObject);
            view = documentObject.GetComponent<DraggableDocument>();
        }

        if (view != null)
        {
            ApplySpawnPosition(view, index, record.GetDocumentType());
            view.Bind(record);
            spawnedViews.Add(view);
        }
    }

    private void ResolveDocumentParentIfNeeded()
    {
        if (documentParent != null)
        {
            return;
        }

        var documentSpawnRoot = GameObject.Find("DocumentSpawnRoot");
        if (documentSpawnRoot != null)
        {
            documentParent = documentSpawnRoot.transform;
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

        ApplySpawnPointTransform(documentRectTransform, spawnPoint);
        return true;
    }

    private static void ApplySpawnPointTransform(RectTransform documentRectTransform, RectTransform spawnPoint)
    {
        documentRectTransform.position = spawnPoint.position;
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
}
