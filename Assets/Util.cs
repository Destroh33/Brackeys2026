using UnityEngine;

public static class Util
{
    public static bool Contains(this LayerMask layerMask, int layer)
    {
        return ((1 << layer) & layerMask.value) != 0;
    }

    public static Vector3 GetRandomDirectionInCone(float maxAngleRad)
    {
        var point = Random.insideUnitCircle * Mathf.Tan(maxAngleRad);
        return new Vector3(point.x, point.y, 1.0f).normalized;
    }
}
