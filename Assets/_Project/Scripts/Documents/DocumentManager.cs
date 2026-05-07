using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DocumentManager : MonoBehaviour
{
    [SerializeField] private Transform documentParent;
    [SerializeField] private DraggableDocument documentPrefab;

    private readonly List<DocumentRecord> currentDocuments = new List<DocumentRecord>();
    private readonly List<DraggableDocument> spawnedViews = new List<DraggableDocument>();

    public IReadOnlyList<DocumentRecord> CurrentDocuments => currentDocuments;

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
            SpawnDocumentView(clonedRecord);
        }
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
    }

    private void SpawnDocumentView(DocumentRecord record)
    {
        if (record == null)
        {
            return;
        }

        DraggableDocument view = null;

        if (documentPrefab != null)
        {
            view = Instantiate(documentPrefab, documentParent != null ? documentParent : transform);
        }
        else
        {
            var documentObject = new GameObject("Document", typeof(RectTransform), typeof(CanvasGroup), typeof(DraggableDocument));
            documentObject.transform.SetParent(documentParent != null ? documentParent : transform, false);
            view = documentObject.GetComponent<DraggableDocument>();
        }

        if (view != null)
        {
            view.Bind(record);
            spawnedViews.Add(view);
        }
    }
}
