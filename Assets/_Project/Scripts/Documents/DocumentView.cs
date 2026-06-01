using TMPro;
using UnityEngine;

public class DocumentView : MonoBehaviour
{
    [Header("Document Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text nameValueText;
    [SerializeField] private TMP_Text raValueText;
    [SerializeField] private TMP_Text courseValueText;
    [SerializeField] private TMP_Text notesText;

    [Header("Fallback")]
    [SerializeField] private string emptyValueText = "---";

    public void Bind(DocumentRecord record)
    {
        if (record == null)
        {
            SetText(titleText, "Documento");
            SetText(nameValueText, emptyValueText);
            SetText(raValueText, emptyValueText);
            SetText(courseValueText, emptyValueText);
            SetText(notesText, string.Empty);
            return;
        }

        SetText(titleText, record.GetDisplayName());
        SetFieldText(nameValueText, record, "nome");
        SetFieldText(raValueText, record, "ra");
        SetFieldText(courseValueText, record, "curso");
        SetText(notesText, string.IsNullOrWhiteSpace(record.notes) ? string.Empty : record.notes);
    }

    private void SetFieldText(TMP_Text target, DocumentRecord record, string fieldKey)
    {
        if (target == null)
        {
            return;
        }

        if (record != null &&
            record.TryGetFieldValue(fieldKey, out var value) &&
            !string.IsNullOrWhiteSpace(value))
        {
            target.text = value;
            return;
        }

        target.text = emptyValueText;
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target == null)
        {
            return;
        }

        target.text = value;
    }
}
