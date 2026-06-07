using UnityEngine;

public class DocumentSpawnPointMarker : MonoBehaviour
{
    [SerializeField] private Color markerColor = new Color(0.1f, 0.8f, 1f, 0.9f);
    [SerializeField] private float crossSize = 20f;

    private void OnDrawGizmos()
    {
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            return;
        }

        Gizmos.color = markerColor;

        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        for (var index = 0; index < corners.Length; index++)
        {
            var nextIndex = (index + 1) % corners.Length;
            Gizmos.DrawLine(corners[index], corners[nextIndex]);
        }

        var center = rectTransform.position;
        Gizmos.DrawLine(center + Vector3.left * crossSize, center + Vector3.right * crossSize);
        Gizmos.DrawLine(center + Vector3.down * crossSize, center + Vector3.up * crossSize);
    }
}
