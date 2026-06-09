using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class DocumentField
{
    public string key;
    public string value;

    public DocumentField Clone()
    {
        return new DocumentField
        {
            key = key,
            value = value
        };
    }
}

[Serializable]
public class DocumentRecord
{
    public DocumentDefinition definition;
    public List<DocumentField> fields = new List<DocumentField>();
    public bool hasValidStamp = true;
    public bool isSuspicious;
    public bool isFake;
    public string issueDateIso;
    public string expiryDateIso;
    public string notes;

    public DocumentRecord Clone()
    {
        var clone = new DocumentRecord
        {
            definition = definition,
            hasValidStamp = hasValidStamp,
            isSuspicious = isSuspicious,
            isFake = isFake,
            issueDateIso = issueDateIso,
            expiryDateIso = expiryDateIso,
            notes = notes
        };

        if (fields != null)
        {
            clone.fields = fields.Select(field => field != null ? field.Clone() : null).Where(field => field != null).ToList();
        }

        return clone;
    }

    public DocumentType GetDocumentType()
    {
        return definition != null ? definition.documentType : DocumentType.Unknown;
    }

    public string GetDisplayName()
    {
        if (definition != null && !string.IsNullOrWhiteSpace(definition.displayName))
        {
            return definition.displayName;
        }

        return "Documento";
    }

    public bool TryGetFieldValue(string key, out string value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(key) || fields == null)
        {
            return false;
        }

        for (var index = 0; index < fields.Count; index++)
        {
            var field = fields[index];
            if (field == null || string.IsNullOrWhiteSpace(field.key))
            {
                continue;
            }

            if (string.Equals(field.key.Trim(), key.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                value = field.value;
                return true;
            }
        }

        return false;
    }

    public bool HasField(string key)
    {
        return TryGetFieldValue(key, out _);
    }

    public string BuildSummary()
    {
        var builder = new StringBuilder();

        builder.AppendLine(GetDisplayName());

        if (definition != null && !string.IsNullOrWhiteSpace(definition.description))
        {
            builder.AppendLine(definition.description);
        }

        if (fields != null && fields.Count > 0)
        {
            for (var index = 0; index < fields.Count; index++)
            {
                var field = fields[index];
                if (field == null || string.IsNullOrWhiteSpace(field.key))
                {
                    continue;
                }

                builder.AppendLine($"{field.key}: {field.value}");
            }
        }

        if (!string.IsNullOrWhiteSpace(issueDateIso))
        {
            builder.AppendLine($"Emissao: {issueDateIso}");
        }

        if (!string.IsNullOrWhiteSpace(expiryDateIso))
        {
            builder.AppendLine($"Validade: {expiryDateIso}");
        }

        builder.AppendLine(hasValidStamp ? "Carimbo: valido" : "Carimbo: invalido");

        if (isSuspicious)
        {
            builder.AppendLine("Estado: suspeito");
        }

        if (isFake)
        {
            builder.AppendLine("Estado: falso");
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            builder.AppendLine(notes);
        }

        return builder.ToString();
    }

    public bool IsExpired(DateTime referenceDate)
    {
        if (string.IsNullOrWhiteSpace(expiryDateIso))
        {
            return false;
        }

        if (DateUtility.TryParseFlexibleDate(expiryDateIso, out var parsedExpiry))
        {
            return parsedExpiry.Date < referenceDate.Date;
        }

        return true;
    }
}

[Serializable]
public class DocumentRequirement
{
    public DocumentType documentType = DocumentType.Unknown;
    public bool required = true;
    public bool requiresValidStamp;
    public bool requiresNonExpiredDate;
    public List<string> requiredFieldKeys = new List<string>();
}

[Serializable]
public class DocumentComparisonRule
{
    public DocumentType firstDocumentType = DocumentType.Unknown;
    public string firstFieldKey;
    public DocumentType secondDocumentType = DocumentType.Unknown;
    public string secondFieldKey;

    [TextArea(2, 4)]
    public string description;
}

[Serializable]
public class DocumentExpectedFieldValueRule
{
    public DocumentType documentType = DocumentType.Unknown;
    public string fieldKey;
    public string expectedValue;

    [TextArea(2, 4)]
    public string description;
}
