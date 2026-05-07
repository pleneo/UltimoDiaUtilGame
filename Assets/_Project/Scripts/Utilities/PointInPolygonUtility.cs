using System.Collections.Generic;
using UnityEngine;

public static class PointInPolygonUtility
{
    public static bool IsPointInsidePolygon(Vector2 point, IReadOnlyList<Vector2> polygon)
    {
        if (polygon == null || polygon.Count < 3)
        {
            return false;
        }

        var inside = false;
        var previousIndex = polygon.Count - 1;

        for (var currentIndex = 0; currentIndex < polygon.Count; currentIndex++)
        {
            var currentPoint = polygon[currentIndex];
            var previousPoint = polygon[previousIndex];

            var intersects = (currentPoint.y > point.y) != (previousPoint.y > point.y) &&
                             point.x < (previousPoint.x - currentPoint.x) * (point.y - currentPoint.y) /
                             ((previousPoint.y - currentPoint.y) == 0f ? Mathf.Epsilon : (previousPoint.y - currentPoint.y)) + currentPoint.x;

            if (intersects)
            {
                inside = !inside;
            }

            previousIndex = currentIndex;
        }

        return inside;
    }
}
