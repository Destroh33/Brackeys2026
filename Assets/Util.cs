using UnityEngine;

public static class Util
{
    public static bool Contains(this LayerMask layerMask, int layer)
    {
        return ((1 << layer) & layerMask.value) != 0;
    }
}
